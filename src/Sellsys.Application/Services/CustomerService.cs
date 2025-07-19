using Microsoft.EntityFrameworkCore;
using Sellsys.Application.DTOs.Customers;
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
    public class CustomerService : ICustomerService
    {
        private readonly SellsysDbContext _context;

        public CustomerService(SellsysDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerUpsertDto customerDto)
        {
            if (string.IsNullOrEmpty(customerDto.PhoneNumber))
            {
                return new ApiResponse<CustomerDto> { IsSuccess = false, Message = "Phone number is required.", StatusCode = HttpStatusCode.BadRequest };
            }

            var customer = new Customer
            {
                Name = customerDto.Name,
                ContactPerson = customerDto.ContactPerson,
                PhoneNumber = customerDto.PhoneNumber,
                Address = customerDto.Address
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var resultDto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                ContactPerson = customer.ContactPerson,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt
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
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ContactPerson = c.ContactPerson,
                    PhoneNumber = c.PhoneNumber,
                    Address = c.Address,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<CustomerDto>>.Success(customers);
        }

        public async Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(int id)
        {
            var customer = await _context.Customers
                .Where(c => c.Id == id)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ContactPerson = c.ContactPerson,
                    PhoneNumber = c.PhoneNumber,
                    Address = c.Address,
                    CreatedAt = c.CreatedAt
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
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return ApiResponse.Fail("Customer not found.", HttpStatusCode.NotFound);
            }

            if (string.IsNullOrEmpty(customerDto.PhoneNumber))
            {
                return ApiResponse.Fail("Phone number is required.", HttpStatusCode.BadRequest);
            }

            customer.Name = customerDto.Name;
            customer.ContactPerson = customerDto.ContactPerson;
            customer.PhoneNumber = customerDto.PhoneNumber;
            customer.Address = customerDto.Address;

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return ApiResponse.Success();
        }
    }
}