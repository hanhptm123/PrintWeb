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

        // Hiển thị trang mua thêm giấy
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
                TempData["ErrorMessage"] = "Không tìm thấy thông tin sinh viên.";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Student"] = student;
            var paperTypes = _context.PaperTypes.ToList();
            return View(paperTypes);
        }

        // Xử lý thanh toán
        [HttpPost]
        public IActionResult ProcessPayment(int paperTypeId, int quantity)
        {
            var studentId = User.Identity?.Name;
            if (string.IsNullOrEmpty(studentId))
            {
                return RedirectToAction("Login", "Accounts");
            }

            var student = _context.Students.FirstOrDefault(s => s.StudentId == studentId);
            if (student == null)
            {
                return NotFound("Sinh viên không tồn tại.");
            }

            var paperType = _context.PaperTypes.FirstOrDefault(pt => pt.PaperTypeId == paperTypeId);
            if (paperType == null)
            {
                return NotFound("Loại giấy không tồn tại.");
            }

            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Số lượng giấy phải lớn hơn 0.";
                return RedirectToAction("BuyPaper");
            }

            // Tính tổng tiền thanh toán
            decimal totalAmount = paperType.Price * quantity;

            // Kiểm tra số dư tài khoản
            if (student.AccountBalance < totalAmount)
            {
                TempData["ErrorMessage"] = "Số dư tài khoản không đủ để thanh toán.";
                return RedirectToAction("BuyPaper");
            }

            // Xử lý số lượng giấy (bao gồm giấy A3)
            int pagesToAdd = paperType.PaperTypeId == 2 ? quantity * 2 : quantity; // Giả sử PaperTypeId == 2 là giấy A3

            // Tìm bản ghi giấy của sinh viên
            var paperStudent = _context.DetailPaperStudents
                .FirstOrDefault(d => d.StudentId == studentId && d.PaperTypeId == paperTypeId);
            if (paperStudent != null)
            {
                // Nếu đã có bản ghi, cập nhật số lượng giấy
                paperStudent.Quantity += pagesToAdd;
            }
            else
            {
                // Nếu chưa có bản ghi, tạo mới
                _context.DetailPaperStudents.Add(new DetailPaperStudent
                {
                    StudentId = studentId,
                    PaperTypeId = paperTypeId,
                    Quantity = pagesToAdd
                });
            }

            // Lưu lịch sử thanh toán vào bảng PaymentRecord
            _context.PaymentRecords.Add(new PaymentRecord
            {
                PaymentId = Guid.NewGuid().ToString(),
                StudentId = studentId,
                Amount = totalAmount,
                PaymentMethod = 1, // 1 = Trả qua tài khoản
                PaymentDate = DateTime.Now
            });

            // Trừ số dư tài khoản sinh viên
            student.AccountBalance -= totalAmount;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Thanh toán thành công! Số lượng giấy đã được cập nhật.";
            return RedirectToAction("BuyPaper");
        }
        // Thêm vào Controller
        public IActionResult PaymentHistory()
        {
            var studentId = User.Identity?.Name;
            if (string.IsNullOrEmpty(studentId))
            {
                return RedirectToAction("Login", "Accounts");
            }

            var student = _context.Students.FirstOrDefault(s => s.StudentId == studentId);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin sinh viên.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy lịch sử thanh toán của sinh viên
            var paymentHistory = _context.PaymentRecords
                .Where(pr => pr.StudentId == studentId)
                .OrderByDescending(pr => pr.PaymentDate)
                .ToList();

            ViewData["Student"] = student;
            return View(paymentHistory);
        }

    }
}