using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModel;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitofwork;

        [BindProperty]
        public OrderVM ordervm { get; set; }
        public OrderController(IUnitOfWork u)
        {
            unitofwork = u;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            ordervm = new()
            {
                OrderHeader = unitofwork.orderheader.Get(u => u.Id == orderId, includeproperties: "ApplicationUser"),
                OrderDetail = unitofwork.orderdetail.GetAll(u => u.OrderHeaderId == orderId, includeproperties: "Product")
            };
            return View(ordervm);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = unitofwork.orderheader.Get(u => u.Id == ordervm.OrderHeader.Id);
            orderHeaderFromDb.Name = ordervm.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = ordervm.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = ordervm.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = ordervm.OrderHeader.City;
            orderHeaderFromDb.State = ordervm.OrderHeader.State;
            orderHeaderFromDb.PostalCode = ordervm.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(ordervm.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = ordervm.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(ordervm.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.Carrier = ordervm.OrderHeader.TrackingNumber;
            }
            unitofwork.orderheader.Update(orderHeaderFromDb);
            unitofwork.Save();

            TempData["Success"] = "Order Details Updated Successfully.";

            return RedirectToAction(nameof(Details), new { orderId = ordervm.OrderHeader.Id });
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            unitofwork.orderheader.UpdateStatus(ordervm.OrderHeader.Id, SD.StatusInProcess, null);
            unitofwork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = ordervm.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {

            var orderHeader = unitofwork.orderheader.Get(u => u.Id == ordervm.OrderHeader.Id);
            orderHeader.TrackingNumber = ordervm.OrderHeader.TrackingNumber;
            orderHeader.Carrier = ordervm.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            unitofwork.orderheader.Update(orderHeader);
            unitofwork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = ordervm.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {

            var orderHeader = unitofwork.orderheader.Get(u => u.Id == ordervm.OrderHeader.Id);

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                unitofwork.orderheader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                unitofwork.orderheader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            unitofwork.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = ordervm.OrderHeader.Id });

        }



        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            ordervm.OrderHeader = unitofwork.orderheader
                .Get(u => u.Id == ordervm.OrderHeader.Id, includeproperties: "ApplicationUser");
            ordervm.OrderDetail = unitofwork.orderdetail
                .GetAll(u => u.OrderHeaderId == ordervm.OrderHeader.Id, includeproperties: "Product");

            //stripe logic
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={ordervm.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={ordervm.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in ordervm.OrderDetail)
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
            unitofwork.orderheader.UpdateSessionId(ordervm.OrderHeader.Id, session.Id, session.PaymentIntentId);
            unitofwork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {

            OrderHeader orderHeader = unitofwork.orderheader.Get(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //this is an order by company

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitofwork.orderheader.UpdateSessionId(orderHeaderId, session.Id, session.PaymentIntentId);
                    unitofwork.orderheader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    unitofwork.Save();
                }


            }

            return View(orderHeaderId);
        }

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders;


            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaders = unitofwork.orderheader.GetAll(includeproperties: "ApplicationUser").ToList();
            }
            else
            {

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeaders = unitofwork.orderheader
                    .GetAll(u => u.ApplicationUserId == userId, includeproperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;

            }

            return Json(new { data = objOrderHeaders });
        }
    }
}
