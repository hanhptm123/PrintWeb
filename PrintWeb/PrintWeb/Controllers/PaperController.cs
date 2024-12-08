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

            // Cập nhật giấy cho sinh viên
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

            // Tạo bản ghi mua giấy
            var buyPaperLog = new BuyPaperLog
            {
                StudentId = studentId,
                DateBuy = DateTime.Now,
                TotalBuy = totalAmount
            };
            _context.BuyPaperLogs.Add(buyPaperLog);
            _context.SaveChanges(); // Lưu trước để có ID

            // Thêm chi tiết mua giấy
            _context.DetailBuyPaperLogs.Add(new DetailBuyPaperLog
            {
                BuyPaperLogId = buyPaperLog.BuyPaperLogId,
                PaperTypeId = paperTypeId,
                PaperBuy = quantity
            });

            // Trừ số dư tài khoản sinh viên
            student.AccountBalance -= totalAmount;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Thanh toán thành công! Số lượng giấy đã được cập nhật.";
            return RedirectToAction("BuyPaper");
        }

        public IActionResult PaymentHistory(int studentId)
        {
            // Chuyển đổi studentId từ string thành int
            var studentIdFromIdentity = User.Identity?.Name;
            int studentIdInt;

            if (string.IsNullOrEmpty(studentIdFromIdentity) || !int.TryParse(studentIdFromIdentity, out studentIdInt))
            {
                return RedirectToAction("Login", "Accounts");
            }

            // Lấy sinh viên từ cơ sở dữ liệu với studentId là kiểu int
            var student = _context.Students.Find(studentIdInt);
            if (student == null)
            {
                return NotFound("Không tìm thấy sinh viên.");
            }

            // Lấy các bản ghi mua giấy của sinh viên
            var buyPaperLogs = _context.BuyPaperLogs.Where(b => b.StudentId == studentIdInt).ToList();

            // Lấy chi tiết của từng BuyPaperLog
            foreach (var log in buyPaperLogs)
            {
                log.Details = _context.DetailBuyPaperLogs.Where(d => d.BuyPaperLogId == log.Id).ToList();
            }

            // Truyền dữ liệu vào ViewBag
            ViewBag.Student = student;
            ViewBag.BuyPaperLogs = buyPaperLogs;

            return View();
        }



    }
}