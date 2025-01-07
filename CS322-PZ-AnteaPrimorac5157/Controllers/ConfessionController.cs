using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using CS322_PZ_AnteaPrimorac5157.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using CS322_PZ_AnteaPrimorac5157.Extensions;
using Microsoft.EntityFrameworkCore;

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

        private IActionResult RedirectToDetailsAction(int id) =>
            RedirectToAction(nameof(Details), new { id });

        private async Task<ConfessionDetailsViewModel?> GetConfessionDetailsViewModel(int id, CreateCommentViewModel? commentModel = null)
        {
            var confession = await _confessionService.GetConfessionAsync(id, includeComments: true);
            if (confession == null) return null;

            return new ConfessionDetailsViewModel
            {
                Id = confession.Id,
                Title = confession.Title,
                Content = confession.Content,
                DateCreated = confession.DateCreated,
                Likes = confession.Likes,
                UserHasLiked = HttpContext.Session.HasLikedConfession(id),
                Comments = confession.Comments.Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    AuthorNickname = c.AuthorNickname,
                    DateCreated = c.DateCreated,
                    Likes = c.Likes,
                    UserHasLiked = HttpContext.Session.HasLikedComment(c.Id)
                }).ToList(),
                NewComment = commentModel ?? new CreateCommentViewModel { ConfessionId = id }
            };
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

        public async Task<IActionResult> Search(string searchTerm)
        {
            try
            {
                var searchResults = await _confessionService.SearchConfessionsAsync(searchTerm);

                ViewData["SearchTerm"] = searchTerm;
                ViewData["Message"] = searchResults.Any()
                    ? null
                    : $"No confessions found matching '{searchTerm}'";

                var viewModels = searchResults.Select(c => new ConfessionListViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Content = c.Content,
                    DateCreated = c.DateCreated,
                    Likes = c.Likes,
                    CommentCount = c.Comments?.Count ?? 0
                }).ToList();

                return View(nameof(Index), viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching confessions");
                ViewData["Message"] = "An error occurred while searching confessions.";
                return View(nameof(Index), new List<ConfessionListViewModel>());
            }
        }

        // GET: /Confession/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var viewModel = await GetConfessionDetailsViewModel(id);
            if (viewModel == null) return NotFound();
            return View(viewModel);
        }

        // POST: /Confession/ToggleLike/5
        [HttpPost]
        public async Task<IActionResult> ToggleLike(int id)
        {
            try
            {
                bool hasLiked = HttpContext.Session.HasLikedConfession(id);
                _logger.LogInformation($"Confession {id} - Current like state: {hasLiked}");

                if (hasLiked)
                {
                    await _confessionService.DecrementLikesAsync(id);
                    HttpContext.Session.SetConfessionLiked(id, false);
                    _logger.LogInformation($"Confession {id} - Decremented likes, set liked to false");
                }
                else
                {
                    await _confessionService.IncrementLikesAsync(id);
                    HttpContext.Session.SetConfessionLiked(id, true);
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var confession = await _confessionService.GetConfessionAsync(id);
            if (confession == null) return NotFound();

            var viewModel = new EditConfessionViewModel
            {
                Id = confession.Id,
                Title = confession.Title,
                Content = confession.Content,
                RowVersion = confession.RowVersion
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditConfessionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _confessionService.UpdateConfessionAsync(model);
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (ValidationException ex)
            {
                // Dodaj validacijske greške
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Another admin has modified this confession. Please reload and try again.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating confession");
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int id, CreateCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ConfessionId = id;
                var viewModel = await GetConfessionDetailsViewModel(id, model);
                if (viewModel == null) return NotFound();
                return View("Details", viewModel);
            }

            try
            {
                await _confessionService.AddCommentAsync(id, model);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var viewModel = await GetConfessionDetailsViewModel(id, model);
                if (viewModel == null) return NotFound();
                return View("Details", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding comment to confession {ConfessionId}", id);
                return RedirectToAction(nameof(Details), new { id });
            };
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditComment(int confessionId, int commentId)
        {
            var comment = await _confessionService.GetCommentAsync(commentId);
            if (comment == null || comment.ConfessionId != confessionId)
                return NotFound();

            var viewModel = new EditCommentViewModel
            {
                Id = comment.Id,
                ConfessionId = comment.ConfessionId,
                Content = comment.Content,
                AuthorNickname = comment.AuthorNickname
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(EditCommentViewModel model)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            try
            {
                await _confessionService.UpdateCommentAsync(model);

                // Vrati sanitiziranu verziju komentara iz baze podataka
                var updatedComment = await _confessionService.GetCommentAsync(model.Id);
                if (updatedComment == null)
                {
                    return Json(new { success = false, message = "Comment not found" });
                }

                return Json(new
                {
                    success = true,
                    content = updatedComment.Content,        // Već sanitizirano
                    authorNickname = updatedComment.AuthorNickname
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", model.Id);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while updating the comment"
                });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCommentLike(int confessionId, int commentId)
        {
            try
            {
                bool hasLiked = HttpContext.Session.HasLikedComment(commentId);
                _logger.LogInformation($"Comment {commentId} - Current like state: {hasLiked}");

                if (hasLiked)
                {
                    await _confessionService.DecrementCommentLikesAsync(commentId);
                    HttpContext.Session.SetCommentLiked(commentId, false);
                    _logger.LogInformation($"Comment {commentId} - Decremented likes, set liked to false");
                }
                else
                {
                    await _confessionService.IncrementCommentLikesAsync(commentId);
                    HttpContext.Session.SetCommentLiked(commentId, true);
                    _logger.LogInformation($"Comment {commentId} - Incremented likes, set liked to true");
                }

                return RedirectToAction(nameof(Details), new { id = confessionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling like for comment ID: {CommentId}", commentId);
                return RedirectToAction(nameof(Details), new { id = confessionId });
            }
        }
    }
}