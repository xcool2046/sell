using Microsoft.EntityFrameworkCore;
using Sellsys.Application.DTOs.Customers;
using Sellsys.Application.Interfaces;
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