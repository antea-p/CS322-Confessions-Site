using CS322_PZ_AnteaPrimorac5157.Controllers;
using CS322_PZ_AnteaPrimorac5157.Data;
using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace CS322_PZ_AnteaPrimorac5157.Tests.Controllers
{
    public class HomeControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_loggerMock.Object, _context);
        }

        [Fact]
        public void Index_WithNoConfessions_ReturnsViewWithEmptyList()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ConfessionListViewModel>>(result.Model);
            Assert.Empty(model);
            Assert.Equal("Confessions list is empty!", result.ViewData["Message"]);
        }

        [Fact]
        public void Index_WithSingleConfession_ReturnsViewWithOneConfession()
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
            _context.Confessions.Add(confession);
            _context.SaveChanges();

            // Act
            var result = _controller.Index() as ViewResult;

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
        public void Index_WithMultipleConfessions_ReturnsAllConfessionsOrderedByDate()
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

            _context.Confessions.AddRange(confessions);
            _context.SaveChanges();

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ConfessionListViewModel>>(result.Model);
            var confessionsList = model.ToList();
            Assert.Equal(2, confessionsList.Count);
            Assert.Equal("New Confession", confessionsList[0].Title); // ispovijest sa tekućim datumom
            Assert.Equal("Old Confession", confessionsList[1].Title); // ispovijest sa jučerašnjim datumom
        }

        [Fact]
        public void Index_WithConfessionWithComments_ReturnsCorrectCommentCount()
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

            _context.Confessions.Add(confession);
            _context.SaveChanges();

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ConfessionListViewModel>>(result.Model);
            var confessionViewModel = Assert.Single(model);
            Assert.Equal(2, confessionViewModel.CommentCount);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}