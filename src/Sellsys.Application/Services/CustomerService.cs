using Microsoft.EntityFrameworkCore;
using Sellsys.Application.DTOs.Customers;
using Sellsys.Application.Interfaces;
using Sellsys.Application.Common;
using Sellsys.CrossCutting.Common;
using Sellsys.Domain.Entities;
using Sellsys.Domain.Common;
using Sellsys.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sellsys.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly SellsysDbContext _context;

        public CustomerService(SellsysDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerUpsertDto customerDto)
        {
            // 验证至少有一个联系人
            if (customerDto.Contacts == null || !customerDto.Contacts.Any())
            {
                return new ApiResponse<CustomerDto> { IsSuccess = false, Message = "请至少添加一位联系人", StatusCode = HttpStatusCode.BadRequest };
            }

            var customer = new Customer
            {
                Name = customerDto.Name,
                Province = customerDto.Province,
                City = customerDto.City,
                Address = customerDto.Address,
                Remarks = customerDto.Remarks,
                IndustryTypes = customerDto.IndustryTypes,
                SalesPersonId = customerDto.SalesPersonId,
                SupportPersonId = customerDto.SupportPersonId,
                CreatedAt = TimeHelper.GetBeijingTime()
            };

            // 添加联系人
            foreach (var contactDto in customerDto.Contacts)
            {
                customer.Contacts.Add(new Contact
                {
                    Name = contactDto.Name,
                    Phone = contactDto.Phone,
                    IsPrimary = contactDto.IsPrimary
                });
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // 重新查询以获取关联数据
            var createdCustomer = await _context.Customers
                .Include(c => c.Contacts)
                .Include(c => c.SalesPerson)
                .Include(c => c.SupportPerson)
                .FirstOrDefaultAsync(c => c.Id == customer.Id);

            var resultDto = new CustomerDto
            {
                Id = createdCustomer!.Id,
                Name = createdCustomer.Name,
                Province = createdCustomer.Province,
                City = createdCustomer.City,
                Address = createdCustomer.Address,
                Remarks = createdCustomer.Remarks,
                IndustryTypes = createdCustomer.IndustryTypes,
                SalesPersonId = createdCustomer.SalesPersonId,
                SalesPersonName = createdCustomer.SalesPerson?.Name,
                SupportPersonId = createdCustomer.SupportPersonId,
                SupportPersonName = createdCustomer.SupportPerson?.Name,
                CreatedAt = createdCustomer.CreatedAt,
                Contacts = createdCustomer.Contacts.Select(c => new ContactDto
                {
                    Id = c.Id,
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Phone = c.Phone,
                    IsPrimary = c.IsPrimary,
                    CreatedAt = c.CreatedAt
                }).ToList()
            };

            return ApiResponse<CustomerDto>.Success(resultDto);
        }

        public async Task<ApiResponse> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return ApiResponse.Fail("Customer not found.", HttpStatusCode.NotFound);
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return ApiResponse.Success();
        }

        public async Task<ApiResponse<List<CustomerDto>>> GetAllCustomersAsync()
        {
            var customers = await _context.Customers
                .Include(c => c.Contacts)
                .Include(c => c.SalesPerson)
                .Include(c => c.SupportPerson)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Province = c.Province,
                    City = c.City,
                    Address = c.Address,
                    Remarks = c.Remarks,
                    IndustryTypes = c.IndustryTypes,
                    SalesPersonId = c.SalesPersonId,
                    SalesPersonName = c.SalesPerson != null ? c.SalesPerson.Name : null,
                    SupportPersonId = c.SupportPersonId,
                    SupportPersonName = c.SupportPerson != null ? c.SupportPerson.Name : null,
                    CreatedAt = c.CreatedAt,
                    Contacts = c.Contacts.Select(contact => new ContactDto
                    {
                        Id = contact.Id,
                        CustomerId = contact.CustomerId,
                        Name = contact.Name,
                        Phone = contact.Phone,
                        IsPrimary = contact.IsPrimary,
                        CreatedAt = contact.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            return ApiResponse<List<CustomerDto>>.Success(customers);
        }

        /// <summary>
        /// 根据用户权限获取客户列表
        /// </summary>
        /// <param name="userId">当前用户ID，如果为null则返回所有客户（管理员权限）</param>
        /// <returns>过滤后的客户列表</returns>
        public async Task<ApiResponse<List<CustomerDto>>> GetCustomersWithPermissionAsync(int? userId = null)
        {
            // 如果没有传递用户ID，返回所有客户（管理员权限）
            if (!userId.HasValue)
            {
                return await GetAllCustomersAsync();
            }

            // 获取当前用户信息
            var currentUser = await _context.Employees
                .Include(e => e.Group)
                    .ThenInclude(g => g!.Department)
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Id == userId.Value);

            if (currentUser == null)
            {
                return ApiResponse<List<CustomerDto>>.Fail("用户不存在", System.Net.HttpStatusCode.Unauthorized);
            }

            // 构建基础查询
            var query = _context.Customers
                .Include(c => c.Contacts)
                .Include(c => c.SalesPerson)
                    .ThenInclude(sp => sp!.Group)
                .Include(c => c.SupportPerson)
                    .ThenInclude(sp => sp!.Group)
                .AsQueryable();

            // 根据用户部门和角色进行权限过滤
            var departmentName = currentUser.Group?.Department?.Name;
            var roleLevel = GetRoleLevel(currentUser.Role?.Name);

            if (departmentName == "销售部")
            {
                query = ApplySalesPermissionFilter(query, currentUser, roleLevel);
            }
            else if (departmentName == "客服部")
            {
                query = ApplySupportPermissionFilter(query, currentUser, roleLevel);
            }
            else
            {
                // 其他部门暂时不显示客户数据
                query = query.Where(c => false);
            }

            var customers = await query
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Province = c.Province,
                    City = c.City,
                    Address = c.Address,
                    Remarks = c.Remarks,
                    IndustryTypes = c.IndustryTypes,
                    SalesPersonId = c.SalesPersonId,
                    SalesPersonName = c.SalesPerson != null ? c.SalesPerson.Name : null,
                    SupportPersonId = c.SupportPersonId,
                    SupportPersonName = c.SupportPerson != null ? c.SupportPerson.Name : null,
                    CreatedAt = c.CreatedAt,
                    Contacts = c.Contacts.Select(contact => new ContactDto
                    {
                        Id = contact.Id,
                        CustomerId = contact.CustomerId,
                        Name = contact.Name,
                        Phone = contact.Phone,
                        IsPrimary = contact.IsPrimary,
                        CreatedAt = contact.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            return ApiResponse<List<CustomerDto>>.Success(customers);
        }

        /// <summary>
        /// 获取角色级别
        /// </summary>
        private RoleLevel GetRoleLevel(string? roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return RoleLevel.Staff;

            if (roleName.Contains("经理"))
                return RoleLevel.Manager;
            else if (roleName.Contains("主管"))
                return RoleLevel.Supervisor;
            else
                return RoleLevel.Staff;
        }

        /// <summary>
        /// 应用销售部门权限过滤
        /// </summary>
        private IQueryable<Customer> ApplySalesPermissionFilter(IQueryable<Customer> query, Employee currentUser, RoleLevel roleLevel)
        {
            if (roleLevel == RoleLevel.Staff)
            {
                // 普通销售：只能看到分配给自己的客户
                return query.Where(c => c.SalesPersonId == currentUser.Id);
            }
            else if (roleLevel == RoleLevel.Supervisor || roleLevel == RoleLevel.Manager)
            {
                // 销售主管/经理：可以看到同组销售的客户 + 未分配销售的客户
                return query.Where(c =>
                    c.SalesPersonId == null || // 未分配销售的客户
                    (c.SalesPerson != null && c.SalesPerson.GroupId == currentUser.GroupId) // 同组销售的客户
                );
            }

            return query.Where(c => false);
        }

        /// <summary>
        /// 应用客服部门权限过滤
        /// </summary>
        private IQueryable<Customer> ApplySupportPermissionFilter(IQueryable<Customer> query, Employee currentUser, RoleLevel roleLevel)
        {
            if (roleLevel == RoleLevel.Staff)
            {
                // 普通客服：只能看到分配给自己的客户
                return query.Where(c => c.SupportPersonId == currentUser.Id);
            }
            else if (roleLevel == RoleLevel.Supervisor || roleLevel == RoleLevel.Manager)
            {
                // 客服主管/经理：可以看到同组客服的客户 + 未分配客服的客户
                return query.Where(c =>
                    c.SupportPersonId == null || // 未分配客服的客户
                    (c.SupportPerson != null && c.SupportPerson.GroupId == currentUser.GroupId) // 同组客服的客户
                );
            }

            return query.Where(c => false);
        }

        public async Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Contacts)
                .Include(c => c.SalesPerson)
                .Include(c => c.SupportPerson)
                .Where(c => c.Id == id)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Province = c.Province,
                    City = c.City,
                    Address = c.Address,
                    Remarks = c.Remarks,
                    IndustryTypes = c.IndustryTypes,
                    SalesPersonId = c.SalesPersonId,
                    SalesPersonName = c.SalesPerson != null ? c.SalesPerson.Name : null,
                    SupportPersonId = c.SupportPersonId,
                    SupportPersonName = c.SupportPerson != null ? c.SupportPerson.Name : null,
                    CreatedAt = c.CreatedAt,
                    Contacts = c.Contacts.Select(contact => new ContactDto
                    {
                        Id = contact.Id,
                        CustomerId = contact.CustomerId,
                        Name = contact.Name,
                        Phone = contact.Phone,
                        IsPrimary = contact.IsPrimary,
                        CreatedAt = contact.CreatedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return new ApiResponse<CustomerDto> { IsSuccess = false, Message = "Customer not found.", StatusCode = HttpStatusCode.NotFound };
            }

            return ApiResponse<CustomerDto>.Success(customer);
        }

        public async Task<ApiResponse> UpdateCustomerAsync(int id, CustomerUpsertDto customerDto)
        {
            var customer = await _context.Customers
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return ApiResponse.Fail("Customer not found.", HttpStatusCode.NotFound);
            }

            // 验证至少有一个联系人
            if (customerDto.Contacts == null || !customerDto.Contacts.Any())
            {
                return ApiResponse.Fail("请至少添加一位联系人", HttpStatusCode.BadRequest);
            }

            // 更新客户基本信息
            customer.Name = customerDto.Name;
            customer.Province = customerDto.Province;
            customer.City = customerDto.City;
            customer.Address = customerDto.Address;
            customer.Remarks = customerDto.Remarks;
            customer.IndustryTypes = customerDto.IndustryTypes;
            customer.SalesPersonId = customerDto.SalesPersonId;
            customer.SupportPersonId = customerDto.SupportPersonId;

            // 更新联系人
            // 删除不在更新列表中的联系人
            var contactsToRemove = customer.Contacts
                .Where(c => !customerDto.Contacts.Any(dto => dto.Id == c.Id))
                .ToList();
            foreach (var contact in contactsToRemove)
            {
                _context.Contacts.Remove(contact);
            }

            // 更新或添加联系人
            foreach (var contactDto in customerDto.Contacts)
            {
                if (contactDto.Id.HasValue)
                {
                    // 更新现有联系人
                    var existingContact = customer.Contacts.FirstOrDefault(c => c.Id == contactDto.Id.Value);
                    if (existingContact != null)
                    {
                        existingContact.Name = contactDto.Name;
                        existingContact.Phone = contactDto.Phone;
                        existingContact.IsPrimary = contactDto.IsPrimary;
                    }
                }
                else
                {
                    // 添加新联系人
                    customer.Contacts.Add(new Contact
                    {
                        Name = contactDto.Name,
                        Phone = contactDto.Phone,
                        IsPrimary = contactDto.IsPrimary
                    });
                }
            }

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return ApiResponse.Success();
        }
    }
}