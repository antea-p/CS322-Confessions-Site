﻿using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.Repositories;
using CS322_PZ_AnteaPrimorac5157.Services;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
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

        [Fact]
        public async Task GetConfessionAsync_WithValidId_ReturnsConfession()
        {
            // Arrange
            var expectedConfession = new Confession
            {
                Id = 1,
                Title = "Test Title",
                Content = "Test Content"
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<bool>()))
                .ReturnsAsync(expectedConfession);

            // Act
            var result = await _service.GetConfessionAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedConfession.Id, result.Id);
            Assert.Equal(expectedConfession.Title, result.Title);
            Assert.Equal(expectedConfession.Content, result.Content);
        }

        [Fact]
        public async Task GetConfessionAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(9999, false))
                .ReturnsAsync((Confession?)null);

            // Act
            var result = await _service.GetConfessionAsync(9999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetConfessionsAsync_WithSortByLikes_ReturnsConfessionsOrderedByLikesDesc()
        {
            // Arrange
            var confessions = new List<Confession>
            {
                new Confession { Title = "Less Popular", Likes = 5, DateCreated = DateTime.UtcNow },
                new Confession { Title = "Most Popular", Likes = 10, DateCreated = DateTime.UtcNow.AddDays(-1) }
            };

            _repositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(confessions);

            // Act
            var result = (await _service.GetConfessionsAsync(orderByLikes: true)).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Most Popular", result[0].Title);
            Assert.Equal("Less Popular", result[1].Title);
        }

        [Fact]
        public async Task GetConfessionsAsync_WithDefaultSort_ReturnsConfessionsOrderedByDateDesc()
        {
            // Arrange
            var confessions = new List<Confession>
            {
                new Confession { Title = "Old Post", Likes = 10, DateCreated = DateTime.UtcNow.AddDays(-1) },
                new Confession { Title = "New Post", Likes = 5, DateCreated = DateTime.UtcNow }
            };

            _repositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(confessions);

            // Act
            var result = (await _service.GetConfessionsAsync(orderByLikes: false)).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("New Post", result[0].Title);
            Assert.Equal("Old Post", result[1].Title);
        }

        [Fact]
        public async Task GetConfessionsAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Confession>());

            // Act
            var resultByLikes = await _service.GetConfessionsAsync(orderByLikes: true);
            var resultByDate = await _service.GetConfessionsAsync(orderByLikes: false);

            // Assert
            Assert.Empty(resultByLikes);
            Assert.Empty(resultByDate);
        }

        [Fact]
        public async Task UpdateConfessionAsync_WithConcurrentUpdate_ThrowsConcurrencyException()
        {
            // Arrange
            var model = new EditConfessionViewModel { 
                Id = 1,
                Title = "Updated Test Title",
                Content = "Updated Test Content"

            };
            var confession = new Confession
            {
                Id = 1,
                Title = "Test Title",
                Content = "Test Content"
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1, false))
                .ReturnsAsync(confession);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Confession>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                () => _service.UpdateConfessionAsync(model)
            );
        }

        [Theory]
        [InlineData("<script>alert('xss')</script>Test", "Test")]
        [InlineData("<b>Bold</b>", "<b>Bold</b>")]
        public async Task UpdateConfessionAsync_SanitizesContent(
           string input, string expected)
        {
            // Arrange
            var model = new EditConfessionViewModel
            {
                Id = 1,
                Title = "Test Title",
                Content = input
            };

            var confession = new Confession { Id = 1 };
            _repositoryMock.Setup(r => r.GetByIdAsync(1, false))
                .ReturnsAsync(confession);

            // Act
            await _service.UpdateConfessionAsync(model);

            // Assert
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Confession>(c =>
                c.Content == expected)));
        }

        [Theory]
        [InlineData("", "Valid Content")]
        [InlineData("   ", "Valid Content")]
        [InlineData("<b></b>", "Valid Content")]
        [InlineData("Valid Title", "")]
        [InlineData("Valid Title", "   ")]
        [InlineData("Valid Title", "<i></i>")]
        public async Task UpdateConfessionAsync_WithEmptyContent_ThrowsValidationException(
            string title, string content)
        {
            // Arrange
            var model = new EditConfessionViewModel
            {
                Id = 1,
                Title = title,
                Content = content
            };

            var confession = new Confession { Id = 1 };
            _repositoryMock.Setup(r => r.GetByIdAsync(1, false))
                .ReturnsAsync(confession);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _service.UpdateConfessionAsync(model));
            Assert.Contains("cannot be empty", exception.Message);
        }

        [Fact]
        public async Task UpdateConfessionAsync_WithLongContent_PreservesContent()
        {
            // Arrange
            var longContent = new string('a', 2000);
            var model = new EditConfessionViewModel
            {
                Id = 1,
                Title = "Test Title",
                Content = longContent
            };

            var confession = new Confession { Id = 1 };
            _repositoryMock.Setup(r => r.GetByIdAsync(1, false))
                .ReturnsAsync(confession);

            // Act
            await _service.UpdateConfessionAsync(model);

            // Assert
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Confession>(c =>
                c.Content.Length == 2000)));
        }

        [Fact]
        public async Task UpdateConfessionAsync_WithNestedHtmlTags_SanitizesCorrectly()
        {
            // Arrange
            var model = new EditConfessionViewModel
            {
                Id = 1,
                Title = "Test Title",
                Content = "<b><i>Bold and italic</i></b><script>alert('bad')</script>"
            };

            var confession = new Confession { Id = 1 };
            _repositoryMock.Setup(r => r.GetByIdAsync(1, false))
                .ReturnsAsync(confession);

            // Act
            await _service.UpdateConfessionAsync(model);

            // Assert
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Confession>(c =>
                c.Content == "<b><i>Bold and italic</i></b>")));
        }

        [Fact]
        public async Task DeleteConfessionAsync_WithValidId_DeletesConfession()
        {
            // Arrange
            var confession = new Confession
            {
                Id = 1,
                Title = "Test Title",
                Content = "Test Content"
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1, false))
                .ReturnsAsync(confession);
            _repositoryMock.Setup(r => r.DeleteAsync(confession))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteConfessionAsync(1);

            // Assert
            _repositoryMock.Verify(r => r.DeleteAsync(confession), Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteConfessionAsync_WithNonExistentId_ThrowsInvalidOperationException()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(9999, false))
                .ReturnsAsync((Confession?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.DeleteConfessionAsync(999));

            Assert.Equal("Confession with ID 999 not found", exception.Message);
        }

        [Fact]
        public async Task DeleteConfessionAsync_WhenRepositoryFails_LogsAndRethrowsException()
        {
            // Arrange
            var confession = new Confession { Id = 1 };
            var expectedError = new Exception("Database error");

            _repositoryMock.Setup(r => r.GetByIdAsync(1, false))
                .ReturnsAsync(confession);
            _repositoryMock.Setup(r => r.DeleteAsync(confession))
                .ThrowsAsync(expectedError);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.DeleteConfessionAsync(1));

            Assert.Same(expectedError, exception);
 
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AddCommentAsync_SanitizesInput()
        {
            // Arrange
            var confessionId = 1;
            var model = new CreateCommentViewModel
            {
                Content = "<b>Valid</b><script>alert(1)</script>",
                AuthorNickname = "<i>Tester</i><img src=x onerror=alert(1)>"
            };

            _repositoryMock.Setup(r => r.AddCommentAsync(It.IsAny<Comment>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddCommentAsync(confessionId, model);

            // Assert
            _repositoryMock.Verify(r => r.AddCommentAsync(It.Is<Comment>(c =>
                c.Content == "<b>Valid</b>" &&
                c.AuthorNickname == "<i>Tester</i>"
            )), Times.Once);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", "nickname")]
        [InlineData("content", "")]
        [InlineData("<script>alert(1)</script>", "nickname")]
        [InlineData("content", "<script>alert(1)</script>")]
        public async Task AddCommentAsync_WithInvalidInput_ThrowsValidation(string content, string nickname)
        {
            // Arrange
            var model = new CreateCommentViewModel
            {
                Content = content,
                AuthorNickname = nickname
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.AddCommentAsync(1, model));
        }

        [Fact]
        public async Task UpdateCommentAsync_WithValidInput_UpdatesComment()
        {
            // Arrange
            var model = new EditCommentViewModel
            {
                Id = 1,
                ConfessionId = 1,
                Content = "Updated content",
                AuthorNickname = "Updated author"
            };

            var existingComment = new Comment
            {
                Id = 1,
                Content = "Old content",
                AuthorNickname = "Old author"
            };

            _repositoryMock.Setup(r => r.GetCommentAsync(1))
                .ReturnsAsync(existingComment);

            // Act
            await _service.UpdateCommentAsync(model);

            // Assert
            _repositoryMock.Verify(r => r.UpdateCommentAsync(It.Is<Comment>(c =>
                c.Content == model.Content &&
                c.AuthorNickname == model.AuthorNickname)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCommentAsync_WithNonexistentComment_ThrowsInvalidOperationException()
        {
            // Arrange
            var model = new EditCommentViewModel { Id = 9999 };

            _repositoryMock.Setup(r => r.GetCommentAsync(9999))
                .ReturnsAsync((Comment?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateCommentAsync(model));
        }

        [Theory]
        [InlineData("<script>alert('xss')</script>Test", "Test")]
        [InlineData("<b>Bold</b>", "<b>Bold</b>")]
        public async Task UpdateCommentAsync_SanitizesContent(string input, string expected)
        {
            // Arrange
            var model = new EditCommentViewModel
            {
                Id = 1,
                Content = input,
                AuthorNickname = "Author"
            };

            var existingComment = new Comment { Id = 1 };

            _repositoryMock.Setup(r => r.GetCommentAsync(1))
                .ReturnsAsync(existingComment);

            // Act
            await _service.UpdateCommentAsync(model);

            // Assert
            _repositoryMock.Verify(r => r.UpdateCommentAsync(It.Is<Comment>(c =>
                c.Content == expected)), Times.Once);
        }

        [Fact]
        public async Task DeleteCommentAsync_CallsRepository()
        {
            // Arrange
            var confessionId = 1;
            var commentId = 1;

            _repositoryMock.Setup(r => r.DeleteCommentAsync(confessionId, commentId))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteCommentAsync(confessionId, commentId);

            // Assert
            _repositoryMock.Verify(r => r.DeleteCommentAsync(confessionId, commentId), Times.Once);
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenRepositoryThrows_LogsAndRethrows()
        {
            // Arrange
            var confessionId = 1;
            var commentId = 1;
            var exception = new Exception("Test exception");

            _repositoryMock.Setup(r => r.DeleteCommentAsync(confessionId, commentId))
                .ThrowsAsync(exception);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.DeleteCommentAsync(confessionId, commentId));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
