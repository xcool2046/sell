using Microsoft.AspNetCore.Mvc;
using Sellsys.Application.DTOs.SalesFollowUp;
using Sellsys.Application.Interfaces;
using Sellsys.CrossCutting.Common;

namespace Sellsys.WebApi.Controllers
{
    [ApiController]
    [Route("api/salesfollowuplogs")]
    public class SalesFollowUpController : ControllerBase
    {
        private readonly ISalesFollowUpService _salesFollowUpService;

        public SalesFollowUpController(ISalesFollowUpService salesFollowUpService)
        {
            _salesFollowUpService = salesFollowUpService;
        }

        /// <summary>
        /// 获取所有销售跟进记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SalesFollowUpLogDto>>>> GetSalesFollowUpLogs()
        {
            var response = await _salesFollowUpService.GetAllSalesFollowUpLogsAsync();
            return Ok(response);
        }

        /// <summary>
        /// 根据ID获取销售跟进记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SalesFollowUpLogDto>>> GetSalesFollowUpLog(int id)
        {
            var response = await _salesFollowUpService.GetSalesFollowUpLogByIdAsync(id);
            if (!response.IsSuccess)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        /// <summary>
        /// 根据客户ID获取销售跟进记录列表
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<ApiResponse<List<SalesFollowUpLogDto>>>> GetSalesFollowUpLogsByCustomer(int customerId)
        {
            var response = await _salesFollowUpService.GetSalesFollowUpLogsByCustomerIdAsync(customerId);
            return Ok(response);
        }

        /// <summary>
        /// 创建销售跟进记录
        /// </summary>
        /// <param name="logDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<SalesFollowUpLogDto>>> CreateSalesFollowUpLog(SalesFollowUpLogUpsertDto logDto)
        {
            var response = await _salesFollowUpService.CreateSalesFollowUpLogAsync(logDto);
            if (response.Data == null)
            {
                return BadRequest(response);
            }
            return CreatedAtAction(nameof(GetSalesFollowUpLog), new { id = response.Data.Id }, response);
        }

        /// <summary>
        /// 更新销售跟进记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="logDto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSalesFollowUpLog(int id, SalesFollowUpLogUpsertDto logDto)
        {
            var response = await _salesFollowUpService.UpdateSalesFollowUpLogAsync(id, logDto);
            if (!response.IsSuccess)
            {
                return NotFound(response);
            }
            return NoContent();
        }

        /// <summary>
        /// 删除销售跟进记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSalesFollowUpLog(int id)
        {
            var response = await _salesFollowUpService.DeleteSalesFollowUpLogAsync(id);
            if (!response.IsSuccess)
            {
                return NotFound(response);
            }
            return NoContent();
        }
    }
}
