using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MPB_PORTAL_WEB_APP.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MPB_PORTAL_WEB_APP.Controllers
{
    [Authorize(Roles = "Admin")]  // Only admin can manage users
    public class UsersController : Controller
    {
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UsersController(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        // GET: List Users
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            var users = userManager.Users.ToList();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                user.Role = roles.FirstOrDefault() ?? "No Role";
            }

            // Pagination Logic
            var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling(users.Count / (double)pageSize);

            return View(pagedUsers);
        }
    }
}
