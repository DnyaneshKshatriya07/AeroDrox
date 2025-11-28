using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;

namespace StudentApp.Controllers
{
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;

        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.Students.ToListAsync();
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(student);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var student = await _context.Students.FindAsync(id);
            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(student);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.Students.FindAsync(id);
            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Remove(student);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // ===========================================
        //           ANALYSIS CHART ACTION
        // ===========================================
        public async Task<IActionResult> Analysis()
        {
            var courseGroups = await _context.Students
                .GroupBy(s => s.Course)
                .Select(g => new
                {
                    Course = g.Key,
                    Count = g.Count(),
                    AvgAge = g.Average(s => s.Age)
                })
                .ToListAsync();

            ViewBag.CourseNames = courseGroups.Select(c => c.Course).ToList();
            ViewBag.StudentCounts = courseGroups.Select(c => c.Count).ToList();
            ViewBag.AverageAges = courseGroups.Select(c => c.AvgAge).ToList();

            return View();
        }
    }
}
