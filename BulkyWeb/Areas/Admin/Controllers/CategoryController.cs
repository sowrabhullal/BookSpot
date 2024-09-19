using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Bulky.DataAccess.Repository.IRepository;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork unitofwork;
        public CategoryController(IUnitOfWork u)
        {
            unitofwork = u;
        }
        public IActionResult Index()
        {
            //unitofwork has category object 
            var category = unitofwork.category.GetAll();
            return View(category);
        }

        public IActionResult Create()
        {
            return View();
        }

        //Submit button

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            //Server side validations
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name cannot be same as dsiplay order");
            }
            if (ModelState.IsValid)
            {
                unitofwork.category.Add(obj);
                unitofwork.Save();
                TempData["sucess"] = "Category created sucesfuly";
                return RedirectToAction("Index");
            }
            else
            {
                return View(obj);
            }

        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category categoryfromdb = unitofwork.category.Get(u => u.Id == id);

            if (categoryfromdb == null)
            {
                return NotFound();
            }

            return View(categoryfromdb);
        }

        //Submit button

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            unitofwork.category.Update(obj);
            unitofwork.Save();
            TempData["sucess"] = "Category updated sucesfuly";
            return RedirectToAction("Index");

        }

        //we can give diff name for method but still have same action name- use below attribute
        [ActionName("Delete")]
        public IActionResult DeletePost(int id)
        {
            unitofwork.category.Remove(unitofwork.category.Get(u => u.Id == id));
            unitofwork.Save();
            TempData["sucess"] = "Category deleted sucesfuly";
            return RedirectToAction("Index");
        }
    }
}
