using Availity.Homework.Models;
using Availity.Homework.Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace Availity.Homework.Services.Tests
{
    public class CsvParserTests
    {
        readonly ICsvParser service;

        public CsvParserTests()
        {
            service = new CsvParser();
        }

        #region Parse
        [Fact]
        public void ParseCsv_ShouldReturnTheNumberOfLinesMinusTheFirstLineAndAnyEmptyLinesInTheString_WhenCalledWithMultipleLinesWithSomeThatAreEmpty()
        {
            // Arrange
            // 3 records, 1 header row, 2 empty rows
            var csvData = @"User Id,Name,Insurance Company,Version
                            1,Brandon Ripley,Atena,1
                            1,Brandon Ripley,Atena,3

                            2,Juan Jaspe,Blue Cross,1
                            ";

            // Act
            var records = service.Parse(csvData);

            // Assert
            Assert.Equal(3, records.Count);
        }

        [Fact]
        public void ParseCsv_ShouldReturnTheSameNumberOfEntriesInTheRecordsRowAsTheHeaderRow_WhenCalledWithMultipleLinesThatMatchTheAmountOfRowDataWithHeaderData()
        {
            // Arrange
            var csvData = @"User Id,Name,Insurance Company,Version, Blah
                            1,Brandon Ripley,Atena,1,1
                            1,Brandon Ripley,Atena,3,1
                            2,Juan Jaspe,Blue Cross,1,1
                            3,Ashley Con,Medicare,1,1";

            // Act
            var records = service.Parse(csvData);

            // Assert
            Assert.Equal(5, records[0].Count);
        }

        [Fact]
        public void ParseCsv_ShouldReturnEmptyList_WhenCalledWithOnlyOneLine()
        {
            // Arrange
            var csvData = @"User Id,Name,Insurance Company,Version, Blah";

            // Act
            var records = service.Parse(csvData);

            // Assert
            Assert.Empty(records);
        }

        [Fact]
        public void ParseCsv_ShouldThrowFormatException_WhenCalledWithMultipleLinesAndTheAmountOfRowDataDoesNotMatchTheHeaderData()
        {
            // Arrange
            var csvData = @"User Id,Name,Insurance Company,Version, Blah
                            1,Brandon Ripley,Atena,1
                            1,Brandon Ripley,Atena,3
                            2,Juan Jaspe,Blue Cross,1
                            3,Ashley Con,Medicare,1";

            // Act
            // Assert
            Assert.Throws<FormatException>(() => service.Parse(csvData));
        }

        [Fact]
        public void ParseCsv_ShouldRemoveDoubleQuotesFromRowData_WhenCalledWithMultipleLinesWithSomeThatContainDoubleQuotesAroundRowData()
        {
            // Arrange
            var csvData = @"User Id,Name,Insurance Company,Version
                            1,""Brandon Ripley"",Atena,1
                            1,Brandon Ripley,Atena,""3""
                            2,Juan Jaspe,Blue Cross,1
                            ";

            // Act
            var records = service.Parse(csvData);

            // Assert
            Assert.Equal("Brandon Ripley", records[0]["Name"]);
            Assert.Equal("3", records[1]["Version"]);
        }

        [Fact]
        public void ParseCsv_ShouldRemoveDoubleQuotesFromHeaderData_WhenCalledWithMultipleLinesWithSomeThatContainDoubleQuotesAroundHeaderData()
        {
            // Arrange
            var csvData = @"User Id,""Name"",""Insurance Company"",Version
                            1,Brandon Ripley,Atena,1";

            // Act
            var records = service.Parse(csvData);

            // Assert
            Assert.True(records[0].ContainsKey("Name"));
            Assert.True(records[0].ContainsKey("InsuranceCompany"));
        }

        [Fact]
        public void ParseCsv_ShouldRemoveSpacesFromHeaderFields_WhenCalledWithMultipleLines()
        {
            // Arrange
            var csvData = @"User Id,Name,Insurance Company,Version
                            1,Brandon Ripley,Atena,1";

            // Act
            var records = service.Parse(csvData);

            // Assert
            Assert.True(records[0].ContainsKey("UserId"));
            Assert.True(records[0].ContainsKey("InsuranceCompany"));
        }

        [Fact]
        public void ParseCsv_ShouldReturnRowDataFieldWithCommaStillPresent_WhenCalledWithMultipleLinesWhereFieldIsSurroundedByDoubleQuotesWithACommaInside()
        {
            // Arrange
            var csvData = @"User Id,Name,Insurance Company,Version
                            1,""Ripley, Brandon"",Atena,""1,000,000""";

            // Act
            var records = service.Parse(csvData);

            // Assert
            Assert.Equal("1,000,000", records[0]["Version"]);
            Assert.Equal("Ripley, Brandon", records[0]["Name"]);
        }
        #endregion

        #region ConvertCsvGeneratedRecordsToTypedObject

        #region Helpers
        public class SomeData
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int Number { get; set; }
            public bool HasValue { get; set; }
        };

        Dictionary<string, string> FillInDictionaryToMatchSomeData(string name, string description, string number, string hasValue)
        {
            return new Dictionary<string, string>()
            {
                { "Name", name },
                { "Description", description },
                { "Number", number },
                { "HasValue", hasValue }
            };
        }
        #endregion

        [Fact]
        public void ConvertCsvGeneratedRecordsToTypedObject_ShouldReturnTheSameNumberOfItemsAsRecords_WhenRecordsAreValid()
        {
            // Arrange
            var records = new List<Dictionary<string, string>>();
            records.Add(FillInDictionaryToMatchSomeData("Cool", "Super Cool", "43", "true"));
            records.Add(FillInDictionaryToMatchSomeData("Not Cool", "Super Not Cool", "3", "false"));

            // Act
            var items = service.ConvertCsvGeneratedRecordsToTypedObject<SomeData>(records);

            // Assert
            Assert.Equal(records.Count, items.Count);
        }

        [Fact]
        public void ConvertCsvGeneratedRecordsToTypedObject_ShouldReturnTheItemsFilledInBasedOnTheMatchingDictionaryData_WhenRecordsContainDictionariesThatContainMatchesOnTheObjectPropertyValues()
        {
            // Arrange
            var records = new List<Dictionary<string, string>>();
            records.Add(FillInDictionaryToMatchSomeData("Cool", "Super Cool", "43", "true"));
            records[0].Remove("Number");
            records.Add(FillInDictionaryToMatchSomeData("Not Cool", "Super Not Cool", "3", "false"));
            records[1].Add("SomeOtherValue", "Sky");

            // Act
            var items = service.ConvertCsvGeneratedRecordsToTypedObject<SomeData>(records);

            // Assert
            Assert.Equal(records[0]["Name"], items[0].Name);
            Assert.Equal(records[0]["Description"], items[0].Description);
            Assert.Equal(default(int), items[0].Number);
            Assert.Equal(bool.Parse(records[0]["HasValue"]), items[0].HasValue);
            Assert.Equal(records[1]["Name"], items[1].Name);
            Assert.Equal(records[1]["Description"], items[1].Description);
            Assert.Equal(int.Parse(records[1]["Number"]), items[1].Number);
            Assert.Equal(bool.Parse(records[1]["HasValue"]), items[1].HasValue);
        }
        #endregion

        #region Serialize
        [Fact]
        public void Serialize_ShouldReturnTheCsvDataWithTheObjectProperitiesAsTheFirstLine_WhenCalledWithAPopulatedObject()
        {
            // Arrange
            var expectedHeaderRow = "UserId,Name,Version,InsuranceCompany";
            var rowOne = new EnrollmentImportRowDTO() { Name = "Cool, Sauce", InsuranceCompany = "Atena", UserId = "3", Version = 1002 };
            var rows = new List<EnrollmentImportRowDTO>() { rowOne };

            // Act
            var csvData = service.Serialize(rows);

            // Assert
            Assert.Contains(expectedHeaderRow, csvData);
        }
        #endregion
    }
}
