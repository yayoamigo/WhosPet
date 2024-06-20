using WhosPetCore.DTO.Incoming.Pets;
using WhosPetCore.ServiceContracts.ReportContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace WhosPetUI.Controllers.ReportingControllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LostPetReportController : ControllerBase
    {
        private readonly IAddLostReport _addLostReportService;
        private readonly IGetLostReports _getLostReportsService;
        private readonly IUpdatePetReport _updatePetReportService;
        private readonly ILogger<LostPetReportController> _logger;

        public LostPetReportController(
            IAddLostReport addLostReportService,
            IGetLostReports getLostReportsService,
            IUpdatePetReport updatePetReportService,
            ILogger<LostPetReportController> logger
        )
        {
            _addLostReportService = addLostReportService;
            _getLostReportsService = getLostReportsService;
            _updatePetReportService = updatePetReportService;
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateLostPetReport([FromBody] LostReportDTO reportDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _addLostReportService.CreateLostPetReportAsync(reportDto);
            if (!result)
            {
                _logger.LogError("Failed to create lost pet report");
                return StatusCode(500, "Failed to create lost pet report");
            }

            return Ok("Lost pet report created successfully");
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("by-Id/{id}")]
        public async Task<IActionResult> GetLostPetReportsByCity(string City)
        {
            if(string.IsNullOrEmpty(City))
            {
                return BadRequest("City cannot be null or empty");
            }
            var result = await _getLostReportsService.GetLostPetReportByCity(City);
            if (result == null)
            {
                _logger.LogError($"No lost pet reports found for: {City}");
                return NotFound($"No lost pet reports found for: {City}");
            }

            return Ok(result);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPatch]
        [Route("found/{id}")]
        public async Task<IActionResult> FoundLostPet(int id)
        {
            if (id == 0)
            {
                return BadRequest("Id cannot be null or empty");
            }

            var result = await _updatePetReportService.FoundLostPet(id);
            if (!result)
            {
                _logger.LogError($"Failed to mark lost pet report with id: {id} as found");
                return StatusCode(500, $"Failed to mark lost pet report with id: {id} as found");
            }

            return Ok($"Lost pet report with id: {id} marked as found successfully");
        }


    }
}