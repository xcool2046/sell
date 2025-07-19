using Microsoft.AspNetCore.Mvc;
using Sellsys.Application.DTOs.AfterSales;
using Sellsys.Application.Interfaces;
using Sellsys.CrossCutting.Common;

namespace Sellsys.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AfterSalesController : ControllerBase
    {
        private readonly IAfterSalesService _afterSalesService;

        public AfterSalesController(IAfterSalesService afterSalesService)
        {
            _afterSalesService = afterSalesService;
        }

        /// <summary>
        /// 获取所有售后记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<AfterSalesRecordDto>>>> GetAfterSalesRecords()
        {
            var response = await _afterSalesService.GetAllAfterSalesRecordsAsync();
            return Ok(response);
        }

        /// <summary>
        /// 根据ID获取售后记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AfterSalesRecordDto>>> GetAfterSalesRecord(int id)
        {
            var response = await _afterSalesService.GetAfterSalesRecordByIdAsync(id);
            if (!response.IsSuccess)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        /// <summary>
        /// 根据客户ID获取售后记录列表
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<ApiResponse<List<AfterSalesRecordDto>>>> GetAfterSalesRecordsByCustomer(int customerId)
        {
            var response = await _afterSalesService.GetAfterSalesRecordsByCustomerIdAsync(customerId);
            return Ok(response);
        }

        /// <summary>
        /// 搜索售后记录
        /// </summary>
        /// <param name="customerName">客户名称</param>
        /// <param name="province">省份</param>
        /// <param name="city">城市</param>
        /// <param name="status">处理状态</param>
        /// <returns></returns>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<AfterSalesRecordDto>>>> SearchAfterSalesRecords(
            [FromQuery] string? customerName = null,
            [FromQuery] string? province = null,
            [FromQuery] string? city = null,
            [FromQuery] string? status = null)
        {
            var response = await _afterSalesService.SearchAfterSalesRecordsAsync(customerName, province, city, status);
            return Ok(response);
        }

        /// <summary>
        /// 创建售后记录
        /// </summary>
        /// <param name="recordDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AfterSalesRecordDto>>> CreateAfterSalesRecord(AfterSalesRecordUpsertDto recordDto)
        {
            var response = await _afterSalesService.CreateAfterSalesRecordAsync(recordDto);
            if (response.Data == null)
            {
                return BadRequest(response);
            }
            return CreatedAtAction(nameof(GetAfterSalesRecord), new { id = response.Data.Id }, response);
        }

        /// <summary>
        /// 更新售后记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="recordDto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAfterSalesRecord(int id, AfterSalesRecordUpsertDto recordDto)
        {
            var response = await _afterSalesService.UpdateAfterSalesRecordAsync(id, recordDto);
            if (!response.IsSuccess)
            {
                return NotFound(response);
            }
            return NoContent();
        }

        /// <summary>
        /// 删除售后记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAfterSalesRecord(int id)
        {
            var response = await _afterSalesService.DeleteAfterSalesRecordAsync(id);
            if (!response.IsSuccess)
            {
                return NotFound(response);
            }
            return NoContent();
        }
    }
}
