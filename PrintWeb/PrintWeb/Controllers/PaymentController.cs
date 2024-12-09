using Microsoft.AspNetCore.Mvc;
using PrintWeb.Data;
using System;
using System.Linq;
using System.Security.Claims;

namespace PrintWeb.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PrintwebContext _context;

        public PaymentController(PrintwebContext context)
        {
            _context = context;
        }

        // Hiển thị form nạp tiền
        public IActionResult AddBalance()
        {
            // Lấy accountId từ thông tin đăng nhập
            var accountId = User.FindFirstValue(ClaimTypes.Name);

            // Tìm StudentId khớp với AccountId
            var student = _context.Students.FirstOrDefault(s => s.StudentId == accountId);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy tên phương thức thanh toán mặc định
            var defaultPaymentMethod = _context.PaymentMethods.FirstOrDefault(pm => pm.PaymentMethodId == 1);
            if (defaultPaymentMethod != null)
            {
                ViewBag.PaymentName = defaultPaymentMethod.PaymentName;
            }
            else
            {
                ViewBag.PaymentName = "Default Payment Method";
            }

            return View(student); // Truyền thông tin student và PaymentName sang View
        }

        // Xử lý logic nạp tiền
        [HttpPost]
        public IActionResult AddBalance(decimal amount)
        {
            // Lấy accountId từ thông tin đăng nhập
            var accountId = User.FindFirstValue(ClaimTypes.Name);

            // Tìm StudentId khớp với AccountId
            var student = _context.Students.FirstOrDefault(s => s.StudentId == accountId);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction("Index", "Home");
            }

            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Invalid amount. Please enter a positive value.";
                return RedirectToAction("AddBalance");
            }

            // Mặc định phương thức thanh toán là 1
            int defaultPaymentMethodId = 1;

            // Kiểm tra phương thức thanh toán hợp lệ
            var paymentMethod = _context.PaymentMethods.FirstOrDefault(pm => pm.PaymentMethodId == defaultPaymentMethodId);
            if (paymentMethod == null)
            {
                TempData["ErrorMessage"] = "Default payment method not found.";
                return RedirectToAction("AddBalance");
            }

            // Cộng dồn số tiền vào AccountBalance
            student.AccountBalance = (student.AccountBalance ?? 0) + amount;

            // Thêm record vào bảng PaymentRecord
            var paymentRecord = new PaymentRecord
            {
                PaymentId = Guid.NewGuid().ToString(),
                StudentId = student.StudentId, // Dùng StudentId của sinh viên hiện tại
                Amount = amount,
                PaymentMethod = defaultPaymentMethodId,
                PaymentDate = DateTime.Now
            };

            _context.PaymentRecords.Add(paymentRecord);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Balance updated successfully!";
            return RedirectToAction("qrcode");
        }

        public IActionResult qrcode()
        {
            return View();
        }
        public IActionResult Successfull()
        { 
            return View();
        }
    }
}
