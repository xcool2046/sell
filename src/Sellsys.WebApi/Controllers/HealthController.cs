using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sellsys.Infrastructure.Data;
using Sellsys.CrossCutting.Common;

namespace Sellsys.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly SellsysDbContext _context;

        public HealthController(SellsysDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            try
            {
                // Check database connectivity
                await _context.Database.CanConnectAsync();
                
                var healthStatus = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Database = "Connected",
                    Version = "1.0.0"
                };

                return Ok(ApiResponse<object>.Success(healthStatus));
            }
            catch (Exception ex)
            {
                var healthStatus = new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Database = "Disconnected",
                    Error = ex.Message,
                    Version = "1.0.0"
                };

                var failResponse = new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Service Unavailable",
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Data = healthStatus
                };
                return StatusCode(503, failResponse);
            }
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(ApiResponse<string>.Success("pong"));
        }
    }
}
