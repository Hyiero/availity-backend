using System;
using System.Collections.Generic;
using System.Text;

namespace Availity.Homework.Services
{
    public interface ICsvParser
    {
        List<Dictionary<string, string>> Parse(string csvData);
        List<T> ConvertCsvGeneratedRecordsToTypedObject<T>(List<Dictionary<string, string>> records) where T : new();
        string Serialize<T>(List<T> items);
    }
}
