using BookSpot.DataAccess.Repository.IRepository;
using BookSpot.Models;
using BookSpot.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BookSpotWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork unitofwork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork u)
        {
            _logger = logger;
            unitofwork = u;
        }

        public IActionResult Index()
        {
            var product = unitofwork.product.GetAll(includeproperties: "Category");

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            HttpContext.Session.SetInt32(SD.SessionCart, 0);

            if (claim != null)
            {
                ShoppingCart cartfromdb = unitofwork.shoppingcart.Get(u => u.ApplicationUserId == claim.Value);

                if (cartfromdb != null)
                {
                    HttpContext.Session.SetInt32(SD.SessionCart,
                    unitofwork.shoppingcart.GetAll(u => u.ApplicationUserId == cartfromdb.ApplicationUserId).Count());
                }
            }

            return View(product);
        }


        public IActionResult Details(int id) {
            ShoppingCart cart = new()
            {
                Product = unitofwork.product.Get(u => u.Id == id, includeproperties: "Category"),
                Count = 1,
                ProductId = id
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartfromdb = unitofwork.shoppingcart.Get(u => u.ApplicationUserId == shoppingCart.ApplicationUserId &&
            u.ProductId == shoppingCart.ProductId);

            if (cartfromdb != null)
            {
                cartfromdb.Count += shoppingCart.Count;
                unitofwork.shoppingcart.Update(cartfromdb);
                unitofwork.Save();
            }
            else {
                unitofwork.shoppingcart.Add(shoppingCart);
                unitofwork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, 
                    unitofwork.shoppingcart.GetAll(u => u.ApplicationUserId == shoppingCart.ApplicationUserId).Count());
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
