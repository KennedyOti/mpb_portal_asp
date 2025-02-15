using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MPB_PORTAL_WEB_APP.Models;
using MPB_PORTAL_WEB_APP.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace MPB_PORTAL_WEB_APP.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<Users> _userManager;

        public ProfileController(UserManager<Users> userManager)
        {
            _userManager = userManager;
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new ProfileViewModel
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User"
            };

            return View(model);
        }

        // POST: Profile/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Edit", model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            user.FullName = model.Name;
            user.Email = model.Email;

            // If the admin changes the role
            if (User.IsInRole("Admin") && !string.IsNullOrEmpty(model.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Edit", new { id = model.Id });
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View("Edit", model);
        }
    }
}
