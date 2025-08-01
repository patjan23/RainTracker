using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RainTracker.Data;
using RainTracker.Models;

namespace RainTracker.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RainController : ControllerBase
    {
        private readonly RainContext _context;
        private readonly ILogger<RainController> _logger;

        public RainController(RainContext context, ILogger<RainController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves rain data for a specific user
        /// </summary>
        /// <param name="userId">User identifier from x-userId header</param>
        /// <returns>Array of rain data records for the user</returns>
        /// <response code="200">Returns the rain data for the user</response>
        /// <response code="400">If the x-userId header is missing or invalid</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RainDataResponse>> Get()
        {
            var userId = Request.Headers["x-userid"].FirstOrDefault();
            try
            {
                
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("GET request received without x-userId header");
                    return BadRequest(new { error = "x-userId header is required" });

                }

                _logger.LogInformation("Retrieving rain data for user: {UserId}", userId);

                var rainData = await _context.RainRecords
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Timestamp)
                .Select(rd => new RainDataItem
                {
                    Timestamp = rd.Timestamp,
                    Rain = rd.Rain
                })
                .ToListAsync();

                _logger.LogInformation("Retrieved {Count} rain data records for user: {UserId}", rainData.Count, userId);

                var response = new RainDataResponse
                {
                    Data = rainData
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rain data for user: {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error occurred while retrieving data" });
            }
        }

        /// <summary>
        /// Records new rain data for a user
        /// </summary>
        /// <param name="userId">User identifier from x-userId header</param>
        /// <param name="request">Rain data to record</param>
        /// <returns>Status of the operation</returns>
        /// <response code="201">Rain data successfully recorded</response>
        /// <response code="400">If the request is invalid or x-userId header is missing</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Post([FromBody] RainRecordDto? input)
        {
            var userId = Request.Headers["x-userid"].FirstOrDefault();
            try
            {
                if (string.IsNullOrWhiteSpace(userId))                 
                {
                    _logger.LogWarning("POST request received without x-userId header");
                    return BadRequest(new { error = "x-userId header is required" });
                }

                if (input == null) 
                {
                    _logger.LogWarning("POST request received with null request body for user: {UserId}", userId);
                    return BadRequest(new { error = "Request body is required" });
                }

                _logger.LogInformation("Recording rain data for user: {UserId}, Rain: {Rain}", userId, input.Rain);

                var record = new RainRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    Rain = input.Rain,
                    Timestamp = DateTime.UtcNow,
                    UserId = userId
                };

                _context.RainRecords.Add(record);
                await _context.SaveChangesAsync();               

                _logger.LogInformation("Rain data recorded successfully for user: {UserId}, ID: {Id}", userId, record.Id);

                return CreatedAtAction(
                    nameof(Get),
                    new { userId },
                    new { id = record.Id, message = "Rain data recorded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording rain data for user: {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error occurred while recording data" });
            }
        }
    }
}
