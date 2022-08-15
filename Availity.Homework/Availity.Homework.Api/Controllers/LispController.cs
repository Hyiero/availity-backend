using Availity.Homework.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Availity.Homework.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LispController : ControllerBase
    {
        readonly ILispService lispService;

        public LispController(ILispService lispService)
        {
            this.lispService = lispService;
        }

        [HttpPost("validate/code")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult ValidateCode([FromBody]string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest();
            }

            return Ok(lispService.Valid(code));
        }
    }
}
