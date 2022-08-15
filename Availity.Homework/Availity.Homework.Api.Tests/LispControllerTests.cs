using Availity.Homework.Api.Controllers;
using Availity.Homework.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using Xunit;

namespace Availity.Homework.Api.Tests
{
    public class LispControllerTests
    {
        readonly LispController controller;
        readonly Mock<ILispService> mockLispService;

        public LispControllerTests()
        {
            mockLispService = new Mock<ILispService>();
            controller = new LispController(mockLispService.Object);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ValidateCode_ShouldReturnAOkObjectResultWithTheCorrectValue_WhenTheCodePassedInIsNotNullOrEmpty(bool isValid)
        {
            // Arrange
            var code = "((c)) b)";
            mockLispService.Setup(x => x.Valid(code)).Returns(isValid).Verifiable();

            // Act
            var response = controller.ValidateCode(code);

            // Assert
            mockLispService.Verify();
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(isValid, (response as OkObjectResult).Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ValidateCode_ShouldReturnBadRequestResult_WhenTheCodePassedInIsNullOrEmpty(string lispCode)
        {
            // Arrange
            // Act
            var response = controller.ValidateCode(lispCode);

            // Assert
            Assert.IsType<BadRequestResult>(response);
        }
    }
}
