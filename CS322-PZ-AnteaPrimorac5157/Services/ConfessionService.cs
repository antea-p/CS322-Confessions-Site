using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.Repositories;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CS322_PZ_AnteaPrimorac5157.Services
{
    public class ConfessionService : IConfessionService
    {
        private readonly IConfessionRepository _repository;
        private readonly HtmlSanitizer _sanitizer;
        private readonly ILogger<ConfessionService> _logger;

        public ConfessionService(
            IConfessionRepository repository,
            HtmlSanitizer sanitizer,
            ILogger<ConfessionService> logger)
        {
            _repository = repository;
            _sanitizer = sanitizer;
            _logger = logger;
        }

        public async Task<IEnumerable<Confession>> GetConfessionsAsync(bool orderByLikes = false)
        {
            try
            {
                var confessions = await _repository.GetAllAsync();

                if (orderByLikes)
                {
                    return confessions.OrderByDescending(c => c.Likes);
                }

                return confessions.OrderByDescending(c => c.DateCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all confessions");
                throw;
            }
        }

        public async Task<Confession?> GetConfessionAsync(int id, bool includeComments = false)
        {
            try
            {
                return await _repository.GetByIdAsync(id, includeComments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting confession with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Confession>> SearchConfessionsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Search term cannot be empty.");
            }

            try
            {
                return await _repository.SearchConfessionsAsync(searchTerm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service while searching confessions");
                throw;
            }
        }

        public async Task<Confession> CreateConfessionAsync(CreateConfessionViewModel model)
        {
            try
            {
                var sanitizedTitle = _sanitizer.Sanitize(model.Title);
                var sanitizedContent = _sanitizer.Sanitize(model.Content);

                // Provjera teksta naslova i sadržaja nakon sanitizacije
                var titleWithoutTags = Regex.Replace(sanitizedTitle, "<.*?>", string.Empty).Trim();
                var contentWithoutTags = Regex.Replace(sanitizedContent, "<.*?>", string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(titleWithoutTags))
                {
                    throw new ValidationException("Title cannot be empty or contain only HTML tags.");
                }

                if (string.IsNullOrWhiteSpace(contentWithoutTags))
                {
                    throw new ValidationException("Content cannot be empty or contain only HTML tags.");
                }

                var confession = new Confession
                {
                    Title = sanitizedTitle,
                    Content = sanitizedContent,
                    DateCreated = DateTime.UtcNow,
                    Likes = 0
                };

                return await _repository.AddAsync(confession);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating confession");
                throw new ApplicationException("Failed to create confession", ex);
            }
        }

        public async Task DeleteConfessionAsync(int id)
        {
            try
            {
                var confession = await _repository.GetByIdAsync(id);
                if (confession == null)
                {
                    throw new InvalidOperationException($"Confession with ID {id} not found");
                }

                await _repository.DeleteAsync(confession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting confession with ID: {Id}", id);
                throw;
            }
        }

        public async Task<Comment?> GetCommentAsync(int commentId)
        {
            try
            {
                return await _repository.GetCommentAsync(commentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting comment with ID: {Id}", commentId);
                throw;
            }
        }

        public async Task AddCommentAsync(int confessionId, CreateCommentViewModel model)
        {
            var sanitizedContent = _sanitizer.Sanitize(model.Content);
            var sanitizedNickname = _sanitizer.Sanitize(model.AuthorNickname);

            if (string.IsNullOrWhiteSpace(sanitizedContent))
                throw new ValidationException("Comment content cannot be empty or contain only HTML tags.");
            if (string.IsNullOrWhiteSpace(sanitizedNickname))
                throw new ValidationException("Author nickname cannot be empty or contain only HTML tags.");

            var comment = new Comment
            {
                Content = sanitizedContent,
                AuthorNickname = sanitizedNickname,
                ConfessionId = confessionId,
                DateCreated = DateTime.UtcNow
            };

            try
            {
                await _repository.AddCommentAsync(comment);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error adding comment to confession {Id}", confessionId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to confession {Id}", confessionId);
                throw;
            }
        }

        public async Task UpdateCommentAsync(EditCommentViewModel model)
        {
            try
            {
                var comment = await GetCommentAsync(model.Id);
                if (comment == null)
                    throw new InvalidOperationException($"Comment with ID {model.Id} not found");

                var sanitizedContent = _sanitizer.Sanitize(model.Content);
                var sanitizedNickname = _sanitizer.Sanitize(model.AuthorNickname);

                if (string.IsNullOrWhiteSpace(sanitizedContent))
                    throw new ValidationException("Comment content cannot be empty");
                if (string.IsNullOrWhiteSpace(sanitizedNickname))
                    throw new ValidationException("Author nickname cannot be empty");

                comment.Content = sanitizedContent;
                comment.AuthorNickname = sanitizedNickname;

                await _repository.UpdateCommentAsync(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", model.Id);
                throw;
            }
        }

        public async Task DeleteCommentAsync(int confessionId, int commentId)
        {
            try
            {
                await _repository.DeleteCommentAsync(confessionId, commentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in service while deleting comment {CommentId} from confession {ConfessionId}", commentId, confessionId);
                throw;
            }
        }

        public async Task IncrementLikesAsync(int id)
        {
            try
            {
                await _repository.IncrementLikesAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while incrementing likes for confession ID: {Id}", id);
                throw;
            }
        }

        public async Task DecrementLikesAsync(int id)
        {
            try
            {
                await _repository.DecrementLikesAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while decrementing likes for confession ID: {Id}", id);
                throw;
            }
        }
    }
}
