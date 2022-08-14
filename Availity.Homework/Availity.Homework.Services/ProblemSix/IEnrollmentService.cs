using Availity.Homework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Availity.Homework.Services
{
    public interface IEnrollmentService
    {
        List<EnrollmentImportRowDTO> ParseEnrollmentCsv(string csvData);
        Dictionary<string, string> CreateSeperateCsvDataForEnrollmentByGrouping(List<EnrollmentImportRowDTO> rowDTOs, string groupBy);
        List<List<EnrollmentImportRowDTO>> GetEnrollmentGroups(List<EnrollmentImportRowDTO> rowDTOs, string groupBy);
    }
}
