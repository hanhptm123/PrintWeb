using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrintWeb.Data;
using X.PagedList;
using X.PagedList.Extensions;
using X.PagedList.Mvc;  // For pagination helper methods
using X.PagedList.Mvc.Core;// For pagination helper methods


namespace PrintWeb.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly PrintwebContext _context;

        public DocumentsController(PrintwebContext context)
        {
            _context = context;
        }

     

public async Task<IActionResult> Index(int page = 1)
    {
        var studentId = HttpContext.Session.GetString("AccountId");
        if (string.IsNullOrEmpty(studentId))
        {
            return RedirectToAction("Login", "Accounts");
        }

        int pageSize = 5;  // Number of documents per page

            // Fetch documents and apply paging
            var documents = _context.Documents
                              .Where(d => d.StudentId == studentId)
                              .Include(d => d.Student)
                              .OrderBy(d => d.DocumentId)
                              .ToPagedList(page, pageSize);  // Use ToPagedList (not async)

            return View(documents);  // Return the paged documents
    }




    // GET: Documents/Details/5
    public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Student)
                .FirstOrDefaultAsync(m => m.DocumentId == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // GET: Documents/Create
        public IActionResult Create()
        {
            // Lấy studentId từ session
            var studentId = HttpContext.Session.GetString("AccountId");

            // Nếu không có studentId trong session, chuyển hướng về trang login
            if (string.IsNullOrEmpty(studentId))
            {
                return RedirectToAction("Login", "Accounts");
            }

            // Truyền studentId vào ViewData (hoặc có thể bỏ qua phần SelectList nếu chỉ có 1 studentId)
            ViewData["StudentId"] = studentId;

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file)
        {
            if (file == null || (file.ContentType != "application/pdf" && file.ContentType != "application/vnd.openxmlformats-officedocument.wordprocessingml.document"))
            {
                TempData["FileError"] = "Please upload a file in PDF or DOCX format.";
                return RedirectToAction("Create");
            }

            // Lấy studentId từ session
            var studentId = HttpContext.Session.GetString("AccountId");

            if (string.IsNullOrEmpty(studentId))
            {
                TempData["ErrorMessage"] = "Please log in first.";
                return RedirectToAction("Login", "Accounts");
            }

            // Xác định loại file
            string fileType = file.ContentType == "application/pdf" ? "PDF" : "DOCX";

            // Tạo đối tượng Document mới
            var document = new Document
            {
                StudentId = studentId,
                FileName = file.FileName,
                FileType = fileType,  // Lưu loại file (PDF hoặc DOCX)
                UploadedAt = DateTime.Now
            };

            // Xác định đường dẫn thư mục lưu file
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents");

            // Kiểm tra nếu thư mục chưa tồn tại thì tạo mới
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Tạo đường dẫn đầy đủ để lưu file
            var filePath = Path.Combine(directoryPath, file.FileName);

            // Lưu file vào thư mục trên server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Thêm document vào context và lưu vào cơ sở dữ liệu
            _context.Add(document);
            await _context.SaveChangesAsync();

            // Thông báo và chuyển hướng
            TempData["SuccessMessage"] = "File uploaded successfully!";
            return RedirectToAction(nameof(Index));
        }


        // GET: Documents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "StudentId", document.StudentId);
            return View(document);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DocumentId,StudentId,FileName,FileType,UploadedAt")] Document document)
        {
            if (id != document.DocumentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.DocumentId))
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
            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "StudentId", document.StudentId);
            return View(document);
        }

        // GET: Documents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Student)
                .FirstOrDefaultAsync(m => m.DocumentId == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Documents
                .Include(d => d.Student) // Bao gồm thông tin Student nếu cần
                .FirstOrDefaultAsync(m => m.DocumentId == id);

            if (document != null)
            {
                // Xóa các bản ghi liên quan từ bảng PrintingLog
                var relatedPrintingLogs = _context.PrintingLogs
                    .Where(r => r.DocumentId == id); // Tìm các bản ghi liên quan đến DocumentId

                _context.PrintingLogs.RemoveRange(relatedPrintingLogs); // Xóa các bản ghi liên quan từ PrintingLog

                // Sau khi xóa các bản ghi liên quan, xóa tài liệu chính
                _context.Documents.Remove(document);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.DocumentId == id);
        }
    }
}
