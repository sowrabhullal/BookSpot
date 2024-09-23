using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models.ViewModel;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Area("Customer")]
public class CartController : Controller
{
    private readonly IUnitOfWork unitofwork;
    private CartVM cartVM;
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
            includeproperties: "Product")
        };

        foreach (var i in cartVM.ShoppingCartList)
        {
            i.Price = CalculateCost(i);
            cartVM.OrderTotal += (i.Price * i.Count);
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
        var cartItem = unitofwork.shoppingcart.Get(u => u.Id == id);
        if (cartItem != null)
        {
            if (cartItem.Count <= 1)
            {
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
        var cartItem = unitofwork.shoppingcart.Get(u => u.Id == id);
        if (cartItem != null)
        {
            unitofwork.shoppingcart.Remove(cartItem);
            unitofwork.Save();
        }
        return RedirectToAction("Index");
    }

    public IActionResult Summary()
    {
        return View();
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
