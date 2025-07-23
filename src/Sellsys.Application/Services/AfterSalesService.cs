using Microsoft.EntityFrameworkCore;
using Sellsys.Application.DTOs.AfterSales;
using Sellsys.Application.Interfaces;
using Sellsys.CrossCutting.Common;
using Sellsys.Domain.Entities;
using Sellsys.Domain.Common;
using Sellsys.Infrastructure.Data;
using System;
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

        public async Task<ApiResponse<List<CustomerAfterSalesDto>>> GetCustomersWithAfterSalesInfoAsync()
        {
            try
            {
                var customers = await _context.Customers
                    .Include(c => c.Contacts)
                    .Include(c => c.SalesPerson)
                    .Include(c => c.SupportPerson)
                    .Include(c => c.AfterSalesRecords)
                    .Select(c => new CustomerAfterSalesDto
                    {
                        CustomerId = c.Id,
                        CustomerName = c.Name,
                        Province = c.Province,
                        City = c.City,
                        ContactName = c.Contacts.FirstOrDefault(contact => contact.IsPrimary) != null
                            ? c.Contacts.FirstOrDefault(contact => contact.IsPrimary)!.Name
                            : c.Contacts.FirstOrDefault() != null
                                ? c.Contacts.FirstOrDefault()!.Name
                                : null,
                        ContactPhone = c.Contacts.FirstOrDefault(contact => contact.IsPrimary) != null
                            ? c.Contacts.FirstOrDefault(contact => contact.IsPrimary)!.Phone
                            : c.Contacts.FirstOrDefault() != null
                                ? c.Contacts.FirstOrDefault()!.Phone
                                : null,
                        ContactCount = c.Contacts.Count,
                        SalesPersonId = c.SalesPersonId,
                        SalesPersonName = c.SalesPerson != null ? c.SalesPerson.Name : null,
                        SupportPersonId = c.SupportPersonId,
                        SupportPersonName = c.SupportPerson != null ? c.SupportPerson.Name : null,
                        ServiceRecordCount = c.AfterSalesRecords.Count,
                        UpdatedAt = c.AfterSalesRecords.Any()
                            ? c.AfterSalesRecords.Max(r => r.UpdatedAt)
                            : c.CreatedAt,
                        CreatedAt = c.CreatedAt,
                        LatestRecordStatus = c.AfterSalesRecords.Any()
                            ? c.AfterSalesRecords.OrderByDescending(r => r.UpdatedAt).First().Status
                            : null,
                        PendingRecordCount = c.AfterSalesRecords.Count(r => r.Status == "待处理"),
                        ProcessingRecordCount = c.AfterSalesRecords.Count(r => r.Status == "处理中"),
                        CompletedRecordCount = c.AfterSalesRecords.Count(r => r.Status == "处理完成")
                    })
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToListAsync();

                return new ApiResponse<List<CustomerAfterSalesDto>>
                {
                    IsSuccess = true,
                    Data = customers,
                    Message = "获取客户售后信息成功"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<CustomerAfterSalesDto>>
                {
                    IsSuccess = false,
                    Message = $"获取客户售后信息失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task<ApiResponse<List<CustomerAfterSalesDto>>> SearchCustomersWithAfterSalesInfoAsync(
            string? customerName = null,
            string? supportPersonName = null,
            string? status = null)
        {
            try
            {
                var query = _context.Customers
                    .Include(c => c.Contacts)
                    .Include(c => c.SalesPerson)
                    .Include(c => c.SupportPerson)
                    .Include(c => c.AfterSalesRecords)
                    .AsQueryable();

                // 按客户名称筛选
                if (!string.IsNullOrWhiteSpace(customerName))
                {
                    query = query.Where(c => c.Name.Contains(customerName));
                }

                // 按客服人员筛选
                if (!string.IsNullOrWhiteSpace(supportPersonName) && supportPersonName != "全部")
                {
                    query = query.Where(c => c.SupportPerson != null && c.SupportPerson.Name.Contains(supportPersonName));
                }

                // 按状态筛选
                if (!string.IsNullOrWhiteSpace(status) && status != "全部")
                {
                    switch (status)
                    {
                        case "待处理":
                            query = query.Where(c => c.AfterSalesRecords.Any(r => r.Status == "待处理"));
                            break;
                        case "处理中":
                            query = query.Where(c => c.AfterSalesRecords.Any(r => r.Status == "处理中"));
                            break;
                        case "处理完成":
                            query = query.Where(c => c.AfterSalesRecords.Any(r => r.Status == "处理完成"));
                            break;
                    }
                }

                var customers = await query
                    .Select(c => new CustomerAfterSalesDto
                    {
                        CustomerId = c.Id,
                        CustomerName = c.Name,
                        Province = c.Province,
                        City = c.City,
                        ContactName = c.Contacts.FirstOrDefault(contact => contact.IsPrimary) != null
                            ? c.Contacts.FirstOrDefault(contact => contact.IsPrimary)!.Name
                            : c.Contacts.FirstOrDefault() != null
                                ? c.Contacts.FirstOrDefault()!.Name
                                : null,
                        ContactPhone = c.Contacts.FirstOrDefault(contact => contact.IsPrimary) != null
                            ? c.Contacts.FirstOrDefault(contact => contact.IsPrimary)!.Phone
                            : c.Contacts.FirstOrDefault() != null
                                ? c.Contacts.FirstOrDefault()!.Phone
                                : null,
                        ContactCount = c.Contacts.Count,
                        SalesPersonId = c.SalesPersonId,
                        SalesPersonName = c.SalesPerson != null ? c.SalesPerson.Name : null,
                        SupportPersonId = c.SupportPersonId,
                        SupportPersonName = c.SupportPerson != null ? c.SupportPerson.Name : null,
                        ServiceRecordCount = c.AfterSalesRecords.Count,
                        UpdatedAt = c.AfterSalesRecords.Any()
                            ? c.AfterSalesRecords.Max(r => r.UpdatedAt)
                            : c.CreatedAt,
                        CreatedAt = c.CreatedAt,
                        LatestRecordStatus = c.AfterSalesRecords.Any()
                            ? c.AfterSalesRecords.OrderByDescending(r => r.UpdatedAt).First().Status
                            : null,
                        PendingRecordCount = c.AfterSalesRecords.Count(r => r.Status == "待处理"),
                        ProcessingRecordCount = c.AfterSalesRecords.Count(r => r.Status == "处理中"),
                        CompletedRecordCount = c.AfterSalesRecords.Count(r => r.Status == "处理完成")
                    })
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToListAsync();

                return new ApiResponse<List<CustomerAfterSalesDto>>
                {
                    IsSuccess = true,
                    Data = customers,
                    Message = "搜索客户售后信息成功"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<CustomerAfterSalesDto>>
                {
                    IsSuccess = false,
                    Message = $"搜索客户售后信息失败: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
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
                CreatedAt = TimeHelper.GetBeijingTime(),
                UpdatedAt = TimeHelper.GetBeijingTime()
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
            record.UpdatedAt = TimeHelper.GetBeijingTime();

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
