using BulkyWeb.Data;
using BulkyWeb.Models;
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

        public IActionResult Create() { 
            return View();
        }

        //Submit button

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            //Server side validations
            if (obj.Name == obj.DisplayOrder.ToString()) {
                ModelState.AddModelError("Name", "Name cannot be same as dsiplay order");
            }
            if (ModelState.IsValid)
            {
                _db.Categories.Add(obj);
                _db.SaveChanges();
                TempData["sucess"] = "Category created sucesfuly";
                return RedirectToAction("Index");
            }
            else { 
                return View(obj);
            }
            
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) { 
                return NotFound();
            }

            Category categoryfromdb = _db.Categories.Find(id);

            if (categoryfromdb == null) {
                return NotFound();
            }
            
            return View(categoryfromdb);
        }

        //Submit button

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            _db.Categories.Update(obj);
            _db.SaveChanges();
            TempData["sucess"] = "Category updated sucesfuly";
            return RedirectToAction("Index");

        }

        //we can give diff name for method but still have same action name- use below attribute
        [ActionName("Delete")]
        public IActionResult DeletePost(int id) {
            _db.Categories.Remove(_db.Categories.Find(id));
            _db.SaveChanges();
            TempData["sucess"] = "Category deleted sucesfuly";
            return RedirectToAction("Index");
        }
    }
}
