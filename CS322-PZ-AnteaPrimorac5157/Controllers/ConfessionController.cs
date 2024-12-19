using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using CS322_PZ_AnteaPrimorac5157.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace CS322_PZ_AnteaPrimorac5157.Controllers
{
    public class ConfessionController : Controller
    {
        private readonly IConfessionService _confessionService;
        private readonly ILogger<ConfessionController> _logger;

        public ConfessionController(
            IConfessionService confessionService,
            ILogger<ConfessionController> logger)
        {
            _confessionService = confessionService;
            _logger = logger;
        }

        // GET: /Confession
        public async Task<IActionResult> Index()
        {
            try
            {
                var confessions = await _confessionService.GetConfessionsAsync();

                var viewModels = confessions.Select(c => new ConfessionListViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Content = c.Content,
                    DateCreated = c.DateCreated,
                    Likes = c.Likes,
                    CommentCount = c.Comments?.Count ?? 0
                }).OrderByDescending(c => c.DateCreated).ToList();

                if (!viewModels.Any())
                {
                    ViewData["Message"] = "Confessions list is empty!";
                }

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching confessions");
                ViewData["Message"] = "An error occurred while loading confessions.";
                return View(new List<ConfessionListViewModel>());
            }
        }

        // GET: /Confession/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Confession/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateConfessionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _confessionService.CreateConfessionAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating confession");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        // POST: /Confession/Delete/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var confession = await _confessionService.GetConfessionAsync(id);
                if (confession == null)
                {
                    _logger.LogWarning("Attempt to delete non-existent confession with ID: {Id}", id);
                    return NotFound();
                }

                await _confessionService.DeleteConfessionAsync(id);
                _logger.LogInformation("Confession with ID: {Id} was successfully deleted", id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting confession with ID: {Id}", id);
                return StatusCode(500, "An error occurred while deleting the confession.");
            }
        }
    }
}