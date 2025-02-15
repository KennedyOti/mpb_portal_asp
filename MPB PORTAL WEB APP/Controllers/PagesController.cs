using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPB_PORTAL_WEB_APP.Data;
using MPB_PORTAL_WEB_APP.Models;
using MPB_PORTAL_WEB_APP.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace MPB_PORTAL_WEB_APP.Controllers
{
    [Authorize]
    public class PagesController : Controller
    {
        private readonly AppDbContext _context;

        public PagesController(AppDbContext context)
        {
            _context = context;
        }

        // Index: List pages with filters
        public async Task<IActionResult> Index(string title, string metaTitle)
        {
            var pages = _context.Pages.AsQueryable();

            if (!string.IsNullOrEmpty(title))
                pages = pages.Where(p => p.Title.Contains(title));

            if (!string.IsNullOrEmpty(metaTitle))
                pages = pages.Where(p => p.MetaTitle.Contains(metaTitle));

            return View(await pages.ToListAsync());
        }

        // Create: Show form
        public IActionResult Create() => View(new PageViewModel());

        // Create: Handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var page = new Pages
                {
                    Title = model.Title,
                    MetaTitle = model.MetaTitle,
                    MetaDescription = model.MetaDescription,
                    Content = model.Content
                };

                _context.Pages.Add(page);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Page created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // Show details
        public async Task<IActionResult> Show(int id)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page == null) return NotFound();
            return View(page);
        }

        // Edit: Show form
        public async Task<IActionResult> Edit(int id)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page == null) return NotFound();

            var model = new PageViewModel
            {
                Id = page.Id,
                Title = page.Title,
                MetaTitle = page.MetaTitle,
                MetaDescription = page.MetaDescription,
                Content = page.Content
            };
            return View(model);
        }

        // Edit: Handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PageViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var page = await _context.Pages.FindAsync(id);
                if (page == null) return NotFound();

                page.Title = model.Title;
                page.MetaTitle = model.MetaTitle;
                page.MetaDescription = model.MetaDescription;
                page.Content = model.Content;

                _context.Pages.Update(page);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Page updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // Delete: Show confirmation
        public async Task<IActionResult> Delete(int id)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page == null) return NotFound();
            return View(page);
        }

        // Delete: Handle deletion
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page != null)
            {
                _context.Pages.Remove(page);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Page deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
