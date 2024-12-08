using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrintWeb.Data;
using System.Security.Claims;

namespace PrintWeb.Controllers
{
    [Authorize]
    public class PrintingLogController : Controller
    {
        private readonly PrintwebContext _context;

        public PrintingLogController(PrintwebContext context)
        {
            _context = context;
        }

        // GET: PrintingLog
        public async Task<IActionResult> Index(int? page)
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "You must log in to view printing logs.";
                return RedirectToAction("Login", "Accounts");
            }

            const int PageSize = 5; 
            var currentPage = page ?? 1;
            var totalRecords = await _context.PrintingLogs
                .Where(pl => pl.StudentId == userId)
                .CountAsync();

            var logs = await _context.PrintingLogs
                .AsNoTracking()
                .Include(pl => pl.Student)
                .Include(pl => pl.Printer)
                .Include(pl => pl.Document)
                .Include(pl => pl.PaperType)
                .Where(pl => pl.StudentId == userId)
                .OrderByDescending(pl => pl.StartTime)
                .Skip((currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalRecords / PageSize);

            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;

            return View(logs);
        }

        public IActionResult Create()
        {
            ViewBag.DocumentId = new SelectList(_context.Documents.ToList(), "DocumentId", "FileName");
            ViewBag.PaperTypeId = new SelectList(_context.PaperTypes.ToList(), "PaperTypeId", "PaperTypeName");
            ViewBag.PrinterId = new SelectList(_context.Printers.ToList(), "PrinterId", "Brand");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PrintingLog printing)
        {
            if (ModelState.IsValid)
            {
                _context.PrintingLogs.Add(printing);
                _context.SaveChanges(); 
                TempData["Message"] = "PrintingLog created successfully";
                return RedirectToAction("Index");
            }
            return View(printing);
        }
    }
}
