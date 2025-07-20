using Microsoft.AspNetCore.Mvc;
using Sellsys.Application.DTOs.Products;
using Sellsys.Application.Interfaces;
using System.Threading.Tasks;

namespace Sellsys.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var response = await _productService.GetAllProductsAsync();
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var response = await _productService.GetProductByIdAsync(id);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductUpsertDto productDto)
        {
            var response = await _productService.CreateProductAsync(productDto);
            if (response.IsSuccess && response.Data != null)
            {
                return CreatedAtAction(nameof(GetProductById), new { id = response.Data.Id }, response);
            }
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpsertDto productDto)
        {
            var response = await _productService.UpdateProductAsync(id, productDto);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var response = await _productService.DeleteProductAsync(id);
            return new ObjectResult(response) { StatusCode = (int)response.StatusCode };
        }
    }
}