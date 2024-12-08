using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrintWeb.Data;

namespace PrintWeb.Controllers
{
    public class PrintingLogsController : Controller
    {
        private readonly PrintwebContext _context;

        public PrintingLogsController(PrintwebContext context)
        {
            _context = context;
        }

        // GET: PrintingLogs
        public async Task<IActionResult> Index()
        {
            var printwebContext = _context.PrintingLogs.Include(p => p.Document).Include(p => p.PaperType).Include(p => p.Printer).Include(p => p.Student);
            return View(await printwebContext.ToListAsync());
        }

        // GET: PrintingLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var printingLog = await _context.PrintingLogs
                .Include(p => p.Document)
                .Include(p => p.PaperType)
                .Include(p => p.Printer)
                .Include(p => p.Student)
                .FirstOrDefaultAsync(m => m.PrintingLogId == id);
            if (printingLog == null)
            {
                return NotFound();
            }

            return View(printingLog);
        }

        // GET: PrintingLogs/Create
        public IActionResult Create()
        {
            var accountId = User.Identity.Name; // Assume the logged-in user's AccountId is stored in User.Identity.Name
            var studentId = _context.Accounts
                .Where(a => a.AccountId == accountId)
                .Select(a => a.AccountNavigation.StudentId)
                .FirstOrDefault();

            if (studentId == null)
            {
                return Unauthorized(); // Handle cases where StudentId cannot be retrieved
            }

            // Filter documents for the current student only
            var documents = _context.Documents
                .Where(d => d.StudentId == studentId)
                .Select(d => new { d.DocumentId, d.FileName });

            // Filter printers with status "Ready"
            var readyPrinters = _context.Printers
                .Where(p => p.Status == "Ready")
                .Select(p => new { p.PrinterId, DisplayName = $"{p.Brand} {p.Model} (Building {p.BuildingName} - Room {p.RoomNumber})" });

            ViewData["DocumentId"] = new SelectList(documents, "DocumentId", "FileName");
            ViewData["PaperTypeId"] = new SelectList(_context.PaperTypes, "PaperTypeId", "PaperTypeName");
            ViewData["PrinterId"] = new SelectList(readyPrinters, "PrinterId", "DisplayName");
            ViewData["ColoredOptions"] = new SelectList(new[] { "Yes", "No" }); // Option for Colored
            ViewData["IsTwoSideOptions"] = new SelectList(new[]
            {
        new { Value = true, Text = "Yes" },
        new { Value = false, Text = "No" }
    }, "Value", "Text");

            return View();
        }




        // POST: PrintingLogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DocumentId,PrinterId,PaperTypeId,Colored,Quantity,IsTwoSide")] PrintingLog printingLog)
        {
            var accountId = User.Identity.Name; // Assume the logged-in user's AccountId is stored in User.Identity.Name
            var studentId = _context.Accounts
                .Where(a => a.AccountId == accountId)
                .Select(a => a.AccountNavigation.StudentId)
                .FirstOrDefault();

            if (studentId == null)
            {
                return Unauthorized(); // Handle cases where StudentId cannot be retrieved
            }

            printingLog.StudentId = studentId;
            printingLog.StartTime = DateTime.Now; // Set StartTime to current time
            printingLog.EndTime = printingLog.StartTime.Value.AddMinutes(10); // Set EndTime to StartTime + 10 minutes
            printingLog.Status = "Completed"; // Set Status to always "Completed"

            // Update quantity in DetailPaperStudent
            var detailPaper = _context.DetailPaperStudents
                .FirstOrDefault(d => d.StudentId == studentId && d.PaperTypeId == printingLog.PaperTypeId);

            if (detailPaper == null || detailPaper.Quantity < printingLog.Quantity)
            {
                ModelState.AddModelError(string.Empty, "Insufficient paper quantity. Please add more paper.");
            }
            else
            {
                detailPaper.Quantity -= printingLog.Quantity ?? 0;
            }

            if (ModelState.IsValid)
            {
                _context.Add(printingLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload select lists and options for the view
            ViewData["DocumentId"] = new SelectList(_context.Documents, "DocumentId", "FileName", printingLog.DocumentId);
            ViewData["PaperTypeId"] = new SelectList(_context.PaperTypes, "PaperTypeId", "PaperTypeName", printingLog.PaperTypeId);
            ViewData["PrinterId"] = new SelectList(_context.Printers, "PrinterId", "PrinterId", printingLog.PrinterId);
            ViewData["ColoredOptions"] = new SelectList(new[] { "Yes", "No" }, printingLog.Colored);
            ViewData["IsTwoSideOptions"] = new SelectList(new[]
            {
        new { Value = true, Text = "Yes" },
        new { Value = false, Text = "No" }
    }, "Value", "Text", printingLog.IsTwoSide);

            return View(printingLog);
        }

        // GET: PrintingLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var printingLog = await _context.PrintingLogs.FindAsync(id);
            if (printingLog == null)
            {
                return NotFound();
            }
            ViewData["DocumentId"] = new SelectList(_context.Documents, "DocumentId", "DocumentId", printingLog.DocumentId);
            ViewData["PaperTypeId"] = new SelectList(_context.PaperTypes, "PaperTypeId", "PaperTypeId", printingLog.PaperTypeId);
            ViewData["PrinterId"] = new SelectList(_context.Printers, "PrinterId", "PrinterId", printingLog.PrinterId);
            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "StudentId", printingLog.StudentId);
            return View(printingLog);
        }

        // POST: PrintingLogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PrintingLogId,StudentId,PrinterId,DocumentId,PaperTypeId,Colored,StartTime,EndTime,Quantity,IsTwoSide,Status")] PrintingLog printingLog)
        {
            if (id != printingLog.PrintingLogId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(printingLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrintingLogExists(printingLog.PrintingLogId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DocumentId"] = new SelectList(_context.Documents, "DocumentId", "DocumentId", printingLog.DocumentId);
            ViewData["PaperTypeId"] = new SelectList(_context.PaperTypes, "PaperTypeId", "PaperTypeId", printingLog.PaperTypeId);
            ViewData["PrinterId"] = new SelectList(_context.Printers, "PrinterId", "PrinterId", printingLog.PrinterId);
            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "StudentId", printingLog.StudentId);
            return View(printingLog);
        }

        // GET: PrintingLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var printingLog = await _context.PrintingLogs
                .Include(p => p.Document)
                .Include(p => p.PaperType)
                .Include(p => p.Printer)
                .Include(p => p.Student)
                .FirstOrDefaultAsync(m => m.PrintingLogId == id);
            if (printingLog == null)
            {
                return NotFound();
            }

            return View(printingLog);
        }

        // POST: PrintingLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var printingLog = await _context.PrintingLogs.FindAsync(id);
            if (printingLog != null)
            {
                _context.PrintingLogs.Remove(printingLog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrintingLogExists(int id)
        {
            return _context.PrintingLogs.Any(e => e.PrintingLogId == id);
        }
    }
}
