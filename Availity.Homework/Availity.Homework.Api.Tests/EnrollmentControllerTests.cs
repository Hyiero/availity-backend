using Availity.Homework.Api.Controllers;
using Availity.Homework.Models;
using Availity.Homework.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Availity.Homework.Api.Tests
{
    public class EnrollmentControllerTests
    {
        readonly EnrollmentController controller;
        readonly Mock<IEnrollmentService> mockEnrollmentService;
        readonly Mock<IFileService> mockFileService;

        public EnrollmentControllerTests()
        {
            mockEnrollmentService = new Mock<IEnrollmentService>();
            mockFileService = new Mock<IFileService>();
            controller = new EnrollmentController(mockEnrollmentService.Object, mockFileService.Object);
        }

        #region SaveEnrollmentInformation

        #region Helpers
        Mock<IFormFile> SetupMockFileReaderData(string content, string fileName)
        {
            var fileMock = new Mock<IFormFile>();
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);

            return fileMock;
        }
        #endregion

        [Fact]
        public void SaveEnrollmentInformation_ShouldReturnABadRequestResponse_WhenTheFilePassedInIsNull()
        {
            // Arrange
            // Act
            var response = controller.SplitEnrollmentBy("InsuranceCompany", null);

            // Assert
            Assert.IsType<BadRequestResult>(response);
        }

        [Fact]
        public void SaveEnrollmentInformation_ShouldReturnABadRequestResponse_WhenTheFilePassedInDoesNotHaveANameThatEndsWithDotCsv()
        {
            // Arrange
            var content = @"User Id,Name,Insurance Company,Version
                            1,Brandon Ripley,Atena,1";
            var fileName = "test.pdf";
            var file = SetupMockFileReaderData(content, fileName);

            // Act
            var response = controller.SplitEnrollmentBy("InsuranceCompany", file.Object);

            // Assert
            Assert.IsType<BadRequestResult>(response);
        }

        [Fact]
        public void SaveEnrollmentInformation_ShouldReturnOkObjectResultWithCorrectResponseData_WhenTheFilePassedIsValidAndIsParsedWithoutError()
        {
            // Arrange
            var content = @"User Id,Name,Insurance Company,Version
                            1,Brandon Ripley,Atena,1";
            var fileName = "test.csv";
            var file = SetupMockFileReaderData(content, fileName);
            var enrollments = new List<EnrollmentImportRowDTO>() { new EnrollmentImportRowDTO() };
            var groupBy = "InsuranceCompany";
            var newCsvs = new Dictionary<string, string>();
            mockEnrollmentService.Setup(x => x.ParseEnrollmentCsv(content)).Returns(enrollments).Verifiable();
            mockEnrollmentService.Setup(x => x.CreateSeperateCsvDataForEnrollmentByGrouping(enrollments, groupBy)).Returns(newCsvs).Verifiable();

            // Act
            var response = controller.SplitEnrollmentBy(groupBy, file.Object);

            // Assert
            mockEnrollmentService.Verify();
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(newCsvs, (response as OkObjectResult).Value);
        }
        #endregion
    }
}
