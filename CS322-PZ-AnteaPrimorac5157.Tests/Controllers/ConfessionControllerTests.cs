using CS322_PZ_AnteaPrimorac5157.Controllers;
using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.Services;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using CS322_PZ_AnteaPrimorac5157.Extensions;
using CS322_PZ_AnteaPrimorac5157.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CS322_PZ_AnteaPrimorac5157.Tests.Controllers
{
    public class ConfessionControllerTests
    {
        private readonly Mock<IConfessionService> _serviceMock;
        private readonly Mock<ILogger<ConfessionController>> _loggerMock;
        private readonly ConfessionController _controller;

        public ConfessionControllerTests()
        {
            _serviceMock = new Mock<IConfessionService>();
            _loggerMock = new Mock<ILogger<ConfessionController>>();
            _controller = new ConfessionController(_serviceMock.Object, _loggerMock.Object);
        }

        // Podesi autoriziranog korisnika
        private void SetupAuthorizedUser()
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "test@test.com") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        [Fact]
        public async Task Index_WithNoConfessions_ReturnsViewWithEmptyList()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetConfessionsAsync(false))
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

            _serviceMock.Setup(s => s.GetConfessionsAsync(false))
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
        public async Task Index_WithMultipleConfessions_DefaultSortsByDate()
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

            _serviceMock.Setup(s => s.GetConfessionsAsync(false))
                                  .ReturnsAsync(confessions.OrderByDescending(c => c.DateCreated));

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
        public async Task Index_WithMultipleConfessions_CanSortByLikes()
        {
            // Arrange
            var confessions = new List<Confession>
            {
                new Confession
                {
                    Title = "Less Popular",
                    Content = "Content",
                    DateCreated = DateTime.UtcNow,
                    Likes = 5,
                    Comments = new List<Comment>()
                },
                new Confession
                {
                    Title = "More Popular",
                    Content = "Content",
                    DateCreated = DateTime.UtcNow.AddDays(-1),
                    Likes = 10,
                    Comments = new List<Comment>()
                }
            };

            _serviceMock.Setup(s => s.GetConfessionsAsync(true))
                       .ReturnsAsync(confessions.OrderByDescending(c => c.Likes));

            // Act
            var result = (await _controller.Index(sortByLikes: true)) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ConfessionListViewModel>>(result.Model);
            var confessionsList = model.ToList();
            Assert.Equal(2, confessionsList.Count);
            Assert.Equal("More Popular", confessionsList[0].Title);
            Assert.Equal("Less Popular", confessionsList[1].Title);

            var currentSort = result.ViewData["CurrentSort"];
            Assert.NotNull(currentSort);
            Assert.True(currentSort is bool);
            Assert.True((bool)currentSort);
        }

        [Fact]
        public async Task Index_WhenSortingByLikes_StoresCurrentSortInViewData()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetConfessionsAsync(true))
                       .ReturnsAsync(new List<Confession>());

            // Act
            var result = (await _controller.Index(sortByLikes: true)) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var currentSort = result.ViewData["CurrentSort"];
            Assert.NotNull(currentSort);
            Assert.True(currentSort is bool);
            Assert.True((bool) currentSort);
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

            _serviceMock.Setup(s => s.GetConfessionsAsync(false))
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
            _serviceMock.Setup(s => s.GetConfessionsAsync(false))
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

        [Fact]
        public async Task Details_LoadsConfessionWithComments_AndUserLikeState()
        {
            // Arrange
            var confessionId = 1;
            var confession = new Confession
            {
                Id = confessionId,
                Comments = new List<Comment>
        {
            new Comment { Content = "Test Comment" }
        }
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockSession();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            _serviceMock.Setup(s => s.GetConfessionAsync(confessionId, true))
                .ReturnsAsync(confession);

            // Act
            var result = await _controller.Details(confessionId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<ConfessionDetailsViewModel>(result.Model);
            Assert.Single(model.Comments);
            Assert.False(model.UserHasLiked);
        }

        [Fact]
        public async Task Details_LoadsCorrectLikeStateFromSession()
        {
            // Arrange
            var confessionId = 1;
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockSession();
            httpContext.Session.SetConfessionLiked(confessionId, true);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            var confession = new Confession { Id = confessionId };
            _serviceMock.Setup(s => s.GetConfessionAsync(confessionId, true))
                .ReturnsAsync(confession);

            // Act
            var result = await _controller.Details(confessionId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<ConfessionDetailsViewModel>(result.Model);
            Assert.True(model.UserHasLiked);
        }

        [Fact]
        public async Task ToggleLike_WhenNotLiked_IncrementsAndSetsSession()
        {
            // Arrange
            var confessionId = 1;
            var confession = new Confession { Id = confessionId, Likes = 0 };

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockSession();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            _serviceMock.Setup(s => s.GetConfessionAsync(confessionId, false))
                .ReturnsAsync(confession);
            _serviceMock.Setup(s => s.IncrementLikesAsync(confessionId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ToggleLike(confessionId);

            // Assert
            _serviceMock.Verify(s => s.IncrementLikesAsync(confessionId), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ConfessionController.Details), redirectResult.ActionName);
        }

        [Fact]
        public async Task ToggleLike_WhenAlreadyLiked_DecrementsAndClearsSession()
        {
            // Arrange
            var confessionId = 1;
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockSession();
            httpContext.Session.SetConfessionLiked(confessionId, true);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.ToggleLike(confessionId);

            // Assert  
            _serviceMock.Verify(s => s.DecrementLikesAsync(confessionId), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ConfessionController.Details), redirectResult.ActionName);
        }

        [Fact]
        public void Create_GET_ReturnsViewResult()
        {
            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_POST_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            var model = new CreateConfessionViewModel
            {
                Title = "Test Title",
                Content = "Test Content"
            };

            _serviceMock.Setup(s => s.CreateConfessionAsync(It.IsAny<CreateConfessionViewModel>()))
                .ReturnsAsync(new Confession()
            );

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            // Provjera da iz ConfessionControllera preusmjerava na Index rutu
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Null(redirectResult.ControllerName);
        }

        [Fact]
        public async Task Create_POST_WithValidationException_ReturnsView_WithError()
        {
            // Arrange
            var model = new CreateConfessionViewModel
            {
                Title = "Test Title",
                Content = "Test Content"
            };

            _serviceMock.Setup(s => s.CreateConfessionAsync(It.IsAny<CreateConfessionViewModel>()))
                .ThrowsAsync(new ValidationException("Test validation error")
            );

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState.Values,
                v => v.Errors.Any(e => e.ErrorMessage == "Test validation error")
            );
        }

        [Fact]
        public async Task Create_POST_WithUnexpectedException_LogsErrorAndReturnsView_WithGenericError()
        {
            // Arrange
            var model = new CreateConfessionViewModel
            {
                Title = "Test Title",
                Content = "Test Content"
            };

            _serviceMock.Setup(s => s.CreateConfessionAsync(It.IsAny<CreateConfessionViewModel>()))
                .ThrowsAsync(new Exception("Unexpected error")
            );

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState.Values,
                v => v.Errors.Any(e => e.ErrorMessage == "An unexpected error occurred. Please try again.")
            );

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), 
                Times.Once);
        }

        [Fact]
        public async Task Create_POST_WithInvalidModelState_ReturnsView_WithoutCallingService()
        {
            // Arrange
            var model = new CreateConfessionViewModel();
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);

            // Provjeri da NIJE pozvan ConfessionService
            _serviceMock.Verify(s => s.CreateConfessionAsync(It.IsAny<CreateConfessionViewModel>()), Times.Never);
        }

        [Fact]
        public async Task Edit_GET_WithValidId_ReturnsView()
        {
            // Arrange
            var confessionId = 1;
            var confession = new Confession { Id = confessionId, Title = "Test", Content = "Content" };
            _serviceMock.Setup(s => s.GetConfessionAsync(confessionId, false))
                .ReturnsAsync(confession);

            // Act
            var result = await _controller.Edit(confessionId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<EditConfessionViewModel>(result.Model);
            Assert.Equal(confession.Title, model.Title);
        }

        [Fact]
        public async Task Edit_GET_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            const int nonExistentId = 9999;
            _serviceMock.Setup(s => s.GetConfessionAsync(nonExistentId, false))
                .ReturnsAsync((Confession?)null);

            // Act
            var result = await _controller.Edit(nonExistentId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_POST_WithConcurrencyConflict_ReturnsViewWithError()
        {
            // Arrange
            var model = new EditConfessionViewModel { Id = 1 };
            _serviceMock.Setup(s => s.UpdateConfessionAsync(model))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            // Act
            var result = await _controller.Edit(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Contains(
                result.ViewData.ModelState.Values,
                v => v.Errors.Any(e => e.ErrorMessage.Contains("Another admin has modified"))
            );
        }

        [Fact]
        public async Task Delete_WithValidId_RedirectsToIndex()
        {
            // Arrange
            var confessionId = 1;
            var confession = new Confession { Id = confessionId };

            _serviceMock.Setup(s => s.GetConfessionAsync(confessionId, false))
                .ReturnsAsync(confession);
            _serviceMock.Setup(s => s.DeleteConfessionAsync(confessionId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(confessionId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ConfessionController.Index), redirectResult.ActionName);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Delete_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var confessionId = 9999;
            _serviceMock.Setup(s => s.GetConfessionAsync(confessionId, false))
                .ReturnsAsync((Confession?) null);

            // Act
            var result = await _controller.Delete(confessionId);

            // Assert
            Assert.IsType<NotFoundResult>(result);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Delete_WhenExceptionOccurs_Returns500Error()
        {
            // Arrange
            var confessionId = 1;
            _serviceMock.Setup(s => s.GetConfessionAsync(confessionId, false))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.Delete(confessionId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("An error occurred while deleting the confession.", objectResult.Value);

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
        public async Task AddComment_WithValidData_AddsCommentAndRedirects()
        {
            // Arrange
            var confessionId = 1;
            var model = new CreateCommentViewModel
            {
                Content = "Test Comment",
                AuthorNickname = "Tester"
            };

            _serviceMock.Setup(s => s.AddCommentAsync(confessionId, model))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddComment(confessionId, model);

            // Assert
            _serviceMock.Verify(s => s.AddCommentAsync(confessionId, model), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(Details), redirectResult.ActionName);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.True(redirectResult.RouteValues.ContainsKey("id"));
            Assert.Equal(confessionId, (int)redirectResult.RouteValues["id"]!);
        }

        // TODO: robusniji test
        [Fact]
        public async Task AddComment_WithInvalidModel_ReturnsToDetailsWithError()
        {
            // Arrange
            var confessionId = 1;
            var model = new CreateCommentViewModel { ConfessionId = confessionId };
            _controller.ModelState.AddModelError("Content", "Content is required");
            _controller.ModelState.AddModelError("AuthorNickname", "Nickname is required");


            // Act
            var result = await _controller.AddComment(confessionId, model);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFoundResult);
            _serviceMock.Verify(s => s.AddCommentAsync(It.IsAny<int>(), It.IsAny<CreateCommentViewModel>()), Times.Never);
        }

        [Fact]
        public async Task AddComment_WhenExceptionOccurs_LogsAndRedirectsToDetails()
        {
            // Arrange
            var confessionId = 1;
            var model = new CreateCommentViewModel
            {
                Content = "Test",
                AuthorNickname = "Tester"
            };
            var exception = new Exception("Test error");

            _serviceMock.Setup(s => s.AddCommentAsync(confessionId, model))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.AddComment(confessionId, model);

            // Assert
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(Details), redirectResult.ActionName);
        }

        // TODO
        [Fact]
        public async Task EditComment_Success_ReturnsJsonResult()
        {
            // Arrange
            var model = new EditCommentViewModel
            {
                Id = 1,
                ConfessionId = 1,
                Content = "Updated content",
                AuthorNickname = "Updated author"
            };

            _serviceMock.Setup(s => s.UpdateCommentAsync(It.IsAny<EditCommentViewModel>()))
                .Returns(Task.CompletedTask);

            SetupAuthorizedUser();

            // Act
            var result = await _controller.EditComment(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);

            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            Assert.True(deserialized["success"].GetBoolean());
            Assert.Equal(model.Content, deserialized["content"].GetString());
            Assert.Equal(model.AuthorNickname, deserialized["authorNickname"].GetString());
        }

        // TODO: popraviti
        [Fact]
        public async Task EditComment_WithInvalidModel_ReturnsErrorJson()
        {
            // Arrange
            var model = new EditCommentViewModel
            {
                Id = 1,
                ConfessionId = 1
            };

            SetupAuthorizedUser();

            // Eksplicitna validacija modela
            var validationContext = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

            // Dodavanje greški validacije u ModelState
            foreach (var validationResult in validationResults)
            {
                if (validationResult.ErrorMessage != null && validationResult.MemberNames.Any())
                {
                    _controller.ModelState.AddModelError(
                        validationResult.MemberNames.First(),
                        validationResult.ErrorMessage);
                }
            }

            // Act
            var result = await _controller.EditComment(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);

            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            Assert.NotNull(deserialized);
            if (deserialized.TryGetValue("success", out JsonElement successElement))
            {
                Assert.False(successElement.GetBoolean());
            }
            else
            {
                Assert.Fail("success property not found in response");
            }

            if (deserialized.TryGetValue("message", out JsonElement messageElement))
            {
                Assert.Equal("Invalid data", messageElement.GetString());
            }
            else
            {
                Assert.Fail("message property not found in response");
            }
        }

        [Fact]
        public async Task EditComment_WhenServiceThrows_ReturnsErrorJson()
        {
            // Arrange
            var model = new EditCommentViewModel
            {
                Id = 1,
                Content = "Test",
                AuthorNickname = "Test"
            };
            var exception = new Exception("Test error");

            _serviceMock.Setup(s => s.UpdateCommentAsync(model))
                .ThrowsAsync(exception);

            SetupAuthorizedUser();

            // Act
            var result = await _controller.EditComment(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);

            var json = JsonSerializer.Serialize(jsonResult.Value);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            Assert.False(deserialized["success"].GetBoolean());
            Assert.Equal("An error occurred while updating the comment", deserialized["message"].GetString());

            _loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
        }

        [Fact]
        public async Task EditComment_UnauthorizedUser_ReturnsForbidden()
        {
            // Arrange
            var model = new EditCommentViewModel();

            // Podesi neautoriziranog korisnika (tj. gosta u ovoj aplikaciji)
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var result = await _controller.EditComment(model);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeleteComment_WhenUserAuthenticated_DeletesAndRedirects()
        {
            // Arrange
            var confessionId = 1;
            var commentId = 1;

            _serviceMock.Setup(s => s.DeleteCommentAsync(confessionId, commentId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteComment(confessionId, commentId);

            // Assert
            _serviceMock.Verify(s => s.DeleteCommentAsync(confessionId, commentId), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ConfessionController.Details), redirectResult.ActionName);
            Assert.Equal(confessionId, redirectResult.RouteValues["id"]);
        }

        [Fact]
        public async Task DeleteComment_WhenServiceThrows_LogsAndRedirects()
        {
            // Arrange
            var confessionId = 1;
            var commentId = 1;
            var exception = new Exception("Test exception");

            _serviceMock.Setup(s => s.DeleteCommentAsync(confessionId, commentId))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.DeleteComment(confessionId, commentId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ConfessionController.Details), redirectResult.ActionName);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    exception,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
