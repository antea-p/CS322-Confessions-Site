using CS322_PZ_AnteaPrimorac5157.Data;
using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.Repositories;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CS322_PZ_AnteaPrimorac5157.Tests.Repositories
{
    public class ConfessionRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _contextOptions;
        private readonly Mock<ILogger<ConfessionRepository>> _loggerMock;

        public ConfessionRepositoryTests()
        {
            _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestConfessionDb")
                .Options;

            _loggerMock = new Mock<ILogger<ConfessionRepository>>();

            using var context = new ApplicationDbContext(_contextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllConfessions_OrderedByDateDesc()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confession1 = new Confession
            {
                Title = "First Confession",
                Content = "Content 1",
                DateCreated = DateTime.UtcNow.AddDays(-1)
            };
            var confession2 = new Confession
            {
                Title = "Second Confession",
                Content = "Content 2",
                DateCreated = DateTime.UtcNow
            };

            await context.Confessions.AddRangeAsync(confession1, confession2);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            var confessions = result.ToList();
            Assert.Equal(2, confessions.Count);
            Assert.Equal(confession2.Title, confessions[0].Title);
            Assert.Equal(confession1.Title, confessions[1].Title);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsConfession()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confession = new Confession
            {
                Title = "Test Confession",
                Content = "Test Content"
            };
            await context.Confessions.AddAsync(confession);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(confession.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(confession.Title, result.Title);
        }

        [Fact]
        public async Task GetWithCommentsAsync_IncludesComments()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confession = new Confession
            {
                Title = "Test Confession",
                Content = "Test Content"
            };
            var comment = new Comment
            {
                Content = "Test Comment",
                AuthorNickname = "Tester",
                Confession = confession
            };

            confession.Comments.Add(comment);
            await context.Confessions.AddAsync(confession);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetWithCommentsAsync(confession.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Comments);
            Assert.Equal(comment.Content, result.Comments.First().Content);
        }

        [Fact]
        public async Task AddAsync_AddsConfessionToDatabase()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confession = new Confession
            {
                Title = "New Confession",
                Content = "New Content"
            };

            // Act
            var result = await repository.AddAsync(confession);

            // Assert
            // Provjeri da je dodijeljen ID (novi entiteti imaju ID 0 prije nego budu sačuvani)
            Assert.NotEqual(0, result.Id); 
            var savedConfession = await context.Confessions.FindAsync(result.Id);
            Assert.NotNull(savedConfession);
            Assert.Equal(confession.Title, savedConfession.Title);
        }

        [Fact]
        public async Task DeleteAsync_RemovesConfessionAndComments()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confession = new Confession
            {
                Title = "Test Confession",
                Content = "Test Content"
            };
            var comment = new Comment
            {
                Content = "Test Comment",
                AuthorNickname = "Tester",
                Confession = confession
            };

            confession.Comments.Add(comment);
            await context.Confessions.AddAsync(confession);
            await context.SaveChangesAsync();

            // Act
            await repository.DeleteAsync(confession);

            // Assert
            Assert.Null(await context.Confessions.FindAsync(confession.Id));
            Assert.Empty(await context.Comments.Where(c => c.ConfessionId == confession.Id).ToListAsync());
        }

        [Fact]
        public async Task IncrementLikesAsync_IncreasesLikeCount()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confession = new Confession
            {
                Title = "Test Confession",
                Content = "Test Content",
                Likes = 0
            };
            await context.Confessions.AddAsync(confession);
            await context.SaveChangesAsync();

            // Act
            await repository.IncrementLikesAsync(confession.Id);

            // Assert
            var updatedConfession = await context.Confessions.FindAsync(confession.Id);
            Assert.NotNull(updatedConfession);
            Assert.Equal(1, updatedConfession.Likes);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            // Act
            var result = await repository.GetByIdAsync(9999);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v != null && v.ToString()!.Contains("Error occurred while getting confession with ID: 9999")),
                    It.IsAny<Exception?>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()
                ),
                Times.Never);
        }

        [Fact]
        public async Task DecrementLikesAsync_WithZeroLikes_DoesNotDecrementBelowZero()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confession = new Confession
            {
                Title = "Test Confession",
                Content = "Test Content",
                Likes = 0
            };
            await context.Confessions.AddAsync(confession);
            await context.SaveChangesAsync();

            // Act
            await repository.DecrementLikesAsync(confession.Id);

            // Assert
            var updatedConfession = await context.Confessions.FindAsync(confession.Id);
            Assert.NotNull(updatedConfession);
            Assert.Equal(0, updatedConfession.Likes);
        }

        [Theory]
        [InlineData("<b>Bold text</b>", "<b>Bold text</b>")]
        [InlineData("<i>Italic text</i>", "<i>Italic text</i>")]
        [InlineData("<p>Paragraph</p>", "<p>Paragraph</p>")]
        [InlineData("<br>Line break", "<br>Line break")]
        public async Task AddAsync_WithAllowedHtmlContent_StoresContentCorrectly(string content, string expectedContent)
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confession = new Confession
            {
                Title = "HTML Test",
                Content = content
            };

            // Act
            var result = await repository.AddAsync(confession);

            // Assert
            var savedConfession = await context.Confessions.FindAsync(result.Id);
            Assert.NotNull(savedConfession);
            Assert.Equal(expectedContent, savedConfession.Content);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetWithCommentsAsync_WithInvalidId_ReturnsNull(string invalidContent)
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confession = new Confession
            {
                Title = "Valid Title",
                Content = invalidContent ?? "Valid Content"
            };
            await context.Confessions.AddAsync(confession);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetWithCommentsAsync(9999);

            // Assert
            Assert.Null(result);
        }
    }
}