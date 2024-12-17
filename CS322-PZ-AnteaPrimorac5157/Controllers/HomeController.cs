using CS322_PZ_AnteaPrimorac5157.Data;
using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CS322_PZ_AnteaPrimorac5157.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _context;

		public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
		{
			_logger = logger;
			_context = context;
		}

		public IActionResult Index()
		{
			var confessions = _context.Confessions
				.Where(c => c != null)
				.Select(c => new ConfessionListViewModel
				{
					Id = c.Id,
					Title = c.Title,
					Content = c.Content,
					DateCreated = c.DateCreated,
					Likes = c.Likes,
					CommentCount = c.Comments.Count
				})
				.OrderByDescending(c => c.DateCreated)
				.ToList();

			if (!confessions.Any())
			{
				ViewData["Message"] = "No confessions yet. Be the first to confess!";
			}

			return View(confessions);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
