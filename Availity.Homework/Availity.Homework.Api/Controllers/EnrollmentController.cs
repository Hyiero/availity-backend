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
        readonly IFileService fileService;

        public EnrollmentController(IEnrollmentService enrollmentService, IFileService fileService)
        {
            this.enrollmentService = enrollmentService;
            this.fileService = fileService;
        }

        [Consumes("multipart/form-data")]
        [HttpPost("split")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult SplitEnrollmentBy([FromQuery] string groupBy, IFormFile file)
        {
            if (file == null || !file.FileName.EndsWith(".csv"))
            {
                return BadRequest();
            }

            var enrollments = enrollmentService.ParseEnrollmentCsv(ExtractContentsOfFile(file));
            var splitCsvs = enrollmentService.CreateSeperateCsvDataForEnrollmentByGrouping(enrollments, groupBy);

            // This code would save the files to the disk drive on the server. Since I decided to do these questions in a webapi form
            // I thought it not wise to save to the disk drive and send back the filepaths to the users, instead I sent the newly created csv file contents
            // to the frontend and used the front to put that content into blobs that would then be saved as .csv files
            // If you uncomment to try it out you will need to also change IActionResult to async Task<IActionResult>
            // await fileService.SaveFiles(splitCsvs);

            return Ok(splitCsvs);
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
