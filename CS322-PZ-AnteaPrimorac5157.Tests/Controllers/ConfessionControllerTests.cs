using CS322_PZ_AnteaPrimorac5157.Controllers;
using CS322_PZ_AnteaPrimorac5157.Models;
using CS322_PZ_AnteaPrimorac5157.Services;
using CS322_PZ_AnteaPrimorac5157.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
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
    }
}
