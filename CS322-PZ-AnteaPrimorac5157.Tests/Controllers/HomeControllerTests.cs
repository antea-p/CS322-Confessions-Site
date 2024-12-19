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
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_loggerMock.Object);
        }

        [Fact]
        public void Index_RedirectsToConfessionIndex()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Confession", redirectResult.ControllerName);
        }
    }
}
