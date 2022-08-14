using Availity.Homework.Services;
using System;
using Xunit;

namespace Availity.Homework.Services.Tests
{
    public class LispServiceTests
    {
        readonly ILispService service;

        public LispServiceTests()
        {
            service = new LispService();   
        }

        [Fact]
        public void Valid_ShouldReturnFalse_IfThereIsMoreClosingParenthesesThanOpeningOnes()
        {
            // Arrange
            var lispCode = "((c)) b)";

            // Act
            // Assert
            Assert.False(service.Valid(lispCode));
        }

        [Fact]
        public void Valid_ShouldReturnFalse_IfNotAllOpeningParenthesesHaveBeenClosed()
        {
            // Arrange
            var lispCode = "((s (f))";

            // Act
            // Assert
            Assert.False(service.Valid(lispCode));
        }

        [Fact]
        public void Valid_ShouldReturnTrue_IfAllParenthesesAreProperlyClosedAndNested()
        {
            // Arrange
            var lispCode = "((s (f) b) e)";

            // Act
            // Assert
            Assert.True(service.Valid(lispCode));
        }
    }
}
