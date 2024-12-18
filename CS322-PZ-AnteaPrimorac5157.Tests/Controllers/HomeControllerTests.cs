using CS322_PZ_AnteaPrimorac5157.Controllers;
using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.Repositories;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CS322_PZ_AnteaPrimorac5157.Tests.Repositories
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private readonly Mock<IConfessionRepository> _repositoryMock;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _repositoryMock = new Mock<IConfessionRepository>();

            _controller = new HomeController(
                _loggerMock.Object,
                _repositoryMock.Object);
        }

        [Fact]
        public async Task Index_WithNoConfessions_ReturnsViewWithEmptyList()
        {
            // Arrange
            _repositoryMock.Setup(repo => repo.GetAllAsync())
                           .ReturnsAsync(new List<Confession>());

            // Act
            var result = (await _controller.Index()) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ConfessionListViewModel>>(result.Model);
            Assert.Empty(model);
            Assert.Equal("Confessions list is empty!", result.ViewData["Message"]);
        }

        [Fact]
        public async Task Index_WithSingleConfession_ReturnsViewWithOneConfession()
        {
            // Arrange
            var confession = new Confession
            {
                Id = 1,
                Title = "Test Confession",
                Content = "Test Content",
                DateCreated = DateTime.UtcNow,
                Likes = 0,
                Comments = new List<Comment>()
            };

            _repositoryMock.Setup(repo => repo.GetAllAsync())
                           .ReturnsAsync(new List<Confession> { confession });

            // Act
            var result = (await _controller.Index()) as ViewResult;

            // Assert
            Assert.NotNull(result);

            // isAssignableFrom provjerava možemo li ovaj objekt tretirati kao tip T, tj. IEnumerable<ConfessionListViewModel>
            var model = Assert.IsAssignableFrom<IEnumerable<ConfessionListViewModel>>(result.Model);
            var confessionsList = Assert.Single(model); // provjeri da lista ima točno 1 element
            Assert.Equal("Test Confession", confessionsList.Title);
            Assert.Equal("Test Content", confessionsList.Content);
            Assert.Equal(0, confessionsList.CommentCount);
        }

        [Fact]
        public async Task Index_WithMultipleConfessions_ReturnsAllConfessionsOrderedByDate()
        {
            // Arrange
            var oldDate = DateTime.UtcNow.AddDays(-1);
            var newDate = DateTime.UtcNow;

            var confessions = new List<Confession>
        {
            new Confession
            {
                Title = "Old Confession",
                Content = "Old Content",
                DateCreated = oldDate,
                Comments = new List<Comment>()
            },
            new Confession
            {
                Title = "New Confession",
                Content = "New Content",
                DateCreated = newDate,
                Comments = new List<Comment>()
            }
        };

            _repositoryMock.Setup(repo => repo.GetAllAsync())
                           .ReturnsAsync(confessions);

            // Act
            var result = (await _controller.Index()) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ConfessionListViewModel>>(result.Model);
            var confessionsList = model.ToList();
            Assert.Equal(2, confessionsList.Count);
            Assert.Equal("New Confession", confessionsList[0].Title); // ispovijest sa tekućim datumom
            Assert.Equal("Old Confession", confessionsList[1].Title); // ispovijest sa jučerašnjim datumom
        }

        [Fact]
        public async Task Index_WithConfessionWithComments_ReturnsCorrectCommentCount()
        {
            // Arrange
            var confession = new Confession
            {
                Title = "Test Confession",
                Content = "Test Content",
                DateCreated = DateTime.UtcNow,
                Comments = new List<Comment>
        {
            new Comment { Content = "Comment 1", AuthorNickname = "User1" },
            new Comment { Content = "Comment 2", AuthorNickname = "User2" }
        }
            };

            _repositoryMock.Setup(repo => repo.GetAllAsync())
                           .ReturnsAsync(new List<Confession> { confession });

            // Act
            var result = (await _controller.Index()) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ConfessionListViewModel>>(result.Model);
            var confessionViewModel = Assert.Single(model);
            Assert.Equal(2, confessionViewModel.CommentCount);
        }

        [Fact]
        public async Task Index_WhenRepositoryThrowsException_ReturnsViewWithError()
        {
            // Arrange
            _repositoryMock.Setup(repo => repo.GetAllAsync())
                           .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = (await _controller.Index()) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("An error occurred while loading confessions.", result.ViewData["Message"]);
            var model = Assert.IsAssignableFrom<IEnumerable<ConfessionListViewModel>>(result.Model);
            Assert.Empty(model);

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.Is<Exception>(e => e.Message == "Database error"),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
