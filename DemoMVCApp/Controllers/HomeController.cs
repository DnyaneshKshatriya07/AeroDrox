using Microsoft.AspNetCore.Mvc;
using DemoMVCApp.Models;

namespace DemoMVCApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var student = new Student()
            {
                Id = 1,
                Name = "Dnyanesh",
                Course = "Computer Engineering"
            };

            return View(student);
        }
    }
}
