using CS322_PZ_AnteaPrimorac5157.Data;
using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Ganss.Xss;
using CS322_PZ_AnteaPrimorac5157.Services;
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

        public IActionResult Create()
        {
            return View();
        }

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
                return RedirectToAction("Index", "Home");
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
    }
}