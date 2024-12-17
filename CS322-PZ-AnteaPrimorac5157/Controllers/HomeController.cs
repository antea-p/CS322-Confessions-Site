using CS322_PZ_AnteaPrimorac5157.Data;
using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using Ganss.Xss;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CS322_PZ_AnteaPrimorac5157.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly HtmlSanitizer _sanitizer;

		public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
		{
			_logger = logger;
			_context = context;
			_sanitizer = new HtmlSanitizer();

			// Dozvoli osnovno formatiranje
			_sanitizer.AllowedTags.Clear();
			_sanitizer.AllowedTags.Add("b");
			_sanitizer.AllowedTags.Add("i");
			_sanitizer.AllowedTags.Add("br");
			_sanitizer.AllowedTags.Add("p");
		}

		public IActionResult Index()
		{
			var confessions = _context.Confessions
				.Where(c => c != null)
				.Select(c => new ConfessionListViewModel
				{
					Id = c.Id,
					Title = c.Title ?? string.Empty,
					Content = c.Content ?? string.Empty,
					DateCreated = c.DateCreated,
					Likes = c.Likes,
					CommentCount = c.Comments.Count
				})
				.OrderByDescending(c => c.DateCreated)
				.ToList();

			var validConfessions = confessions
					   .Select(confession =>
					   {
						   var sanitizedTitle = _sanitizer.Sanitize(confession.Title).Trim();
						   var sanitizedContent = _sanitizer.Sanitize(confession.Content).Trim();

						   confession.Title = sanitizedTitle;
						   confession.Content = sanitizedContent;

						   return confession;
					   })
					   // Ne prikazuj ispovijesti koje su prazne nakon sanitizacije
					   // (npr. slučaj gdje je sadržaj ispovijesti "<script>alert("Hi")</script>)
					   .Where(confession =>
						   !string.IsNullOrWhiteSpace(confession.Title) ||
						   !string.IsNullOrWhiteSpace(confession.Content))
					   .ToList();

			if (!confessions.Any())
			{
				ViewData["Message"] = "Confessions list is empty!";
			}

			return View(validConfessions);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
