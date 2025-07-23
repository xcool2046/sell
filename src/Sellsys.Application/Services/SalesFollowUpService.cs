using Microsoft.EntityFrameworkCore;
using Sellsys.Application.DTOs.SalesFollowUp;
using Sellsys.Application.Interfaces;
using Sellsys.CrossCutting.Common;
using Sellsys.Domain.Entities;
using Sellsys.Infrastructure.Data;
using System.Net;

namespace Sellsys.Application.Services
{
    public class SalesFollowUpService : ISalesFollowUpService
    {
        private readonly SellsysDbContext _context;

        public SalesFollowUpService(SellsysDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<SalesFollowUpLogDto>>> GetAllSalesFollowUpLogsAsync()
        {
            try
            {
                var logs = await _context.SalesFollowUpLogs
                    .Include(s => s.Customer)
                    .Include(s => s.Contact)
                    .Include(s => s.SalesPerson)
                    .OrderByDescending(s => s.CreatedAt)
                    .Select(s => MapToDto(s))
                    .ToListAsync();

                return new ApiResponse<List<SalesFollowUpLogDto>>
                {
                    IsSuccess = true,
                    Data = logs,
                    Message = "获取销售跟进记录成功"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<SalesFollowUpLogDto>>
                {
                    IsSuccess = false,
                    Message = $"获取销售跟进记录失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task<ApiResponse<SalesFollowUpLogDto>> GetSalesFollowUpLogByIdAsync(int id)
        {
            try
            {
                var log = await _context.SalesFollowUpLogs
                    .Include(s => s.Customer)
                    .Include(s => s.Contact)
                    .Include(s => s.SalesPerson)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (log == null)
                {
                    return new ApiResponse<SalesFollowUpLogDto>
                    {
                        IsSuccess = false,
                        Message = "销售跟进记录不存在",
                        StatusCode = HttpStatusCode.NotFound
                    };
                }

                var dto = MapToDto(log);

                return new ApiResponse<SalesFollowUpLogDto>
                {
                    IsSuccess = true,
                    Data = dto,
                    Message = "获取销售跟进记录成功"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SalesFollowUpLogDto>
                {
                    IsSuccess = false,
                    Message = $"获取销售跟进记录失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task<ApiResponse<List<SalesFollowUpLogDto>>> GetSalesFollowUpLogsByCustomerIdAsync(int customerId)
        {
            try
            {
                var logs = await _context.SalesFollowUpLogs
                    .Include(s => s.Customer)
                    .Include(s => s.Contact)
                    .Include(s => s.SalesPerson)
                    .Where(s => s.CustomerId == customerId)
                    .OrderByDescending(s => s.CreatedAt)
                    .Select(s => MapToDto(s))
                    .ToListAsync();

                return new ApiResponse<List<SalesFollowUpLogDto>>
                {
                    IsSuccess = true,
                    Data = logs,
                    Message = "获取客户销售跟进记录成功"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<SalesFollowUpLogDto>>
                {
                    IsSuccess = false,
                    Message = $"获取客户销售跟进记录失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task<ApiResponse<SalesFollowUpLogDto>> CreateSalesFollowUpLogAsync(SalesFollowUpLogUpsertDto logDto)
        {
            try
            {
                // 验证客户是否存在
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == logDto.CustomerId);
                if (!customerExists)
                {
                    return new ApiResponse<SalesFollowUpLogDto>
                    {
                        IsSuccess = false,
                        Message = "指定的客户不存在",
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }

                // 如果指定了联系人，验证联系人是否属于该客户
                if (logDto.ContactId.HasValue)
                {
                    var contactExists = await _context.Contacts
                        .AnyAsync(c => c.Id == logDto.ContactId.Value && c.CustomerId == logDto.CustomerId);
                    if (!contactExists)
                    {
                        return new ApiResponse<SalesFollowUpLogDto>
                        {
                            IsSuccess = false,
                            Message = "指定的联系人不属于该客户",
                            StatusCode = HttpStatusCode.BadRequest
                        };
                    }
                }

                // 如果指定了销售人员，验证销售人员是否存在
                if (logDto.SalesPersonId.HasValue)
                {
                    var salesPersonExists = await _context.Employees.AnyAsync(e => e.Id == logDto.SalesPersonId.Value);
                    if (!salesPersonExists)
                    {
                        return new ApiResponse<SalesFollowUpLogDto>
                        {
                            IsSuccess = false,
                            Message = "指定的销售人员不存在",
                            StatusCode = HttpStatusCode.BadRequest
                        };
                    }
                }

                var log = new SalesFollowUpLog
                {
                    CustomerId = logDto.CustomerId,
                    ContactId = logDto.ContactId,
                    Summary = logDto.Summary,
                    CustomerIntention = logDto.CustomerIntention,
                    NextFollowUpDate = logDto.NextFollowUpDate,
                    SalesPersonId = logDto.SalesPersonId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SalesFollowUpLogs.Add(log);
                await _context.SaveChangesAsync();

                // 重新查询以获取关联数据
                var createdLog = await _context.SalesFollowUpLogs
                    .Include(s => s.Customer)
                    .Include(s => s.Contact)
                    .Include(s => s.SalesPerson)
                    .FirstOrDefaultAsync(s => s.Id == log.Id);

                var dto = MapToDto(createdLog!);

                return new ApiResponse<SalesFollowUpLogDto>
                {
                    IsSuccess = true,
                    Data = dto,
                    Message = "创建销售跟进记录成功"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SalesFollowUpLogDto>
                {
                    IsSuccess = false,
                    Message = $"创建销售跟进记录失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task<ApiResponse> UpdateSalesFollowUpLogAsync(int id, SalesFollowUpLogUpsertDto logDto)
        {
            try
            {
                var log = await _context.SalesFollowUpLogs.FindAsync(id);
                if (log == null)
                {
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = "销售跟进记录不存在",
                        StatusCode = HttpStatusCode.NotFound
                    };
                }

                // 验证客户是否存在
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == logDto.CustomerId);
                if (!customerExists)
                {
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = "指定的客户不存在",
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }

                // 如果指定了联系人，验证联系人是否属于该客户
                if (logDto.ContactId.HasValue)
                {
                    var contactExists = await _context.Contacts
                        .AnyAsync(c => c.Id == logDto.ContactId.Value && c.CustomerId == logDto.CustomerId);
                    if (!contactExists)
                    {
                        return new ApiResponse
                        {
                            IsSuccess = false,
                            Message = "指定的联系人不属于该客户",
                            StatusCode = HttpStatusCode.BadRequest
                        };
                    }
                }

                // 如果指定了销售人员，验证销售人员是否存在
                if (logDto.SalesPersonId.HasValue)
                {
                    var salesPersonExists = await _context.Employees.AnyAsync(e => e.Id == logDto.SalesPersonId.Value);
                    if (!salesPersonExists)
                    {
                        return new ApiResponse
                        {
                            IsSuccess = false,
                            Message = "指定的销售人员不存在",
                            StatusCode = HttpStatusCode.BadRequest
                        };
                    }
                }

                // 更新字段
                log.CustomerId = logDto.CustomerId;
                log.ContactId = logDto.ContactId;
                log.Summary = logDto.Summary;
                log.CustomerIntention = logDto.CustomerIntention;
                log.NextFollowUpDate = logDto.NextFollowUpDate;
                log.SalesPersonId = logDto.SalesPersonId;

                await _context.SaveChangesAsync();

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "更新销售跟进记录成功"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"更新销售跟进记录失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task<ApiResponse> DeleteSalesFollowUpLogAsync(int id)
        {
            try
            {
                var log = await _context.SalesFollowUpLogs.FindAsync(id);
                if (log == null)
                {
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = "销售跟进记录不存在",
                        StatusCode = HttpStatusCode.NotFound
                    };
                }

                _context.SalesFollowUpLogs.Remove(log);
                await _context.SaveChangesAsync();

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "删除销售跟进记录成功"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"删除销售跟进记录失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        private static SalesFollowUpLogDto MapToDto(SalesFollowUpLog log)
        {
            return new SalesFollowUpLogDto
            {
                Id = log.Id,
                CustomerId = log.CustomerId,
                CustomerName = log.Customer?.Name ?? string.Empty,
                ContactId = log.ContactId,
                ContactName = log.Contact?.Name,
                ContactPhone = log.Contact?.Phone,
                Summary = log.Summary,
                CustomerIntention = log.CustomerIntention,
                NextFollowUpDate = log.NextFollowUpDate,
                SalesPersonId = log.SalesPersonId,
                SalesPersonName = log.SalesPerson?.Name,
                CreatedAt = log.CreatedAt
            };
        }
    }
}
