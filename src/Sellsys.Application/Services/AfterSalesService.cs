using Microsoft.EntityFrameworkCore;
using Sellsys.Application.DTOs.AfterSales;
using Sellsys.Application.Interfaces;
using Sellsys.CrossCutting.Common;
using Sellsys.Domain.Entities;
using Sellsys.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sellsys.Application.Services
{
    public class AfterSalesService : IAfterSalesService
    {
        private readonly SellsysDbContext _context;

        public AfterSalesService(SellsysDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<AfterSalesRecordDto>> CreateAfterSalesRecordAsync(AfterSalesRecordUpsertDto recordDto)
        {
            // 验证客户是否存在
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == recordDto.CustomerId);
            if (!customerExists)
            {
                return new ApiResponse<AfterSalesRecordDto> 
                { 
                    IsSuccess = false, 
                    Message = "指定的客户不存在", 
                    StatusCode = HttpStatusCode.BadRequest 
                };
            }

            // 如果指定了联系人，验证联系人是否属于该客户
            if (recordDto.ContactId.HasValue)
            {
                var contactExists = await _context.Contacts
                    .AnyAsync(c => c.Id == recordDto.ContactId.Value && c.CustomerId == recordDto.CustomerId);
                if (!contactExists)
                {
                    return new ApiResponse<AfterSalesRecordDto> 
                    { 
                        IsSuccess = false, 
                        Message = "指定的联系人不属于该客户", 
                        StatusCode = HttpStatusCode.BadRequest 
                    };
                }
            }

            // 如果指定了客服人员，验证客服人员是否存在
            if (recordDto.SupportPersonId.HasValue)
            {
                var supportPersonExists = await _context.Employees.AnyAsync(e => e.Id == recordDto.SupportPersonId.Value);
                if (!supportPersonExists)
                {
                    return new ApiResponse<AfterSalesRecordDto> 
                    { 
                        IsSuccess = false, 
                        Message = "指定的客服人员不存在", 
                        StatusCode = HttpStatusCode.BadRequest 
                    };
                }
            }

            var record = new AfterSalesRecord
            {
                CustomerId = recordDto.CustomerId,
                ContactId = recordDto.ContactId,
                CustomerFeedback = recordDto.CustomerFeedback,
                OurReply = recordDto.OurReply,
                Status = recordDto.Status,
                ProcessedDate = recordDto.ProcessedDate,
                SupportPersonId = recordDto.SupportPersonId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.AfterSalesRecords.Add(record);
            await _context.SaveChangesAsync();

            // 重新查询以获取关联数据
            var createdRecord = await _context.AfterSalesRecords
                .Include(r => r.Customer)
                .Include(r => r.Contact)
                .Include(r => r.SupportPerson)
                .FirstOrDefaultAsync(r => r.Id == record.Id);

            var resultDto = MapToDto(createdRecord!);

            return new ApiResponse<AfterSalesRecordDto> 
            { 
                IsSuccess = true, 
                Data = resultDto, 
                Message = "售后记录创建成功" 
            };
        }

        public async Task<ApiResponse> DeleteAfterSalesRecordAsync(int id)
        {
            var record = await _context.AfterSalesRecords.FindAsync(id);
            if (record == null)
            {
                return new ApiResponse 
                { 
                    IsSuccess = false, 
                    Message = "售后记录不存在", 
                    StatusCode = HttpStatusCode.NotFound 
                };
            }

            _context.AfterSalesRecords.Remove(record);
            await _context.SaveChangesAsync();

            return new ApiResponse 
            { 
                IsSuccess = true, 
                Message = "售后记录删除成功" 
            };
        }

        public async Task<ApiResponse<List<AfterSalesRecordDto>>> GetAllAfterSalesRecordsAsync()
        {
            var records = await _context.AfterSalesRecords
                .Include(r => r.Customer)
                .Include(r => r.Contact)
                .Include(r => r.SupportPerson)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => MapToDto(r))
                .ToListAsync();

            return new ApiResponse<List<AfterSalesRecordDto>> 
            { 
                IsSuccess = true, 
                Data = records 
            };
        }

        public async Task<ApiResponse<AfterSalesRecordDto>> GetAfterSalesRecordByIdAsync(int id)
        {
            var record = await _context.AfterSalesRecords
                .Include(r => r.Customer)
                .Include(r => r.Contact)
                .Include(r => r.SupportPerson)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (record == null)
            {
                return new ApiResponse<AfterSalesRecordDto> 
                { 
                    IsSuccess = false, 
                    Message = "售后记录不存在", 
                    StatusCode = HttpStatusCode.NotFound 
                };
            }

            var dto = MapToDto(record);

            return new ApiResponse<AfterSalesRecordDto> 
            { 
                IsSuccess = true, 
                Data = dto 
            };
        }

        public async Task<ApiResponse<List<AfterSalesRecordDto>>> GetAfterSalesRecordsByCustomerIdAsync(int customerId)
        {
            var records = await _context.AfterSalesRecords
                .Include(r => r.Customer)
                .Include(r => r.Contact)
                .Include(r => r.SupportPerson)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => MapToDto(r))
                .ToListAsync();

            return new ApiResponse<List<AfterSalesRecordDto>> 
            { 
                IsSuccess = true, 
                Data = records 
            };
        }

        public async Task<ApiResponse<List<AfterSalesRecordDto>>> SearchAfterSalesRecordsAsync(
            string? customerName = null, 
            string? province = null, 
            string? city = null, 
            string? status = null)
        {
            var query = _context.AfterSalesRecords
                .Include(r => r.Customer)
                .Include(r => r.Contact)
                .Include(r => r.SupportPerson)
                .AsQueryable();

            // 按客户名称搜索
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                query = query.Where(r => r.Customer.Name.Contains(customerName));
            }

            // 按省份搜索
            if (!string.IsNullOrWhiteSpace(province))
            {
                query = query.Where(r => r.Customer.Province == province);
            }

            // 按城市搜索
            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(r => r.Customer.City == city);
            }

            // 按状态搜索
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var records = await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => MapToDto(r))
                .ToListAsync();

            return new ApiResponse<List<AfterSalesRecordDto>> 
            { 
                IsSuccess = true, 
                Data = records 
            };
        }

        public async Task<ApiResponse> UpdateAfterSalesRecordAsync(int id, AfterSalesRecordUpsertDto recordDto)
        {
            var record = await _context.AfterSalesRecords.FindAsync(id);
            if (record == null)
            {
                return new ApiResponse 
                { 
                    IsSuccess = false, 
                    Message = "售后记录不存在", 
                    StatusCode = HttpStatusCode.NotFound 
                };
            }

            // 验证客户是否存在
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == recordDto.CustomerId);
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
            if (recordDto.ContactId.HasValue)
            {
                var contactExists = await _context.Contacts
                    .AnyAsync(c => c.Id == recordDto.ContactId.Value && c.CustomerId == recordDto.CustomerId);
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

            // 如果指定了客服人员，验证客服人员是否存在
            if (recordDto.SupportPersonId.HasValue)
            {
                var supportPersonExists = await _context.Employees.AnyAsync(e => e.Id == recordDto.SupportPersonId.Value);
                if (!supportPersonExists)
                {
                    return new ApiResponse 
                    { 
                        IsSuccess = false, 
                        Message = "指定的客服人员不存在", 
                        StatusCode = HttpStatusCode.BadRequest 
                    };
                }
            }

            // 更新记录
            record.CustomerId = recordDto.CustomerId;
            record.ContactId = recordDto.ContactId;
            record.CustomerFeedback = recordDto.CustomerFeedback;
            record.OurReply = recordDto.OurReply;
            record.Status = recordDto.Status;
            record.ProcessedDate = recordDto.ProcessedDate;
            record.SupportPersonId = recordDto.SupportPersonId;
            record.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse 
            { 
                IsSuccess = true, 
                Message = "售后记录更新成功" 
            };
        }

        private static AfterSalesRecordDto MapToDto(AfterSalesRecord record)
        {
            return new AfterSalesRecordDto
            {
                Id = record.Id,
                CustomerId = record.CustomerId,
                CustomerName = record.Customer?.Name ?? string.Empty,
                CustomerProvince = record.Customer?.Province,
                CustomerCity = record.Customer?.City,
                ContactId = record.ContactId,
                ContactName = record.Contact?.Name,
                ContactPhone = record.Contact?.Phone,
                CustomerFeedback = record.CustomerFeedback,
                OurReply = record.OurReply,
                Status = record.Status,
                ProcessedDate = record.ProcessedDate,
                SupportPersonId = record.SupportPersonId,
                SupportPersonName = record.SupportPerson?.Name,
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt
            };
        }
    }
}
