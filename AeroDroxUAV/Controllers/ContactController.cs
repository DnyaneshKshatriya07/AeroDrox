using Microsoft.AspNetCore.Mvc;

namespace AeroDroxUAV.Controllers
{
    // Apply anti-caching
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ContactController : Controller
    {
        // GET: /Contact/Index
        public IActionResult Index()
        {
            ViewData["Title"] = "Contact Us";
            ViewBag.IsLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
            ViewBag.Role = HttpContext.Session.GetString("Role") ?? "";
            return View();
        }

        // POST: /Contact/Submit
        [HttpPost]
        public IActionResult Submit(string name, string email, string message)
        {
            // In a real application, you would:
            // 1. Validate the input (ModelState.IsValid)
            // 2. Log the contact message
            // 3. Send an email notification

            ViewBag.SuccessMessage = "Thank you for contacting us! We will get back to you shortly.";
            ViewData["Title"] = "Contact Us";
            ViewBag.IsLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
            ViewBag.Role = HttpContext.Session.GetString("Role") ?? "";
            return View("Index");
        }
    }
}