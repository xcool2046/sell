using Sellsys.Application.DTOs.Products;
using Sellsys.CrossCutting.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sellsys.Application.Interfaces
{
    public interface IProductService
    {
        Task<ApiResponse<List<ProductDto>>> GetAllProductsAsync();
        Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id);
        Task<ApiResponse<ProductDto>> CreateProductAsync(ProductUpsertDto productDto);
        Task<ApiResponse> UpdateProductAsync(int id, ProductUpsertDto productDto);
        Task<ApiResponse> DeleteProductAsync(int id);
    }
}