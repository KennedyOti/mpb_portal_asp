using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPB_PORTAL_WEB_APP.Data;
using MPB_PORTAL_WEB_APP.Models;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<Users> _userManager;

    public DashboardController(AppDbContext context, UserManager<Users> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.RegisteredUsers = await _userManager.Users.CountAsync();
        ViewBag.TotalActivities = await _context.Activities.CountAsync();
        ViewBag.TotalCases = await _context.Cases.CountAsync();
        ViewBag.PendingCases = await _context.Cases.CountAsync(c => c.Status == "pending");
        ViewBag.ResolvedCases = await _context.Cases.CountAsync(c => c.Status == "resolved");
        ViewBag.TotalReports = await _context.Reports.CountAsync();

        return View();
    }
}
