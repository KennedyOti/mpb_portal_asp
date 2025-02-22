using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPB_PORTAL_WEB_APP.Data;
using MPB_PORTAL_WEB_APP.Models;
using MPB_PORTAL_WEB_APP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MPB_PORTAL_WEB_APP.Controllers
{
    [Authorize]
    public class CasesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public CasesController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // INDEX: List and filter cases with Pagination
        public async Task<IActionResult> Index(string clientName, string description, string assignedStaffId, string status, DateTime? startDateFrom, DateTime? startDateTo, int pageSize = 10)
        {
            var cases = _context.Cases.Include(c => c.AssignedStaff).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(clientName))
                cases = cases.Where(c => c.ClientName.Contains(clientName));

            if (!string.IsNullOrEmpty(description))
                cases = cases.Where(c => c.Description.Contains(description));

            if (!string.IsNullOrEmpty(assignedStaffId))
                cases = cases.Where(c => c.AssignedStaffId == assignedStaffId);

            if (!string.IsNullOrEmpty(status))
                cases = cases.Where(c => c.Status == status);

            if (startDateFrom.HasValue)
                cases = cases.Where(c => c.StartDate >= startDateFrom);

            if (startDateTo.HasValue)
                cases = cases.Where(c => c.StartDate <= startDateTo);

            // Pagination logic
            int totalRecords = await cases.CountAsync();
            cases = cases.Take(pageSize); // Only take the number of records based on selected pageSize

            // Fetch staff members for dropdown
            ViewBag.StaffMembers = await _context.Users.ToListAsync();
            ViewData["ClientName"] = clientName;
            ViewData["Description"] = description;
            ViewData["AssignedStaffId"] = assignedStaffId;
            ViewData["Status"] = status;
            ViewData["StartDateFrom"] = startDateFrom?.ToString("yyyy-MM-dd");
            ViewData["StartDateTo"] = startDateTo?.ToString("yyyy-MM-dd");
            ViewData["PageSize"] = pageSize;
            ViewData["TotalRecords"] = totalRecords;

            return View(await cases.ToListAsync());
        }

        // CREATE: Show form
        public async Task<IActionResult> Create()
        {
            ViewBag.StaffMembers = await _context.Users.ToListAsync();
            return View(new CaseViewModel());
        }

        // CREATE: Handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CaseViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            model.ClientName = user?.FullName ?? "Unknown User";  // Ensure it's assigned here and never null

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = string.Join("<br>", errors);
                ViewBag.StaffMembers = await _context.Users.ToListAsync();
                return View(model);
            }

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "attachments");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePaths = new List<string>();
                if (model.Files != null && model.Files.Count > 0)
                {
                    foreach (var file in model.Files)
                    {
                        if (file.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            filePaths.Add(Path.Combine("attachments", uniqueFileName));
                        }
                    }
                }

                var newCase = new Cases
                {
                    ClientName = model.ClientName,  // Correctly assigned
                    Description = model.Description,
                    AssignedStaffId = model.AssignedStaffId,
                    Status = model.Status,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Attachments = filePaths.Count > 0 ? JsonSerializer.Serialize(filePaths) : null
                };

                _context.Cases.Add(newCase);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Case created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the case: {ex.Message}";
                ViewBag.StaffMembers = await _context.Users.ToListAsync();
                return View(model);
            }
        }



        // SHOW: View details
        public async Task<IActionResult> Show(int id)
        {
            var caseItem = await _context.Cases.FindAsync(id);
            if (caseItem == null) return NotFound();

            var attachments = !string.IsNullOrEmpty(caseItem.Attachments)
                ? JsonSerializer.Deserialize<List<string>>(caseItem.Attachments)
                : new List<string>();

            var viewModel = new CaseViewModel
            {
                Id = caseItem.Id,
                ClientName = caseItem.ClientName,
                Description = caseItem.Description,
                AssignedStaffId = caseItem.AssignedStaffId,
                Status = caseItem.Status,
                StartDate = caseItem.StartDate,
                EndDate = caseItem.EndDate,
                ExistingAttachments = attachments
            };
            return View(viewModel);
        }

        // EDIT: Show form
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var caseItem = await _context.Cases.FindAsync(id);
            if (caseItem == null) return NotFound();

            var attachments = !string.IsNullOrEmpty(caseItem.Attachments)
                ? JsonSerializer.Deserialize<List<string>>(caseItem.Attachments)
                : new List<string>();

            var model = new CaseViewModel
            {
                Id = caseItem.Id,
                ClientName = caseItem.ClientName,
                Description = caseItem.Description,
                AssignedStaffId = caseItem.AssignedStaffId,
                Status = caseItem.Status,
                StartDate = caseItem.StartDate,
                EndDate = caseItem.EndDate,
                ExistingAttachments = attachments
            };
            ViewBag.StaffMembers = await _context.Users.ToListAsync();
            return View(model);
        }

        // EDIT: Handle form submission
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CaseViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var caseItem = await _context.Cases.FindAsync(id);
                if (caseItem == null) return NotFound();

                caseItem.Description = model.Description;
                caseItem.AssignedStaffId = model.AssignedStaffId;
                caseItem.Status = model.Status;
                caseItem.StartDate = model.StartDate;
                caseItem.EndDate = model.EndDate;

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "attachments");
                var newAttachments = new List<string>();

                if (model.Files != null && model.Files.Count > 0)
                {
                    foreach (var file in model.Files)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        newAttachments.Add(Path.Combine("attachments", uniqueFileName));
                    }
                }

                if (model.ExistingAttachments != null)
                    newAttachments.AddRange(model.ExistingAttachments);

                caseItem.Attachments = newAttachments.Count > 0 ? JsonSerializer.Serialize(newAttachments) : null;

                _context.Cases.Update(caseItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Case updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.StaffMembers = await _context.Users.ToListAsync();
            return View(model);
        }

        // DELETE: Confirm delete
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var caseItem = await _context.Cases.FindAsync(id);
            if (caseItem == null) return NotFound();
            return View(caseItem);
        }

        // DELETE: Handle delete
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var caseItem = await _context.Cases.FindAsync(id);
            if (caseItem != null)
            {
                if (!string.IsNullOrEmpty(caseItem.Attachments))
                {
                    var attachments = JsonSerializer.Deserialize<List<string>>(caseItem.Attachments);
                    foreach (var filePath in attachments)
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                }
                _context.Cases.Remove(caseItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Case deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
