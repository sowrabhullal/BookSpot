using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BulkyWeb.Areas.Customer.Controllers
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

            return View(product);
        }

        public IActionResult Details(int id) {
            var product = unitofwork.product.Get(u=>u.Id==id,includeproperties: "Category");

            return View(product);
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
