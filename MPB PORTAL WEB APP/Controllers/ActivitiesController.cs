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
        public IActionResult Index(string ActivityName, string Organization, string Status, DateTime? StartDate, DateTime? EndDate)
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

            ViewData["ActivityName"] = ActivityName;
            ViewData["Organization"] = Organization;
            ViewData["Status"] = Status;
            ViewData["StartDate"] = StartDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = EndDate?.ToString("yyyy-MM-dd");

            return View(activities.ToList());
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
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
                return View(activity);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrEmpty(user.FullName))
            {
                ModelState.AddModelError("", "User Full Name is required.");
                return View(activity);
            }

            // Assign the logged-in user's full name to the Organization field
            activity.Organization = user.FullName;

            try
            {
                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Activity registered successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                ModelState.AddModelError("", "Failed to save activity.");
                return View(activity);
            }

            return RedirectToAction(nameof(Create));
        }

        // EDIT - Show Form
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var activity = _context.Activities.Find(id);
            if (activity == null) return NotFound();
            return View(activity);
        }

        // EDIT - Save Changes
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CountyActivity activity)
        {
            if (id != activity.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(activity);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Activity updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Activities.Any(e => e.Id == activity.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(activity);
        }

        // DELETE - Confirm
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
        public IActionResult Show(int id)
        {
            var activity = _context.Activities.Find(id);
            if (activity == null) return NotFound();
            return View(activity);
        }
    }
}
