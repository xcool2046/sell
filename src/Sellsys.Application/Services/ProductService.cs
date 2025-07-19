using Microsoft.EntityFrameworkCore;
using Sellsys.Application.DTOs.Products;
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
                Description = productDto.Description,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var resultDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CreatedAt = product.CreatedAt
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

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return ApiResponse.Success();
        }

        public async Task<ApiResponse<List<ProductDto>>> GetAllProductsAsync()
        {
            var products = await _context.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CreatedAt = p.CreatedAt
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
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CreatedAt = p.CreatedAt
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
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.StockQuantity = productDto.StockQuantity;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return ApiResponse.Success();
        }
    }
}