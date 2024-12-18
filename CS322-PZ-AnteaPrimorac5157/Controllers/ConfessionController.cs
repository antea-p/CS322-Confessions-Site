using CS322_PZ_AnteaPrimorac5157.Data;
using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Ganss.Xss;

namespace CS322_PZ_AnteaPrimorac5157.Controllers
{
    public class ConfessionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly HtmlSanitizer _sanitizer;

        public ConfessionController(ApplicationDbContext context, HtmlSanitizer sanitizer)
        {
            _context = context;
            _sanitizer = sanitizer;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateConfessionViewModel model)
        {
            var sanitizedTitle = _sanitizer.Sanitize(model.Title);
            var sanitizedContent = _sanitizer.Sanitize(model.Content);

            if (string.IsNullOrWhiteSpace(sanitizedTitle) || string.IsNullOrWhiteSpace(sanitizedContent))
            {
                ModelState.AddModelError("", "Your submission contains only HTML tags that will be removed. Please provide actual content.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var confession = new Confession
                {
                    Title = sanitizedTitle,
                    Content = sanitizedContent,
                    DateCreated = DateTime.UtcNow,
                    Likes = 0
                };

                _context.Confessions.Add(confession);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

    }
}