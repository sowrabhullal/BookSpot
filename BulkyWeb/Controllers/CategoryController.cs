using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Bulky.DataAccess.Repository.IRepository;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository repository;
        public CategoryController(ICategoryRepository rp)
        {
            repository = rp;
        }
        public IActionResult Index()
        {
            var category = repository.GetAll();
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
                repository.Add(obj);
                repository.Save();
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

            Category categoryfromdb = repository.Get(u => u.Id == id);

            if (categoryfromdb == null) {
                return NotFound();
            }
            
            return View(categoryfromdb);
        }

        //Submit button

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            repository.Update(obj);
            repository.Save();
            TempData["sucess"] = "Category updated sucesfuly";
            return RedirectToAction("Index");

        }

        //we can give diff name for method but still have same action name- use below attribute
        [ActionName("Delete")]
        public IActionResult DeletePost(int id) {
            repository.Remove(repository.Get(u=>u.Id==id));
            repository.Save();
            TempData["sucess"] = "Category deleted sucesfuly";
            return RedirectToAction("Index");
        }
    }
}
