using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.Repositories;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using Ganss.Xss;
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
    }
}
