using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitofwork;
        private readonly IWebHostEnvironment webHostEnvironment;
        public ProductController(IUnitOfWork u, IWebHostEnvironment w)
        {
            unitofwork = u;
            webHostEnvironment = w;
        }

        [Authorize]
        public IActionResult Index()
        {
            //unitofwork has category object 
            var product = unitofwork.product.GetAll(includeproperties: "Category");

            return View(product);
        }

        //combining create and edit it upsert - [update + insert ]
        //we might or might not get id, so is the below parameter
        public IActionResult Upsert(int? id)
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

            if (id == null || id == 0)
            {
                //create
                return View(productvm);
            }
            else
            {
                //update
                productvm.Product = unitofwork.product.Get(u => u.Id == id);
                return View(productvm);
            }
        }

        //Iformfile we get when uploading a file

        [HttpPost]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            //Server side validations
            if (ModelState.IsValid)
            {
                string wwwrootpath = webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwrootpath, @"images\product");
                    Console.WriteLine(obj.Product.Title);
                    if (!String.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        var oldfilepath = Path.Combine(wwwrootpath, obj.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldfilepath))
                        {
                            // Delete the file
                            System.IO.File.Delete(oldfilepath);
                        }
                    }

                    using (var filestream = new FileStream(Path.Combine(productPath, filename), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }

                    obj.Product.ImageUrl = @"\images\product\" + filename;
                }
                if (obj.Product.Id != 0)
                {
                    unitofwork.product.Update(obj.Product);
                }
                else
                {
                    unitofwork.product.Add(obj.Product);
                }

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

        //public IActionResult Edit(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }

        //    Product productfromdb = unitofwork.product.Get(u => u.Id == id);

        //    if (productfromdb == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(productfromdb);
        //}

        //Submit button

        //[HttpPost]
        //public IActionResult Edit(ProductVM obj)
        //{
        //    unitofwork.product.Update(obj.Product);
        //    unitofwork.Save();
        //    TempData["sucess"] = "Product updated sucesfuly";
        //    return RedirectToAction("Index");

        //}

        //[ActionName("Delete")]
        //public IActionResult DeletePost(int id)
        //{
        //    unitofwork.product.Remove(unitofwork.product.Get(u => u.Id == id));
        //    unitofwork.Save();
        //    TempData["sucess"] = "Product deleted sucesfuly";
        //    return RedirectToAction("Index");
        //}

        //API's Already supported in ASP.NET, nothing to add explicitly
        //To access http://localhost:5059/Admin/Product/GetAll
        [HttpGet]
        public IActionResult GetAll()
        {
            var product = unitofwork.product.GetAll(includeproperties: "Category");

            return Json(new { data = product });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var producttodelete = unitofwork.product.Get(u => u.Id == id);

            if (producttodelete == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            else {
                unitofwork.product.Remove(producttodelete);
                unitofwork.Save();
                return Json(new { success = true, message = "Product deleted sucessfully" });
            }
        }
    }
}
