using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models.ViewModel;
using Bulky.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController: Controller
    {
        private readonly IUnitOfWork unitofwork;
        public CompanyController(IUnitOfWork u)
        {
            unitofwork = u;
        }
        public IActionResult Index()
        {
            var company = unitofwork.company.GetAll();
            return View(company);
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                //create
                return View();
            }
            else
            {
                //update
                var company = unitofwork.company.Get(u => u.Id == id);
                return View(company);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company obj)
        {
            //Server side validations
            if (ModelState.IsValid)
            {
                if (obj.Id != 0)
                {
                    unitofwork.company.Update(obj);
                }
                else
                {
                    unitofwork.company.Add(obj);
                }
                unitofwork.Save();
                TempData["sucess"] = "Company created sucesfuly";
                return RedirectToAction("Index");
            }
            else
            {
                return View(obj);
            }
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var company = unitofwork.company.GetAll();

            return Json(new { data = company });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var companytodelete = unitofwork.company.Get(u => u.Id == id);

            if (companytodelete == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            else
            {
                unitofwork.company.Remove(companytodelete);
                unitofwork.Save();
                return Json(new { success = true, message = "Company deleted sucessfully" });
            }
        }
    }
}
