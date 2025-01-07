using CS322_PZ_AnteaPrimorac5157.Data;
using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.Repositories;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Transactions;
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
        public async Task GetAllAsync_WhenSortedByLikes_ReturnsConfessionsOrderedByLikesDesc()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confession1 = new Confession
            {
                Title = "Less Liked",
                Content = "Content 1",
                DateCreated = DateTime.UtcNow,
                Likes = 5
            };
            var confession2 = new Confession
            {
                Title = "Most Liked",
                Content = "Content 2",
                DateCreated = DateTime.UtcNow.AddDays(-1),
                Likes = 10
            };

            await context.Confessions.AddRangeAsync(confession1, confession2);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetTopConfessionsAsync(10);

            // Assert
            var confessions = result.ToList();
            Assert.Equal(2, confessions.Count);
            Assert.Equal("Most Liked", confessions[0].Title);
            Assert.Equal("Less Liked", confessions[1].Title);
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
            var result = await repository.GetByIdAsync(confession.Id, includeComments: true);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Comments);
            Assert.Equal(comment.Content, result.Comments.First().Content);
        }

        [Fact]
        public async Task GetByIdAsync_WithCommentsDisabled_DoesNotIncludeComments()
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

            context.ChangeTracker.Clear();

            // Act
            var result = await repository.GetByIdAsync(confession.Id, includeComments: false);

            // Assert
            // Komentari nisu učitani, ali ostala polja jesu
            Assert.NotNull(result);
            Assert.Empty(result.Comments);
            Assert.Equal("Test Confession", result.Title);
            Assert.Equal("Test Content", result.Content);
        }

        [Fact]
        public async Task SearchConfessionsAsync_WithMatchingTitle_ReturnsConfessions()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confessions = new List<Confession>
            {
                new Confession { Title = "Test Confession", Content = "Some content" },
                new Confession { Title = "Another post", Content = "Different content" }
            };

            await context.Confessions.AddRangeAsync(confessions);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchConfessionsAsync("Test");

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Confession", result.First().Title);
        }

        [Fact]
        public async Task SearchConfessionsAsync_WithMatchingContent_ReturnsConfessions()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confessions = new List<Confession>
            {
                new Confession { Title = "Test Confession", Content = "Some content" },
                new Confession { Title = "Another post", Content = "Different content" }
            };

            await context.Confessions.AddRangeAsync(confessions);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.SearchConfessionsAsync("Different");

            // Assert
            Assert.Single(result);
            Assert.Equal("Different content", result.First().Content);
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
        public async Task DeleteAsync_WithAlreadyDeletedConfession_ThrowsConcurrencyException()
        {
            // Arrange
            using var context1 = new ApplicationDbContext(_contextOptions);
            using var context2 = new ApplicationDbContext(_contextOptions);

            var repository1 = new ConfessionRepository(context1, _loggerMock.Object);
            var repository2 = new ConfessionRepository(context2, _loggerMock.Object);

            var confession = new Confession
            {
                Title = "Test Confession",
                Content = "Test Content"
            };
            await context1.Confessions.AddAsync(confession);
            await context1.SaveChangesAsync();

            var confession1 = await context1.Confessions.FindAsync(confession.Id);
            var confession2 = await context2.Confessions.FindAsync(confession.Id);

            Assert.NotNull(confession1);
            Assert.NotNull(confession2);

            // Brisanje ispovijesti u drugom kontekstu
            await repository2.DeleteAsync(confession2);

            // Act & Assert - Ne bi trebalo biti moguće izbrisati ispovijest u prvom kontekstu (već izbrisana)
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                repository1.DeleteAsync(confession1));
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

            context.ChangeTracker.Clear();

            // Act
            await repository.IncrementLikesAsync(confession.Id);

            // Assert
            var updatedConfession = await context.Confessions.FindAsync(confession.Id);
            Assert.NotNull(updatedConfession);
            Assert.Equal(1, updatedConfession.Likes);
        }

        [Fact]
        public async Task IncrementLikesAsync_WithConcurrentAccess_HandlesRaceCondition()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);

            var confession = new Confession
            {
                Title = "Test Confession",
                Content = "Test Content",
                Likes = 0
            };
            await context.Confessions.AddAsync(confession);
            await context.SaveChangesAsync();

            // Act
            var tasks = Enumerable.Range(0, 5).Select(async _ =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var newContext = new ApplicationDbContext(_contextOptions);
                var newRepo = new ConfessionRepository(newContext, _loggerMock.Object);

                var retryCount = 0;
                while (retryCount < 3)
                {
                    try
                    {
                        await newRepo.IncrementLikesAsync(confession.Id);
                        scope.Complete();
                        break;
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        retryCount++;
                        if (retryCount == 3) throw;
                    }
                }
            });

            await Task.WhenAll(tasks);

            // Assert
            using var verifyContext = new ApplicationDbContext(_contextOptions);
            var updatedConfession = await verifyContext.Confessions.FindAsync(confession.Id);
            Assert.NotNull(updatedConfession);
            Assert.Equal(5, updatedConfession.Likes);
        }

        [Fact]
        public async Task IncrementLikesAsync_WithNonExistentConfession_DoesNothing()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            // Act & Assert
            await repository.IncrementLikesAsync(9999); // Ne bi trebalo izbaciti izuzetak
        }

        [Fact]
        public async Task IncrementLikesAsync_WithTrackedEntity_UpdatesCorrectly()
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

            context.ChangeTracker.Clear();

            // Act
            await repository.IncrementLikesAsync(confession.Id);

            // Assert
            var updatedConfession = await context.Confessions.FindAsync(confession.Id);
            Assert.NotNull(updatedConfession);
            Assert.Equal(1, updatedConfession.Likes);

            // Verify in new context
            using var verifyContext = new ApplicationDbContext(_contextOptions);
            var reloadedConfession = await verifyContext.Confessions.FindAsync(confession.Id);
            Assert.NotNull(reloadedConfession);
            Assert.Equal(1, reloadedConfession.Likes);
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
            var result = await repository.GetByIdAsync(9999, includeComments: true);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCommentAsync_WithValidId_ReturnsComment()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confession = new Confession
            {
                Title = "Test",
                Content = "Test"
            };
            var comment = new Comment
            {
                Content = "Test Comment",
                AuthorNickname = "Tester",
                Confession = confession
            };
            await context.Confessions.AddAsync(confession);
            await context.Comments.AddAsync(comment);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetCommentAsync(comment.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(comment.Content, result.Content);
            Assert.Equal(comment.AuthorNickname, result.AuthorNickname);
        }

        [Fact]
        public async Task AddCommentAsync_UpdatesConfessionComments()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confession = new Confession
            {
                Title = "Test",
                Content = "Test"
            };
            await context.Confessions.AddAsync(confession);
            await context.SaveChangesAsync();

            var comment = new Comment
            {
                Content = "Test Comment",
                AuthorNickname = "Tester",
                ConfessionId = confession.Id
            };

            // Act
            await repository.AddCommentAsync(comment);

            // Assert
            var savedConfession = await context.Confessions
                .Include(c => c.Comments)
                .FirstAsync(c => c.Id == confession.Id);

            Assert.Single(savedConfession.Comments);
            Assert.Equal(comment.Content, savedConfession.Comments.First().Content);
        }

        [Fact]
        public async Task AddCommentAsync_WithInvalidConfessionId_Throws()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var comment = new Comment
            {
                Content = "Test",
                AuthorNickname = "Tester",
                ConfessionId = 9999
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() =>
                repository.AddCommentAsync(comment));
        }


        [Fact]
        public async Task UpdateCommentAsync_WithValidComment_UpdatesDatabase()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            var confession = new Confession { Title = "Test", Content = "Test" };
            var comment = new Comment
            {
                Content = "Original",
                AuthorNickname = "Original",
                Confession = confession
            };
            await context.Confessions.AddAsync(confession);
            await context.Comments.AddAsync(comment);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();

            comment.Content = "Updated";
            comment.AuthorNickname = "Updated";

            // Act
            await repository.UpdateCommentAsync(comment);

            // Assert
            var updatedComment = await context.Comments.FindAsync(comment.Id);
            Assert.NotNull(updatedComment);
            Assert.Equal("Updated", updatedComment.Content);
            Assert.Equal("Updated", updatedComment.AuthorNickname);
        }

        [Fact]
        public async Task DeleteCommentAsync_WithValidIds_DeletesComment()
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

            context.ChangeTracker.Clear();

            // Act
            await repository.DeleteCommentAsync(confession.Id, comment.Id);

            // Assert
            var deletedComment = await context.Comments.FindAsync(comment.Id);
            Assert.Null(deletedComment);
        }

        [Fact]
        public async Task DeleteCommentAsync_WithInvalidIds_DoesNothing()
        {
            // Arrange
            using var context = new ApplicationDbContext(_contextOptions);
            var repository = new ConfessionRepository(context, _loggerMock.Object);

            // Act & Assert
            await repository.DeleteCommentAsync(9999, 9999); // Ne smije izbaciti izuzetak
        }
    }
}