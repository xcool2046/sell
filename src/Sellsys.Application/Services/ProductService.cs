using Microsoft.EntityFrameworkCore;
using Sellsys.Application.DTOs.Products;
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
    public class ProductService : IProductService
    {
        private readonly SellsysDbContext _context;

        public ProductService(SellsysDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<ProductDto>> CreateProductAsync(ProductUpsertDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Specification = productDto.Specification,
                Unit = productDto.Unit,
                ListPrice = productDto.ListPrice,
                MinPrice = productDto.MinPrice,
                SalesCommission = productDto.SalesCommission,
                SupervisorCommission = productDto.SupervisorCommission,
                ManagerCommission = productDto.ManagerCommission,
                CreatedAt = TimeHelper.GetBeijingTime(),
                UpdatedAt = TimeHelper.GetBeijingTime()
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var resultDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Specification = product.Specification,
                Unit = product.Unit,
                ListPrice = product.ListPrice,
                MinPrice = product.MinPrice,
                SalesCommission = product.SalesCommission,
                SupervisorCommission = product.SupervisorCommission,
                ManagerCommission = product.ManagerCommission,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            return ApiResponse<ProductDto>.Success(resultDto);
        }

        public async Task<ApiResponse> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return ApiResponse.Fail("Product not found.", HttpStatusCode.NotFound);
            }

            // Check if product is referenced by any order items
            var hasOrderItems = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
            if (hasOrderItems)
            {
                return ApiResponse.Fail("Cannot delete product because it is referenced by existing orders.", HttpStatusCode.BadRequest);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return ApiResponse.Success();
        }

        public async Task<ApiResponse<List<ProductDto>>> GetAllProductsAsync()
        {
            var products = await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Specification = p.Specification,
                    Unit = p.Unit,
                    ListPrice = p.ListPrice,
                    MinPrice = p.MinPrice,
                    SalesCommission = p.SalesCommission,
                    SupervisorCommission = p.SupervisorCommission,
                    ManagerCommission = p.ManagerCommission,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return ApiResponse<List<ProductDto>>.Success(products);
        }

        public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Specification = p.Specification,
                    Unit = p.Unit,
                    ListPrice = p.ListPrice,
                    MinPrice = p.MinPrice,
                    SalesCommission = p.SalesCommission,
                    SupervisorCommission = p.SupervisorCommission,
                    ManagerCommission = p.ManagerCommission,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return new ApiResponse<ProductDto> { IsSuccess = false, Message = "Product not found.", StatusCode = HttpStatusCode.NotFound };
            }

            return ApiResponse<ProductDto>.Success(product);
        }

        public async Task<ApiResponse> UpdateProductAsync(int id, ProductUpsertDto productDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return ApiResponse.Fail("Product not found.", HttpStatusCode.NotFound);
            }

            product.Name = productDto.Name;
            product.Specification = productDto.Specification;
            product.Unit = productDto.Unit;
            product.ListPrice = productDto.ListPrice;
            product.MinPrice = productDto.MinPrice;
            product.SalesCommission = productDto.SalesCommission;
            product.SupervisorCommission = productDto.SupervisorCommission;
            product.ManagerCommission = productDto.ManagerCommission;
            product.UpdatedAt = TimeHelper.GetBeijingTime();

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return ApiResponse.Success();
        }
    }
}