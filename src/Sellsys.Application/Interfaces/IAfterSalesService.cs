using Sellsys.Application.DTOs.AfterSales;
using Sellsys.CrossCutting.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sellsys.Application.Interfaces
{
    public interface IAfterSalesService
    {
        /// <summary>
        /// 获取客户售后服务聚合信息（用于售后服务主界面）
        /// </summary>
        /// <returns></returns>
        Task<ApiResponse<List<CustomerAfterSalesDto>>> GetCustomersWithAfterSalesInfoAsync();

        /// <summary>
        /// 搜索客户售后服务聚合信息
        /// </summary>
        /// <param name="customerName">客户名称</param>
        /// <param name="supportPersonName">客服人员名称</param>
        /// <param name="status">处理状态</param>
        /// <returns></returns>
        Task<ApiResponse<List<CustomerAfterSalesDto>>> SearchCustomersWithAfterSalesInfoAsync(
            string? customerName = null,
            string? supportPersonName = null,
            string? status = null);

        /// <summary>
        /// 获取所有售后记录
        /// </summary>
        /// <returns></returns>
        Task<ApiResponse<List<AfterSalesRecordDto>>> GetAllAfterSalesRecordsAsync();

        /// <summary>
        /// 根据ID获取售后记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ApiResponse<AfterSalesRecordDto>> GetAfterSalesRecordByIdAsync(int id);

        /// <summary>
        /// 根据客户ID获取售后记录列表
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        Task<ApiResponse<List<AfterSalesRecordDto>>> GetAfterSalesRecordsByCustomerIdAsync(int customerId);
        
        /// <summary>
        /// 创建售后记录
        /// </summary>
        /// <param name="recordDto"></param>
        /// <returns></returns>
        Task<ApiResponse<AfterSalesRecordDto>> CreateAfterSalesRecordAsync(AfterSalesRecordUpsertDto recordDto);
        
        /// <summary>
        /// 更新售后记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="recordDto"></param>
        /// <returns></returns>
        Task<ApiResponse> UpdateAfterSalesRecordAsync(int id, AfterSalesRecordUpsertDto recordDto);
        
        /// <summary>
        /// 删除售后记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ApiResponse> DeleteAfterSalesRecordAsync(int id);
        
        /// <summary>
        /// 搜索售后记录
        /// </summary>
        /// <param name="customerName">客户名称</param>
        /// <param name="province">省份</param>
        /// <param name="city">城市</param>
        /// <param name="status">处理状态</param>
        /// <returns></returns>
        Task<ApiResponse<List<AfterSalesRecordDto>>> SearchAfterSalesRecordsAsync(
            string? customerName = null, 
            string? province = null, 
            string? city = null, 
            string? status = null);
    }
}
