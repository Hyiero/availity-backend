using Availity.Homework.Models;
using Availity.Homework.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Availity.Homework.Services.Tests
{
    public class EnrollmentServiceTests
    {
        readonly IEnrollmentService service;
        readonly Mock<ICsvParser> mockCsvParser;

        public EnrollmentServiceTests()
        {
            mockCsvParser = new Mock<ICsvParser>();
            service = new EnrollmentService(mockCsvParser.Object);   
        }

        #region ParseEnrollmentCsv
        [Fact]
        public void ParseEnrollmentCsv_ShouldEnrollmentRecords_IfInvokedWithNoExceptions()
        {
            // Arrange
            var csvData = "";
            var records = new List<Dictionary<string, string>>();
            var enrollmentRecords = new List<EnrollmentImportRowDTO>();
            mockCsvParser.Setup(x => x.Parse(csvData)).Returns(records).Verifiable();
            mockCsvParser.Setup(x => x.ConvertCsvGeneratedRecordsToTypedObject<EnrollmentImportRowDTO>(records)).Returns(enrollmentRecords).Verifiable();

            // Act
            var results = service.ParseEnrollmentCsv(csvData);

            // Assert
            mockCsvParser.Verify();
            Assert.Same(enrollmentRecords, results);
        }

        [Fact]
        public void ParseEnrollmentCsv_ShouldBubbleUpTheThrownServiceException_IfParseCsvThrowsException()
        {
            // Arrange
            var csvData = "";
            var exceptionThrown = new Exception();
            mockCsvParser.Setup(x => x.Parse(csvData)).Throws(exceptionThrown);

            // Act
            // Assert
            var thrownException = Assert.Throws<Exception>(() => service.ParseEnrollmentCsv(csvData));
            Assert.Same(exceptionThrown, thrownException);
        }

        [Fact]
        public void ParseEnrollmentCsv_ShouldBubbleUpTheThrownServiceException_IfConvertCsvGeneratedRecordsToTypedObjectThrowsException()
        {
            // Arrange
            var csvData = "";
            var records = new List<Dictionary<string, string>>();
            var exceptionThrown = new Exception();
            mockCsvParser.Setup(x => x.Parse(csvData)).Returns(records);
            mockCsvParser.Setup(x => x.ConvertCsvGeneratedRecordsToTypedObject<EnrollmentImportRowDTO>(records)).Throws(exceptionThrown);

            // Act
            // Assert
            var thrownException = Assert.Throws<Exception>(() => service.ParseEnrollmentCsv(csvData));
            Assert.Same(exceptionThrown, thrownException);
        }
        #endregion

        #region CreateSeperateCsvDataForEnrollmentByGrouping
        [Theory]
        [InlineData("InsuranceCompany", 3)]
        [InlineData("Name", 1)]
        [InlineData("UserId", 4)]
        public void CreateSeperateCsvDataForEnrollmentByGrouping_ShouldReturnTheCorrectNumberOfItemsBasedOnGrouping_WhenAValidPropertyNameIsSpecified(string groupBy, int totalGroups)
        {
            // Arrange
            var rowDTOs = new List<EnrollmentImportRowDTO>();
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "AB", Name = "C", UserId = "1", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "AB", Name = "C", UserId = "2", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "AC", Name = "C", UserId = "3", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "AD", Name = "C", UserId = "4", Version = 1 });

            mockCsvParser.Setup(x => x.Serialize(It.IsAny<List<EnrollmentImportRowDTO>>())).Returns("");

            // Act
            var results = service.CreateSeperateCsvDataForEnrollmentByGrouping(rowDTOs, groupBy);

            // Assert
            Assert.Equal(totalGroups, results.Count);
        }

        [Fact]
        public void CreateSeperateCsvDataForEnrollmentByGrouping_ShouldCallSerializeWithTheCorrectGroupOrderedByNameAscendingAndReturnTheCorrectKeyPairResults_WhenDuplicateUsersArePresentForSameInsuranceCompanyExist()
        {
            // Arrange
            var rowDTOs = new List<EnrollmentImportRowDTO>();
            var firstCsvData = "adssda";
            var secondCsvData = "dasdasd";
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Atena", Name = "Brandon Ripley", UserId = "1", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Atena", Name = "Brandon Ripley", UserId = "1", Version = 2 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Atena", Name = "Brandon Ripley", UserId = "1", Version = 3 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Con", UserId = "2", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Con", UserId = "2", Version = 2 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Anderson", UserId = "3", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Anderson", UserId = "3", Version = 2 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Brandon Ripley", UserId = "1", Version = 2 });
            mockCsvParser.SetupSequence(x => x.Serialize(It.IsAny<List<EnrollmentImportRowDTO>>()))
                .Returns(firstCsvData).Returns(secondCsvData);

            // Act
            var results = service.CreateSeperateCsvDataForEnrollmentByGrouping(rowDTOs, "InsuranceCompany");

            // Assert
            Assert.Equal(firstCsvData, results["Atena"]);
            Assert.Equal(secondCsvData, results["Medicare"]);
            mockCsvParser.Verify(x => x.Serialize(It.Is<List<EnrollmentImportRowDTO>>(x => x.Count == 1 && x[0].InsuranceCompany == "Atena" && x[0].Name == "Brandon Ripley" && x[0].Version == 3)), Times.Once);
            mockCsvParser.Verify(x => x.Serialize(It.Is<List<EnrollmentImportRowDTO>>(x => x.Count == 3 
                && x[0].InsuranceCompany == "Medicare" && x[0].Name == "Ashley Anderson" && x[0].Version == 2
                && x[1].InsuranceCompany == "Medicare" && x[1].Name == "Ashley Con" && x[1].Version == 2
                && x[2].InsuranceCompany == "Medicare" && x[2].Name == "Brandon Ripley" && x[2].Version == 2))
            , Times.Once);
        }

        [Fact]
        public void CreateSeperateCsvDataForEnrollmentByGrouping_ShouldThrowNullReferenceException_WhenGroupByIsProvidedThatIsntAValidModelProperty()
        {
            // Arrange
            var rowDTOs = new List<EnrollmentImportRowDTO>();
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Atena", Name = "Brandon Ripley", UserId = "1", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Atena", Name = "Brandon Ripley", UserId = "1", Version = 2 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Atena", Name = "Brandon Ripley", UserId = "1", Version = 3 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Con", UserId = "2", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Con", UserId = "2", Version = 2 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Anderson", UserId = "3", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Anderson", UserId = "3", Version = 2 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Brandon Ripley", UserId = "1", Version = 2 });
            mockCsvParser.Setup(x => x.Serialize(It.IsAny<List<EnrollmentImportRowDTO>>())).Returns("");

            // Act
            // Assert
            Assert.Throws<NullReferenceException>(() => service.CreateSeperateCsvDataForEnrollmentByGrouping(rowDTOs, "SomethingNotValid"));
        }
        #endregion

        #region GetEnrollmentGroups
        [Theory]
        [InlineData("InsuranceCompany", 3)]
        [InlineData("Name", 1)]
        [InlineData("UserId", 4)]
        public void GetEnrollmentGroups_ShouldReturnEnrollmentsGroupedByTheSpecifiedPropertyName_WhenAValidPropertyNameIsSpecified(string groupBy, int totalGroups)
        {
            // Arrange
            var rowDTOs = new List<EnrollmentImportRowDTO>();
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "AB", Name = "C", UserId = "1", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "AB", Name = "C", UserId = "2", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "AC", Name = "C", UserId = "3", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "AD", Name = "C", UserId = "4", Version = 1 });

            // Act
            var results = service.GetEnrollmentGroups(rowDTOs, groupBy);

            // Assert
            Assert.Equal(totalGroups, results.Count);
        }

        [Fact]
        public void GetEnrollmentGroups_ShouldReturnEnrollmentsWithOneUnquieUserPerInsuranceCompanyContainingTheHighestVersionOutOfAllTheRows_WhenDuplicateUsersForSameInsuranceCompanyExist()
        {
            // Arrange
            var rowDTOs = new List<EnrollmentImportRowDTO>();
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Atena", Name = "Brandon Ripley", UserId = "1", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Atena", Name = "Brandon Ripley", UserId = "1", Version = 2 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Atena", Name = "Brandon Ripley", UserId = "1", Version = 3 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Con", UserId = "2", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Con", UserId = "2", Version = 2 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Anderson", UserId = "3", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Anderson", UserId = "3", Version = 2 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Brandon Ripley", UserId = "1", Version = 2 });

            // Act
            var results = service.GetEnrollmentGroups(rowDTOs, "InsuranceCompany");

            // Assert
            Assert.Single(results[0]);
            Assert.Equal("Brandon Ripley", results[0][0].Name);
            Assert.Equal(3, results[0][0].Version);
            Assert.Equal(3, results[1].Count);
            var row = results[1].FirstOrDefault(x => x.Name == "Brandon Ripley");
            Assert.NotNull(row);
            Assert.Equal(2, row.Version);
        }

        [Fact]
        public void GetEnrollmentGroups_ShouldReturnEnrollmentsOrderedByNameAscendingInsideEachGroup_WhenWeHaveMultipleUsersPresentForAnInsuranceCompany()
        {
            // Arrange
            var rowDTOs = new List<EnrollmentImportRowDTO>();
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Con", UserId = "2", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Con", UserId = "2", Version = 2 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Anderson", UserId = "3", Version = 1 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Ashley Anderson", UserId = "3", Version = 2 });
            rowDTOs.Add(new EnrollmentImportRowDTO() { InsuranceCompany = "Medicare", Name = "Brandon Ripley", UserId = "1", Version = 2 });

            // Act
            var results = service.GetEnrollmentGroups(rowDTOs, "InsuranceCompany");

            // Assert
            Assert.Equal("Ashley Anderson", results[0][0].Name);
            Assert.Equal("Ashley Con", results[0][1].Name);
            Assert.Equal("Brandon Ripley", results[0][2].Name);
        }
        #endregion
    }
}
