using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPB_PORTAL_WEB_APP.Data;
using MPB_PORTAL_WEB_APP.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MPB_PORTAL_WEB_APP.Controllers
{
    [Authorize]
    public class ActivitiesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public ActivitiesController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // INDEX - List and Filter Activities
        public IActionResult Index(string ActivityName, string Organization, string Status, DateTime? StartDate, DateTime? EndDate, int pageNumber = 1, int pageSize = 10)
        {
            var activities = _context.Activities.AsQueryable();

            if (!string.IsNullOrEmpty(ActivityName))
                activities = activities.Where(a => a.ActivityName.Contains(ActivityName));

            if (!string.IsNullOrEmpty(Organization))
                activities = activities.Where(a => a.Organization.Contains(Organization));

            if (!string.IsNullOrEmpty(Status))
                activities = activities.Where(a => a.Status == Status);

            if (StartDate.HasValue)
                activities = activities.Where(a => a.StartDate >= StartDate);

            if (EndDate.HasValue)
                activities = activities.Where(a => a.EndDate <= EndDate);

            // Pagination
            int totalRecords = activities.Count();
            var paginatedActivities = activities
                .OrderBy(a => a.StartDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewData["ActivityName"] = ActivityName;
            ViewData["Organization"] = Organization;
            ViewData["Status"] = Status;
            ViewData["StartDate"] = StartDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = EndDate?.ToString("yyyy-MM-dd");
            ViewData["TotalRecords"] = totalRecords;
            ViewData["PageNumber"] = pageNumber;
            ViewData["PageSize"] = pageSize;

            return View(paginatedActivities);
        }

        // CREATE - Show Form
        public IActionResult Create()
        {
            return View(new CountyActivity());
        }

        // CREATE - Save New Activity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CountyActivity activity)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View(activity);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrEmpty(user.FullName))
            {
                ModelState.AddModelError("", "User Full Name is required.");
                return View(activity);
            }

            // Assign the logged-in user's full name to Organization
            activity.Organization = user.FullName;

            try
            {
                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Activity registered successfully.";
                return RedirectToAction(nameof(Create));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error occurred: {ex.Message}";
                return View(activity);
            }
        }

        // EDIT - Show Form
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
            {
                TempData["ErrorMessage"] = "Activity not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(activity);
        }

        // EDIT - Save Changes (Ensuring Organization is NOT Updated)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CountyActivity updatedActivity)
        {
            if (id != updatedActivity.Id)
            {
                TempData["ErrorMessage"] = "Invalid Activity ID.";
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View(updatedActivity);
            }

            try
            {
                var existingActivity = await _context.Activities.FindAsync(id);
                if (existingActivity == null)
                {
                    TempData["ErrorMessage"] = "Activity not found.";
                    return NotFound();
                }

                // Update fields except Organization
                existingActivity.ActivityName = updatedActivity.ActivityName;
                existingActivity.ActivityDescription = updatedActivity.ActivityDescription;
                existingActivity.Location = updatedActivity.Location;
                existingActivity.StartDate = updatedActivity.StartDate;
                existingActivity.EndDate = updatedActivity.EndDate;
                existingActivity.ActualBeneficiaries = updatedActivity.ActualBeneficiaries;
                existingActivity.ExpectedBeneficiaries = updatedActivity.ExpectedBeneficiaries;
                existingActivity.Status = updatedActivity.Status;

                _context.Update(existingActivity);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Activity updated successfully.";
                return RedirectToAction(nameof(Edit), new { id = existingActivity.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Activities.Any(e => e.Id == updatedActivity.Id))
                {
                    TempData["ErrorMessage"] = "Activity no longer exists.";
                    return NotFound();
                }
                else
                {
                    TempData["ErrorMessage"] = "Concurrency error. Please try again.";
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(updatedActivity);
            }
        }
        // EDIT - Save Changes 
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var activity = _context.Activities.Find(id);
            if (activity == null) return NotFound();
            return View(activity);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity != null)
            {
                _context.Activities.Remove(activity);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Activity deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        // SHOW - Activity Details
        public async Task<IActionResult> Show(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
            {
                TempData["ErrorMessage"] = "Activity not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(activity);
        }
    }
}
