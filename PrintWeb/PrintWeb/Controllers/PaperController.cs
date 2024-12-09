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
                return NotFound("Sinh viên không tồn tại.");
            }

            if (paperTypeIds == null || quantities == null || paperTypeIds.Count != quantities.Count)
            {
                TempData["ErrorMessage"] = "Dữ liệu gửi lên không hợp lệ.";
                return RedirectToAction("BuyPaper");
            }

            decimal totalAmount = 0;
            var detailBuyPaperLogs = new List<DetailBuyPaperLog>();

            // Duyệt qua tất cả các loại giấy và số lượng
            for (int i = 0; i < paperTypeIds.Count; i++)
            {
                int paperTypeId = paperTypeIds[i];
                int quantity = quantities[i];

                if (quantity <= 0) continue;

                var paperType = _context.PaperTypes.FirstOrDefault(pt => pt.PaperTypeId == paperTypeId);
                if (paperType == null) continue;

                decimal amount = paperType.Price * quantity;
                totalAmount += amount;

                // Xử lý số lượng giấy (bao gồm giấy A3)
                int pagesToAdd = paperType.PaperTypeId == 2 ? quantity * 2 : quantity;

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

                // Lưu chi tiết mua giấy vào DetailBuyPaperLog
                detailBuyPaperLogs.Add(new DetailBuyPaperLog
                {
                    PaperTypeId = paperTypeId,
                    PaperBuy = quantity
                });
            }

            // Kiểm tra số dư tài khoản của sinh viên
            if (totalAmount > student.AccountBalance)
            {
                TempData["ErrorMessage"] = "Số dư tài khoản không đủ để thanh toán.";
                return RedirectToAction("BuyPaper");
            }

            // Tạo log mua giấy
            var buyPaperLog = new BuyPaperLog
            {
                StudentId = studentId,
                DateBuy = DateTime.Now,
                TotalBuy = totalAmount
            };

            _context.BuyPaperLogs.Add(buyPaperLog);
            _context.SaveChanges();

            // Lưu các chi tiết mua giấy vào bảng DetailBuyPaperLog
            foreach (var detail in detailBuyPaperLogs)
            {
                detail.BuyPaperLogId = buyPaperLog.BuyPaperLogId;
                _context.DetailBuyPaperLogs.Add(detail);
            }

            // Cập nhật số dư tài khoản của sinh viên
            student.AccountBalance -= totalAmount;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Thanh toán thành công! Số lượng giấy đã được cập nhật.";
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
                return NotFound("Không tìm thấy sinh viên.");
            }

            var buyPaperLogs = _context.BuyPaperLogs
                .Include(log => log.DetailBuyPaperLogs)
                .ThenInclude(detail => detail.PaperType)
                .Where(b => b.StudentId == studentIdFromIdentity)
                .ToList();

            if (buyPaperLogs == null || !buyPaperLogs.Any())
            {
                TempData["ErrorMessage"] = "Không có lịch sử mua giấy.";
            }

            ViewBag.Student = student;
            ViewBag.BuyPaperLogs = buyPaperLogs;

            return View();
        }
    }
}
