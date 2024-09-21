using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            IEnumerable<SelectListItem> CategoryList = unitofwork.category.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value=u.Id.ToString()
            });

            return View(product);
        }

        public IActionResult Create()
        {
            //not used anymore
            /*IEnumerable<SelectListItem> CategoryList = unitofwork.category.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });*/

            ProductVM productvm = new()
            {
                CategoryList = unitofwork.category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };

            return View(productvm);
        }

        [HttpPost]
        public IActionResult Create(ProductVM obj)
        {
            //Server side validations
            if (ModelState.IsValid)
            {
                unitofwork.product.Add(obj.Product);
                unitofwork.Save();
                TempData["sucess"] = "Product created sucesfuly";
                return RedirectToAction("Index");
            }
            else
            {
                //populate drop down again for redirecting
                obj.CategoryList = unitofwork.category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                   

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
        public IActionResult Edit(ProductVM obj)
        {
            unitofwork.product.Update(obj.Product);
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
