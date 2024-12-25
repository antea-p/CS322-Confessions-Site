using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using CS322_PZ_AnteaPrimorac5157.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using CS322_PZ_AnteaPrimorac5157.Extensions;

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
        public async Task<IActionResult> Index(bool sortByLikes = false)
        {
            try
            {
                var confessions = await _confessionService.GetConfessionsAsync(sortByLikes);

                var viewModels = confessions.Select(c => new ConfessionListViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Content = c.Content,
                    DateCreated = c.DateCreated,
                    Likes = c.Likes,
                    CommentCount = c.Comments?.Count ?? 0
                }).ToList();

                ViewData["Message"] = !viewModels.Any() ? "Confessions list is empty!" : null;
                ViewData["CurrentSort"] = sortByLikes;
                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching confessions");
                ViewData["Message"] = "An error occurred while loading confessions.";
                ViewData["CurrentSort"] = false;
                return View(new List<ConfessionListViewModel>());
            }
        }

        // GET: /Confession/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var confession = await _confessionService.GetConfessionAsync(id, includeComments: true);
                if (confession == null)
                {
                    return NotFound();
                }

                bool userHasLiked = HttpContext.Session.HasLiked(id);

                var viewModel = new ConfessionDetailsViewModel
                {
                    Id = confession.Id,
                    Title = confession.Title,
                    Content = confession.Content,
                    DateCreated = confession.DateCreated,
                    Likes = confession.Likes,
                    UserHasLiked = userHasLiked,
                    Comments = confession.Comments.Select(c => new CommentViewModel
                    {
                        Id = c.Id,
                        Content = c.Content,
                        AuthorNickname = c.AuthorNickname,
                        DateCreated = c.DateCreated
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting confession details for ID: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Confession/ToggleLike/5
        [HttpPost]
        public async Task<IActionResult> ToggleLike(int id)
        {
            try
            {
                bool hasLiked = HttpContext.Session.HasLiked(id);
                _logger.LogInformation($"Confession {id} - Current like state: {hasLiked}");

                if (hasLiked)
                {
                    await _confessionService.DecrementLikesAsync(id);
                    HttpContext.Session.SetLiked(id, false);
                    _logger.LogInformation($"Confession {id} - Decremented likes, set liked to false");
                }
                else
                {
                    await _confessionService.IncrementLikesAsync(id);
                    HttpContext.Session.SetLiked(id, true);
                    _logger.LogInformation($"Confession {id} - Incremented likes, set liked to true");
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling like for confession ID: {Id}", id);
                return RedirectToAction(nameof(Details), new { id });
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

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int confessionId, int commentId)
        {
            try
            {
                await _confessionService.DeleteCommentAsync(confessionId, commentId);
                _logger.LogInformation("Comment {CommentId} deleted from confession {ConfessionId}", commentId, confessionId);

                return RedirectToAction(nameof(Details), new { id = confessionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in controller while deleting comment {CommentId} from confession {ConfessionId}", commentId, confessionId);
                return RedirectToAction(nameof(Details), new { id = confessionId });
            }
        }
    }
}