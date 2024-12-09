using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrintWeb.Data;
using System.Linq;
using System.Security.Claims;

namespace PrintWeb.Controllers
{
    public class ProfileController : Controller
    {
        private readonly PrintwebContext _context;

        public ProfileController(PrintwebContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var accountId = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(accountId))
            {
                TempData["ErrorMessage"] = "Please log in to view your profile.";
                return RedirectToAction("Login", "Account");
            }

            var student = _context.Students
                .Include(s => s.Account)
                .Include(s => s.DetailPaperStudents)
                .ThenInclude(dps => dps.PaperType)
                .FirstOrDefault(s => s.StudentId == accountId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy thông tin giấy theo từng loại
            var paperDetails = student.DetailPaperStudents
                .Select(dps => new
                {
                    PaperTypeName = dps.PaperType.PaperTypeName,
                    Quantity = dps.Quantity
                })
                .ToList();

            ViewBag.PaperDetails = paperDetails;

            return View(student);
        }

        public IActionResult EditPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EditPassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["ErrorMessage"] = "All fields are required.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation do not match.";
                return View();
            }

            var accountId = User.FindFirstValue(ClaimTypes.Name);
            if (accountId == null)
            {
                TempData["ErrorMessage"] = "You need to log in to edit your password.";
                return RedirectToAction("Login", "Account");
            }

            var account = _context.Accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null)
            {
                TempData["ErrorMessage"] = "Account not found.";
                return View();
            }

            if (account.Password != currentPassword)
            {
                TempData["ErrorMessage"] = "Current password is incorrect.";
                return View();
            }

            account.Password = newPassword;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Password updated successfully.";
            return RedirectToAction("Index", "Profile");
        }
    }
}
