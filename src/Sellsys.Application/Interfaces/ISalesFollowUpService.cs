using Sellsys.Application.DTOs.SalesFollowUp;
using Sellsys.CrossCutting.Common;

namespace Sellsys.Application.Interfaces
{
    public interface ISalesFollowUpService
    {
        /// <summary>
        /// 获取所有销售跟进记录
        /// </summary>
        /// <returns></returns>
        Task<ApiResponse<List<SalesFollowUpLogDto>>> GetAllSalesFollowUpLogsAsync();

        /// <summary>
        /// 根据ID获取销售跟进记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ApiResponse<SalesFollowUpLogDto>> GetSalesFollowUpLogByIdAsync(int id);

        /// <summary>
        /// 根据客户ID获取销售跟进记录列表
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        Task<ApiResponse<List<SalesFollowUpLogDto>>> GetSalesFollowUpLogsByCustomerIdAsync(int customerId);

        /// <summary>
        /// 创建销售跟进记录
        /// </summary>
        /// <param name="logDto"></param>
        /// <returns></returns>
        Task<ApiResponse<SalesFollowUpLogDto>> CreateSalesFollowUpLogAsync(SalesFollowUpLogUpsertDto logDto);

        /// <summary>
        /// 更新销售跟进记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="logDto"></param>
        /// <returns></returns>
        Task<ApiResponse> UpdateSalesFollowUpLogAsync(int id, SalesFollowUpLogUpsertDto logDto);

        /// <summary>
        /// 删除销售跟进记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ApiResponse> DeleteSalesFollowUpLogAsync(int id);
    }
}
