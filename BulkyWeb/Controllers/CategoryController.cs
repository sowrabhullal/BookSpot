using BulkyWeb.Data;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db) { 
            this._db = db;
        }
        public IActionResult Index()
        {
            var category = _db.Categories.ToList();
            return View(category);
        }
    }
}
