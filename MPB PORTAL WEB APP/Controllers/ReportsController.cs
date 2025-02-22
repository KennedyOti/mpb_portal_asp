using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPB_PORTAL_WEB_APP.Data;
using MPB_PORTAL_WEB_APP.Models;
using MPB_PORTAL_WEB_APP.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MPB_PORTAL_WEB_APP.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public ReportsController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // INDEX - List and Filter Reports
        public async Task<IActionResult> Index(string title, string status, string reportMadeBy, DateTime? dateFrom, DateTime? dateTo)
        {
            var reports = _context.Reports.AsQueryable();

            if (!string.IsNullOrEmpty(title))
                reports = reports.Where(r => r.Title.Contains(title));

            if (!string.IsNullOrEmpty(status))
                reports = reports.Where(r => r.Status == status);

            if (!string.IsNullOrEmpty(reportMadeBy))
                reports = reports.Where(r => r.Report_Made_By.Contains(reportMadeBy));

            if (dateFrom.HasValue)
                reports = reports.Where(r => r.DateOfReport >= dateFrom);

            if (dateTo.HasValue)
                reports = reports.Where(r => r.DateOfReport <= dateTo);

            return View(await reports.ToListAsync());
        }

        // CREATE - Show Form
        public IActionResult Create() => View(new ReportViewModel());

        // CREATE - Handle Form Submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = string.Join("<br>", errors); // Display all validation errors
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                var report = new Report
                {
                    Title = model.Title,
                    DateOfReport = model.DateOfReport,
                    FilePath = Path.Combine("reports", uniqueFileName),  // Store relative path
                    Report_Made_By = user.FullName,
                    Status = model.Status
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Report submitted successfully.";
                return RedirectToAction(nameof(Create));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while saving the report: {ex.Message}";
                return View(model);
            }
        }

        // SHOW - Report Details
        public async Task<IActionResult> Show(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();
            return View(report);
        }

        // EDIT - Show Form
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            var model = new ReportViewModel
            {
                Id = report.Id,
                Title = report.Title,
                DateOfReport = report.DateOfReport,
                Status = report.Status ?? "Submitted", // Default to "Submitted" if null
                FilePath = report.FilePath
            };
            return View(model);
        }

        // EDIT - Save Changes
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReportViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Invalid Report ID.";
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Form validation failed. Please check all fields and try again.";
                return View(model);
            }

            try
            {
                var report = await _context.Reports.FindAsync(id);
                if (report == null)
                {
                    TempData["ErrorMessage"] = "Report not found.";
                    return NotFound();
                }

                // Update Report Fields
                report.Title = model.Title;
                report.DateOfReport = model.DateOfReport;
                report.Status = model.Status ?? "submitted"; // Ensures status is not null

                // Handle File Upload
                if (model.File != null)
                {
                    var uploadsFolder = Path.Combine("wwwroot/reports");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.File.CopyToAsync(stream);
                    }

                    report.FilePath = "reports/" + fileName;
                }

                _context.Reports.Update(report);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Report updated successfully.";
                return RedirectToAction(nameof(Edit), new { id = report.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the report: {ex.Message}";
                return View(model);
            }
        }


        // DELETE - Confirm
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();
            return View(report);
        }

        // DELETE - Execute
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                System.IO.File.Delete(Path.Combine("wwwroot", report.FilePath));
                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Report deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
