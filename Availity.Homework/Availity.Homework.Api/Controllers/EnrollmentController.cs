using Availity.Homework.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Availity.Homework.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentController : ControllerBase
    {
        readonly IEnrollmentService enrollmentService;

        public EnrollmentController(IEnrollmentService enrollmentService)
        {
            this.enrollmentService = enrollmentService;
        }

        [HttpPost("split")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SplitEnrollmentBy([FromQuery] string groupBy, IFormFile file)
        {
            if (file == null || !file.Name.EndsWith(".csv"))
            {
                return BadRequest();
            }

            var enrollments = enrollmentService.ParseEnrollmentCsv(ExtractContentsOfFile(file));
            var splitCsvs = enrollmentService.CreateSeperateCsvDataForEnrollmentByGrouping(enrollments, groupBy);

            return Ok();
        }

        #region Private Methods
        private string ExtractContentsOfFile(IFormFile file)
        {
            using (var fileStream = file.OpenReadStream())
            using (var reader = new StreamReader(fileStream))
            {
                return reader.ReadToEnd();
            }
        }
        #endregion
    }
}
