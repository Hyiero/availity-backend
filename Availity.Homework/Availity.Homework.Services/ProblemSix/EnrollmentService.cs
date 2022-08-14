using Availity.Homework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Availity.Homework.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        readonly ICsvParser csvParser;

        public EnrollmentService(ICsvParser csvParser)
        {
            this.csvParser = csvParser;
        }

        public List<EnrollmentImportRowDTO> ParseEnrollmentCsv(string csvData)
        {
            var records = csvParser.Parse(csvData);
            return csvParser.ConvertCsvGeneratedRecordsToTypedObject<EnrollmentImportRowDTO>(records);
        }

        public Dictionary<string, string> CreateSeperateCsvDataForEnrollmentByGrouping(List<EnrollmentImportRowDTO> rowDTOs, string groupBy)
        {
            var csvDatas = new Dictionary<string, string>();
            var enrollmentGroups = GetEnrollmentGroups(rowDTOs, groupBy);

            foreach(var group in enrollmentGroups)
            {
                var csvData = csvParser.Serialize(group);
                var firstItemInGroup = group.First();
                var uniqueGroupingName = firstItemInGroup.GetType().GetProperty(groupBy).GetValue(firstItemInGroup, null).ToString();
                csvDatas.Add(uniqueGroupingName, csvData);
            }

            return csvDatas;
        }

        public List<List<EnrollmentImportRowDTO>> GetEnrollmentGroups(List<EnrollmentImportRowDTO> rowDTOs, string groupBy)
        {
            var consolidatedInsuranceUsersRows = rowDTOs.GroupBy(x => new { x.UserId, x.InsuranceCompany }).Select(x => x.OrderBy(x => x.Version).Last());

            return consolidatedInsuranceUsersRows.GroupBy(x => x.GetType().GetProperty(groupBy).GetValue(x, null))
                    .Select(x => x.OrderBy(y => y.Name).ToList()).ToList();
        }
    }
}
