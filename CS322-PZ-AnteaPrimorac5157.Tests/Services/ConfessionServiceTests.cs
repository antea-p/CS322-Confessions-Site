using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.Repositories;
using CS322_PZ_AnteaPrimorac5157.Services;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using Ganss.Xss;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS322_PZ_AnteaPrimorac5157.Tests.Services
{
    public class ConfessionServiceTests
    {
        private readonly Mock<IConfessionRepository> _repositoryMock;
        private readonly Mock<ILogger<ConfessionService>> _loggerMock;
        private readonly HtmlSanitizer _sanitizer;
        private readonly ConfessionService _service;

        public ConfessionServiceTests()
        {
            _repositoryMock = new Mock<IConfessionRepository>();
            _loggerMock = new Mock<ILogger<ConfessionService>>();
            _sanitizer = new HtmlSanitizer();

            _sanitizer.AllowedTags.Clear();
            _sanitizer.AllowedTags.Add("b");
            _sanitizer.AllowedTags.Add("i");
            _sanitizer.AllowedTags.Add("u");
            _sanitizer.AllowedTags.Add("s");

            _service = new ConfessionService(
                _repositoryMock.Object,
                _sanitizer,
                _loggerMock.Object);
        }

        [Fact]
        public async Task CreateConfessionAsync_WithValidInput_CreatesConfession()
        {
            // Arrange
            var model = new CreateConfessionViewModel
            {
                Title = "Test Title",
                Content = "Test Content"
            };

            var expectedConfession = new Confession
            {
                Id = 1,
                Title = "Test Title",
                Content = "Test Content",
                Likes = 0
            };

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Confession>()))
                .ReturnsAsync(expectedConfession);

            // Act
            var result = await _service.CreateConfessionAsync(model);

            // Assert
            Assert.Equal(expectedConfession.Id, result.Id);
            Assert.Equal(expectedConfession.Title, result.Title);
            Assert.Equal(expectedConfession.Content, result.Content);
            Assert.Equal(expectedConfession.Likes, result.Likes);
        }

        [Theory]
        [InlineData("<script>alert('xss')</script>Test", "Test")]
        [InlineData("<b>Bold</b>", "<b>Bold</b>")]
        public async Task CreateConfessionAsync_SanitizesHtmlContent(
            string input, string expected)
        {
            // Arrange
            var model = new CreateConfessionViewModel
            {
                Title = "Test Title",
                Content = input
            };

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Confession>()))
                .ReturnsAsync((Confession c) => c);

            // Act
            var result = await _service.CreateConfessionAsync(model);

            // Assert
            Assert.Equal(expected, result.Content);
        }

        [Fact]
        public async Task CreateConfessionAsync_WithEmptyContent_ThrowsValidationException()
        {
            // Arrange
            var model = new CreateConfessionViewModel
            {
                Title = "Test Title",
                Content = "<script></script>"  // Prazno nakon sanitizacije
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateConfessionAsync(model));
        }

        [Theory]
        [InlineData("", "Valid Content")]
        [InlineData("   ", "Valid Content")]
        [InlineData("<b></b>", "Valid Content")]
        [InlineData("<i></i>", "Valid Content")]
        [InlineData("<u></u>", "Valid Content")]
        [InlineData("<s></s>", "Valid Content")]
        [InlineData("Valid Title", "")]
        [InlineData("Valid Title", "   ")]
        [InlineData("Valid Title", "<b></b>")]
        [InlineData("Valid Title", "<i></i>")]
        [InlineData("Valid Title", "<u></u>")]
        [InlineData("Valid Title", "<s></s>")]
        public async Task CreateConfessionAsync_WithInvalidInput_ThrowsValidationException(
       string title, string content)
        {
            // Arrange
            var model = new CreateConfessionViewModel
            {
                Title = title,
                Content = content
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateConfessionAsync(model));
            Assert.Contains("cannot be empty", exception.Message);
        }

        [Fact]
        public async Task CreateConfessionAsync_WithVeryLongContent_PreservesContent()
        {
            // Arrange
            var longContent = new string('a', 2000); // Maksimalna dužina sadržaja
            var model = new CreateConfessionViewModel
            {
                Title = "Test Title",
                Content = longContent
            };

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Confession>()))
                .ReturnsAsync((Confession c) => c);

            // Act
            var result = await _service.CreateConfessionAsync(model);

            // Assert
            Assert.Equal(2000, result.Content.Length);
        }

        [Fact]
        public async Task CreateConfessionAsync_WithNestedHtmlTags_SanitizesCorrectly()
        {
            // Arrange
            var model = new CreateConfessionViewModel
            {
                Title = "Test Title",
                Content = "<b><i>Bold and italic</i></b> and <u><s>underlined strike</s></u><script>alert('bad')</script>"
            };

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Confession>()))
                .ReturnsAsync((Confession c) => c);

            // Act
            var result = await _service.CreateConfessionAsync(model);

            // Assert
            Assert.Equal("<b><i>Bold and italic</i></b> and <u><s>underlined strike</s></u>", result.Content);
        }

        [Fact]
        public async Task CreateConfessionAsync_WhenRepositoryFails_ThrowsApplicationException()
        {
            // Arrange
            var model = new CreateConfessionViewModel
            {
                Title = "Test Title",
                Content = "Test Content"
            };

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Confession>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(
                () => _service.CreateConfessionAsync(model));
            Assert.Contains("Failed to create confession", exception.Message);
        }
    }
}
