using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController:Controller
    {
        private readonly IUnitOfWork unitofwork;
        public ProductController(IUnitOfWork u)
        {
            unitofwork = u;
        }
        public IActionResult Index()
        {
            //unitofwork has category object 
            var product = unitofwork.product.GetAll();
            return View(product);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product obj)
        {
            //Server side validations
            if (ModelState.IsValid)
            {
                unitofwork.product.Add(obj);
                unitofwork.Save();
                TempData["sucess"] = "Product created sucesfuly";
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

            Product productfromdb = unitofwork.product.Get(u => u.Id == id);

            if (productfromdb == null)
            {
                return NotFound();
            }

            return View(productfromdb);
        }

        //Submit button

        [HttpPost]
        public IActionResult Edit(Product obj)
        {
            unitofwork.product.Update(obj);
            unitofwork.Save();
            TempData["sucess"] = "Product updated sucesfuly";
            return RedirectToAction("Index");

        }

        [ActionName("Delete")]
        public IActionResult DeletePost(int id)
        {
            unitofwork.product.Remove(unitofwork.product.Get(u => u.Id == id));
            unitofwork.Save();
            TempData["sucess"] = "Product deleted sucesfuly";
            return RedirectToAction("Index");
        }
    }
}
