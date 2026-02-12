using ApiEcommerce.Repository.IRepository;
using ApiEcommerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ApiEcommerce.Models.Dtos;
using AutoMapper;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
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
        [HttpGet("{productId:int}", Name = "GetProduct")]
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
            if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
            {
                ModelState.AddModelError("Custom Error",$"La categoría {createProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }
            var product = _mapper.Map<Product>(createProductDto);
            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("Custom Error", $"Algo salio mal al guardar el registro {product.Name}");
                return StatusCode(500, ModelState);
            }
            var createdProduct = _productRepository.GetProduct(product.ProductId);
            var productDto = _mapper.Map<ProductDto>(createdProduct);
            return CreatedAtRoute("GetProduct", new { productId = product.ProductId }, productDto);
        }
        [HttpGet("searchByCategory/{categoryId:int}", Name = "GetProductsForCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsForCategory(int categoryId)
        {
            var products = _productRepository.GetProductsForCategory(categoryId);
            if (products.Count == 0)            
            {
                return NotFound($"Product with category {categoryId} does not exist.");
            }
            var productsDto = _mapper.Map<List<ProductDto>>(products);
            return Ok(productsDto);
        }

        [HttpPatch("buyProduct/{name}/{quantity:int}", Name = "BuyProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult BuyProduct(string name, int quantity)
        {
            if(string.IsNullOrEmpty(name) || quantity <= 0)
            { 
                return BadRequest("Product name and/or quantity are invalid.");

            }
            var foundProduct = _productRepository.ProductExists(name);
            if(!foundProduct)
            {
                return NotFound ($"Product with name {name} does not exist.");
            }
            if(!_productRepository.BuyProduct(name,quantity))
            {
                ModelState.AddModelError("Custom Error", $"Something went wrong while buying the product {name}");
            }
            return Ok($"You have bought {quantity} of {name}. Enjoy the article!");
        }
        [HttpPatch("addStock/{name}/{quantity:int}", Name = "AddStock")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult AddStock(string name, int quantity)
        {
            if(string.IsNullOrEmpty(name) || quantity <= 0)
            {
                return BadRequest("Product name and/or quantity are invalid."); 
            }
            var foundProduct = _productRepository.ProductExists(name);
            if(!foundProduct)
            {
                return NotFound($"Product {name} does not exist.");
            }
            if(!_productRepository.AddStock(name,quantity))
            {
                ModelState.AddModelError("Custom Error", $"Something went wrong while adding stock to the product {name}");
            }
            return Ok($"You have added {quantity} of stock to {name}. Now you have more units to sell!");
        }
        
        [HttpGet("searchProductByNameDescription/{searchTerm}", Name = "SearchProducts")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult SearchProducts(string searchTerm)
        {
            if(string.IsNullOrEmpty(searchTerm))
            {
                return BadRequest("Product name is invalid.");
            }
            var products = _productRepository.SearchProducts(searchTerm);
            if(products.Count == 0)
            {
                return NotFound($"No products found with name {searchTerm}.");
            }
            var productsDto = _mapper.Map<List<ProductDto>>(products); 
            
            return Ok(productsDto);
        }

        [HttpPut("updateProduct", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateProduct(int productId,[FromBody] UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null)
            {
                return BadRequest(ModelState);
            }
            if(!_productRepository.ProductExists(productId))
            {
                return NotFound($"Product with id {productId} does not exist.");
            }
            if(!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
            {
                ModelState.AddModelError("Custom Error",$"Category with id {updateProductDto.CategoryId} does not exist."); 
                return BadRequest(ModelState);
            }
            var product = _mapper.Map<Product>(updateProductDto);
            product.ProductId = productId;
            if (!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("Custom Error", $"Algo salio mal al guardar el registro {product.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
        [HttpDelete("{productId:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteProduct(int productId)
        {
            if (productId == 0)
            {
                return BadRequest(ModelState);
            }
            var product = _productRepository.GetProduct(productId);
            
            if(product == null)
            {
                return NotFound($"Product with id {productId} does not exist");
            }
            if(!_productRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("Custom Error", $"Something went wrong while deleting the product {product.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
    } 
}
