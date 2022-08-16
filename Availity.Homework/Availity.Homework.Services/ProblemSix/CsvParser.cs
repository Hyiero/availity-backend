using Availity.Homework.Models;
using Availity.Homework.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Availity.Homework.Services
{
    public class CsvParser : ICsvParser
    {
        public CsvParser() { }

        public List<Dictionary<string, string>> Parse(string csvData)
        {
            var csvRecordsArray = csvData.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrWhiteSpace(x));
            return GetDataRecords(csvRecordsArray);
        }

        public List<T> ConvertCsvGeneratedRecordsToTypedObject<T>(List<Dictionary<string, string>> records) where T : new()
        {
            var items = new List<T>();
            var properities = typeof(T).GetProperties();

            foreach (var record in records)
            {
                var item = new T();
                foreach (var prop in properities)
                {
                    var cellMatch = record.FirstOrDefault(x => x.Key == prop.Name);
                    if (!cellMatch.Equals(default(KeyValuePair<string, string>)))
                    {
                        prop.SetValue(item, Convert.ChangeType(cellMatch.Value, prop.PropertyType));
                    }
                }
                items.Add(item);
            }

            return items;
        }

        public string Serialize<T>(List<T> items)
        {
            var csvDataSB = new StringBuilder();
            var properties = typeof(T).GetProperties();

            csvDataSB.AppendLine(SerializeHeader(properties));
            csvDataSB.Append(SerializeDataRows(items, properties));

            return csvDataSB.ToString();
        }

        #region Private Methods
        private string SerializeHeader(PropertyInfo[] properties)
        {
            var headerSB = new StringBuilder();
            for (var i = 0; i < properties.Length; i++)
            {
                headerSB.AppendWithPrefixComma(properties[i].Name, i != 0);
            }

            return headerSB.ToString();
        }

        private string SerializeDataRows<T>(List<T> items, PropertyInfo[] properties)
        {
            var rowsSB = new StringBuilder();
            var rowSB = new StringBuilder();

            foreach (var item in items)
            {
                rowSB.Clear();
                for (var i = 0; i < properties.Length; i++)
                {
                    var value = properties[i].GetValue(item, null)?.ToString();
                    // Wrap strings in double quotes when a comma is present in the string
                    if (value != null && properties[i].PropertyType.FullName == "System.String" && value.Contains(','))
                    {
                        value = "\"" + value + "\"";
                    }
                    rowSB.AppendWithPrefixComma(value, i != 0);
                }
                rowsSB.AppendLine(rowSB.ToString());
            }

            return rowsSB.ToString();
        }

        private List<string> GetHeader(string headerRow)
        {
            return ParseCommaSeperatedStringIntoStringList(headerRow, true);
        }

        private List<Dictionary<string, string>> GetDataRecords(IEnumerable<string> csvRecordsArray)
        {
            var rows = new List<Dictionary<string, string>>();
            var headerData = GetHeader(csvRecordsArray.First());

            foreach (var csvRecord in csvRecordsArray.Skip(1))
            {
                var rowData = ParseCommaSeperatedStringIntoStringList(csvRecord);
                rows.Add(PairHeaderWithData(rowData, headerData));
            }

            return rows;
        }

        private Dictionary<string, string> PairHeaderWithData(List<string> rowData, List<string> headerData)
        {
            if (headerData.Count != rowData.Count)
            {
                throw new FormatException("Row data does not match the count of the header data");
            }

            var currentRowDict = new Dictionary<string, string>();

            for (int i = 0; i < headerData.Count; i++)
            {
                currentRowDict.Add(headerData[i], rowData[i]);
            }

            return currentRowDict;
        }

        private List<string> ParseCommaSeperatedStringIntoStringList(string line, bool removeSpaces = false)
        {
            var reg = new Regex(",(?=(?:(?:[^\"]*\"){2})*[^\"]*$)");
            var rawData = reg.Split(line);
            var data = new List<string>();
            foreach (var row in rawData)
            {
                data.Add(removeSpaces ? Regex.Replace(RemoveDoubleQuoteTextQualifier(row), @"\s+", "") : RemoveDoubleQuoteTextQualifier(row));
            }

            return data;
        }

        private string RemoveDoubleQuoteTextQualifier(string cell)
        {
            Regex reg = new Regex("^\"(.+(?=\"$))\"$");
            return reg.Replace(cell, "$1").Trim();
        }
        #endregion
    }
}
