using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models.ViewModel;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bulky.Utility;
using Stripe.Checkout;
using Bulky.DataAccess.Repository;
using Microsoft.AspNetCore.Http;

[Area("Customer")]
public class CartController : Controller
{
    private readonly IUnitOfWork unitofwork;

    //no need of passing the cartvm object while posting the summarry, it will be binded automatically
    [BindProperty]
    private CartVM cartVM { get; set; }
    public CartController(IUnitOfWork u)
    {
        unitofwork = u;
    }

    [Area("Customer")]
    [Authorize]
    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        IEnumerable<ShoppingCart> cart = unitofwork.shoppingcart.GetAll(u => u.ApplicationUserId == userId,
            includeproperties: "Product");

        cartVM = new()
        {
            ShoppingCartList = unitofwork.shoppingcart.GetAll(u => u.ApplicationUserId == userId,
            includeproperties: "Product"),
            OrderHeader = new()
        };

        foreach (var i in cartVM.ShoppingCartList)
        {
            i.Price = CalculateCost(i);
            cartVM.OrderHeader.OrderTotal += (i.Price * i.Count);
        }

        return View(cartVM);
    }

    public IActionResult Plus(int id)
    {
        var cartItem = unitofwork.shoppingcart.Get(u => u.Id == id);
        if (cartItem != null)
        {
            cartItem.Count += 1;
            unitofwork.shoppingcart.Update(cartItem);
            unitofwork.Save();
        }
        return RedirectToAction("Index");
    }

    public IActionResult Minus(int id)
    {
        var cartItem = unitofwork.shoppingcart.Get(u => u.Id == id, track: true);
        if (cartItem != null)
        {
            if (cartItem.Count <= 1)
            {
                HttpContext.Session.SetInt32(SD.SessionCart, unitofwork.shoppingcart.GetAll(
                    u => u.ApplicationUserId == cartItem.ApplicationUserId).Count() - 1);
                unitofwork.shoppingcart.Remove(cartItem);
            }
            else
            {
                cartItem.Count -= 1;
                unitofwork.shoppingcart.Update(cartItem);
            }
            unitofwork.Save();
        }
        return RedirectToAction("Index");
    }

    public IActionResult Remove(int id)
    {
        var cartItem = unitofwork.shoppingcart.Get(u => u.Id == id, track:true);
        if (cartItem != null)
        {
            unitofwork.shoppingcart.Remove(cartItem);
            HttpContext.Session.SetInt32(SD.SessionCart, unitofwork.shoppingcart.GetAll(
                u => u.ApplicationUserId == cartItem.ApplicationUserId).Count()-1);
            unitofwork.Save();
        }
        return RedirectToAction("Index");
    }

    public IActionResult Summary()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        IEnumerable<ShoppingCart> cart = unitofwork.shoppingcart.GetAll(u => u.ApplicationUserId == userId,
            includeproperties: "Product");

        cartVM = new()
        {
            ShoppingCartList = unitofwork.shoppingcart.GetAll(u => u.ApplicationUserId == userId,
            includeproperties: "Product"),
            OrderHeader = new()
        };

        foreach (var i in cartVM.ShoppingCartList)
        {
            i.Price = CalculateCost(i);
            cartVM.OrderHeader.OrderTotal += (i.Price * i.Count);
        }

        cartVM.OrderHeader.ApplicationUser = unitofwork.applicationuser.Get(u => u.Id == userId);
        cartVM.OrderHeader.Name = cartVM.OrderHeader.ApplicationUser.Name;
        cartVM.OrderHeader.PhoneNumber = cartVM.OrderHeader.ApplicationUser.PhoneNumber;
        cartVM.OrderHeader.StreetAddress = cartVM.OrderHeader.ApplicationUser.StreetAddress;
        cartVM.OrderHeader.City = cartVM.OrderHeader.ApplicationUser.City;
        cartVM.OrderHeader.State = cartVM.OrderHeader.ApplicationUser.State;
        cartVM.OrderHeader.PostalCode = cartVM.OrderHeader.ApplicationUser.PostalCode;

        return View(cartVM);
    }

    [HttpPost]
    [ActionName("Summary")]
    public IActionResult SummaryPOST(CartVM obj)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        obj.ShoppingCartList = unitofwork.shoppingcart.GetAll(u => u.ApplicationUserId == userId,
                includeproperties: "Product");


        obj.OrderHeader.OrderDate = DateTime.Now;
        obj.OrderHeader.ApplicationUserId = userId;

        ApplicationUser applicationUser = unitofwork.applicationuser.Get(u => u.Id == userId);

        foreach (var i in obj.ShoppingCartList)
        {
            i.Price = CalculateCost(i);
            obj.OrderHeader.OrderTotal += (i.Price * i.Count);
        }

        if (applicationUser.ComapanyId.GetValueOrDefault() == 0)
        {
            //normal user
            obj.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            obj.OrderHeader.OrderStatus = SD.StatusPending;
        }
        else
        {
            //company user
            obj.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            obj.OrderHeader.OrderStatus = SD.StatusApproved;
        }

        unitofwork.orderheader.Add(obj.OrderHeader);
        unitofwork.Save();

        foreach (var cart in obj.ShoppingCartList)
        {
            OrderDetail detail = new()
            {
                ProductId = cart.ProductId,
                OrderHeaderId = obj.OrderHeader.Id,
                Price = cart.Price,
                Count = cart.Count,
            };
            unitofwork.orderdetail.Add(detail);
            unitofwork.Save();
        }

        if (applicationUser.ComapanyId.GetValueOrDefault() == 0)
        {
            //stripe logic for normal customer
            //it is a regular customer account and we need to capture payment
            //stripe logic
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={obj.OrderHeader.Id}",
                CancelUrl = domain + "customer/cart/index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in obj.ShoppingCartList)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), 
                        Currency = "inr",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            unitofwork.orderheader.UpdateSessionId(obj.OrderHeader.Id, session.Id, session.PaymentIntentId);
            unitofwork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        return RedirectToAction(nameof(OrderConfirmation), new { id = obj.OrderHeader.Id });
    }

    public IActionResult OrderConfirmation(int id)
    {
        OrderHeader orderHeader = unitofwork.orderheader.Get(u => u.Id == id, includeproperties: "ApplicationUser");

        if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment) { 
            var service=new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid") {
                unitofwork.orderheader.UpdateSessionId(id, session.Id, session.PaymentIntentId);
                unitofwork.orderheader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                unitofwork.Save();
            }
            HttpContext.Session.Clear();
        }

        //after order confirmation remove the items from the cart
        List<ShoppingCart> shoppingCarts = unitofwork.shoppingcart.GetAll(u=>u.ApplicationUserId==orderHeader.ApplicationUserId).ToList();

        unitofwork.shoppingcart.RemoveRange(shoppingCarts);
        unitofwork.Save();

        return View(id);
    }

    public double CalculateCost(ShoppingCart shoppingCart)
    {
        if (shoppingCart.Count <= 50)
        {
            return shoppingCart.Product.Price;
        }
        else
        {
            if (shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            else
            {
                return shoppingCart.Product.Price100;
            }
        }
    }
}
