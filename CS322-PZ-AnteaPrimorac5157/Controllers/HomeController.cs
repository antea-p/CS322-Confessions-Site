using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using CS322_PZ_AnteaPrimorac5157.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CS322_PZ_AnteaPrimorac5157.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfessionRepository _repository;

        public HomeController(
            ILogger<HomeController> logger,
            IConfessionRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var confessions = await _repository.GetAllAsync();

                var viewModels = confessions.Select(c => new ConfessionListViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Content = c.Content,
                    DateCreated = c.DateCreated,
                    Likes = c.Likes,
                    CommentCount = c.Comments?.Count ?? 0
                })
                .OrderByDescending(c => c.DateCreated)
                .ToList();

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}