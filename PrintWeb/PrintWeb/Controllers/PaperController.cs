using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrintWeb.Data;
using System;
using System.Linq;

namespace PrintWeb.Controllers
{
    [Authorize]
    public class PaperController : Controller
    {
        private readonly PrintwebContext _context;

        public PaperController(PrintwebContext context)
        {
            _context = context;
        }

        // Display page to purchase more paper
        public IActionResult BuyPaper()
        {
            var studentId = User.Identity?.Name;
            if (string.IsNullOrEmpty(studentId))
            {
                return RedirectToAction("Login", "Accounts");
            }

            var student = _context.Students.FirstOrDefault(s => s.StudentId == studentId);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student information not found.";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Student"] = student;
            var paperTypes = _context.PaperTypes.ToList();
            return View(paperTypes);
        }

        [HttpPost]
        public IActionResult ProcessPayment(List<int> paperTypeIds, List<int> quantities)
        {
            var studentId = User.Identity?.Name;
            if (string.IsNullOrEmpty(studentId))
            {
                return RedirectToAction("Login", "Accounts");
            }

            var student = _context.Students.FirstOrDefault(s => s.StudentId == studentId);
            if (student == null)
            {
                return NotFound("Student does not exist.");
            }

            if (paperTypeIds == null || quantities == null || paperTypeIds.Count != quantities.Count)
            {
                TempData["ErrorMessage"] = "The data submitted is not valid.";
                return RedirectToAction("BuyPaper");
            }

            decimal totalAmount = 0;
            var detailBuyPaperLogs = new List<DetailBuyPaperLog>();

            // Loop through all paper types and quantities
            for (int i = 0; i < paperTypeIds.Count; i++)
            {
                int paperTypeId = paperTypeIds[i];
                int quantity = quantities[i];

                if (quantity <= 0) continue;

                var paperType = _context.PaperTypes.FirstOrDefault(pt => pt.PaperTypeId == paperTypeId);
                if (paperType == null) continue;

                decimal amount = paperType.Price * quantity;
                totalAmount += amount;

                // Handle paper quantity (including A3 paper)
                int pagesToAdd = paperType.PaperTypeId == 2 ? quantity  : quantity;

                var paperStudent = _context.DetailPaperStudents
                    .FirstOrDefault(d => d.StudentId == studentId && d.PaperTypeId == paperTypeId);

                if (paperStudent != null)
                {
                    paperStudent.Quantity += pagesToAdd;
                }
                else
                {
                    _context.DetailPaperStudents.Add(new DetailPaperStudent
                    {
                        StudentId = studentId,
                        PaperTypeId = paperTypeId,
                        Quantity = pagesToAdd
                    });
                }

                // Save paper purchase details into DetailBuyPaperLog
                detailBuyPaperLogs.Add(new DetailBuyPaperLog
                {
                    PaperTypeId = paperTypeId,
                    PaperBuy = quantity
                });
            }

            // Check student account balance
            if (totalAmount > student.AccountBalance)
            {
                TempData["ErrorMessage"] = "Insufficient account balance to complete the payment.";
                return RedirectToAction("BuyPaper");
            }

            // Create paper purchase log
            var buyPaperLog = new BuyPaperLog
            {
                StudentId = studentId,
                DateBuy = DateTime.Now,
                TotalBuy = totalAmount
            };

            _context.BuyPaperLogs.Add(buyPaperLog);
            _context.SaveChanges();

            // Save paper purchase details into DetailBuyPaperLog table
            foreach (var detail in detailBuyPaperLogs)
            {
                detail.BuyPaperLogId = buyPaperLog.BuyPaperLogId;
                _context.DetailBuyPaperLogs.Add(detail);
            }

            // Update student account balance
            student.AccountBalance -= totalAmount;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Payment successful! The paper quantity has been updated.";
            return RedirectToAction("BuyPaper");
        }

        public IActionResult PaymentHistory()
        {
            var studentIdFromIdentity = User.Identity?.Name;

            if (string.IsNullOrEmpty(studentIdFromIdentity))
            {
                return RedirectToAction("Login", "Accounts");
            }

            var student = _context.Students.FirstOrDefault(s => s.StudentId == studentIdFromIdentity);
            if (student == null)
            {
                return NotFound("Student not found.");
            }

            var buyPaperLogs = _context.BuyPaperLogs
                .Include(log => log.DetailBuyPaperLogs)
                .ThenInclude(detail => detail.PaperType)
                .Where(b => b.StudentId == studentIdFromIdentity)
                .ToList();

            if (buyPaperLogs == null || !buyPaperLogs.Any())
            {
                TempData["ErrorMessage"] = "No paper purchase history available.";
            }

            ViewBag.Student = student;
            ViewBag.BuyPaperLogs = buyPaperLogs;

            return View();
        }
    }
}
