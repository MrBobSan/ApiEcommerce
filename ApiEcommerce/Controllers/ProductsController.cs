using ApiEcommerce.Repository.IRepository;
using ApiEcommerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ApiEcommerce.Models.Dtos;
using AutoMapper;

namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        public ProductsController(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productsDto = new List<ProductDto>();
            foreach(var product in products)
            {
                productsDto.Add(_mapper.Map<ProductDto>(product));
            }
            return Ok(productsDto);
        }
        [HttpGet("{ProductId:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProduct(int ProductId)
        {
            var product = _productRepository.GetProduct(ProductId);
            if (product == null)            
            {
                return NotFound($"Product with id {ProductId} not found.");
            }
            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (createProductDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_productRepository.ProductExists(createProductDto.Name))
            {
                ModelState.AddModelError("Custom Error","El producto ya existe");
                return BadRequest(ModelState);
            }
            var product = _mapper.Map<Product>(createProductDto);
            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("Custom Error", $"Algo salio mal al guardar el registro {product.Name}");
                return StatusCode(500, ModelState);
            }
            {
                ModelState.AddModelError("Custom Error", $"Algo salio mal al guardar el registro {product.Name}");
                return StatusCode(500, ModelState);
            }
            return CreatedAtRoute("GetProduct", new { ProductId = product.ProductId }, product);
        }
        [HttpPatch("{ProductId:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateProduct(int ProductId,[FromBody] CreateProductDto updateProductDto)
        {
            if(!_productRepository.ProductExists(ProductId))
            {
                return NotFound($"Product with id {ProductId} not found.");
            }
            if (updateProductDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_productRepository.ProductExists(updateProductDto.Name))
            {
                ModelState.AddModelError("Custom Error","El producto ya existe");
                return BadRequest(ModelState);
            }
            var product = _mapper.Map<Product>(updateProductDto);
            product.ProductId = ProductId;
            if (!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("Custom Error", $"Algo salio mal al actualizar el registro {product.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{ProductId:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteProduct(int ProductId)
        {
            if(!_productRepository.ProductExists(ProductId))
            {
                return NotFound($"Product with id {ProductId} not found.");
            }
            var product = _productRepository.GetProduct(ProductId);
            if (product == null)
            {
                return NotFound($"Product with id {ProductId} not found.");
            }
            if (!_productRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("Custom Error", $"Algo salio mal al eliminar el registro {product.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
    }
}
