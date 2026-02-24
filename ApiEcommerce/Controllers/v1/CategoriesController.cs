using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ApiEcommerce.Constants;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

namespace ApiEcommerce.Controllers.V1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    // [EnableCors(PolicyNames.AllowSpecificOrigin)]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [MapToApiVersion("1.0")]
        [Obsolete("This method is deprecated, use GetCategoriesOrderById of v2 instead.")]
        //[EnableCors(PolicyNames.AllowSpecificOrigin)]
        public IActionResult GetCategories()
        {
            var categories = _categoryRepository.GetCategories();
            var categoriesDto = new List<CategoryDto>();
            foreach(var category in categories)
            {
                categoriesDto.Add(_mapper.Map<CategoryDto>(category));
            }
            return Ok(categoriesDto);
        }
        [AllowAnonymous]
        [HttpGet("{id:int}", Name = "GetCategory")]
        // [ResponseCache(Duration = 10)]
        [ResponseCache(CacheProfileName = CacheProfiles.Default10)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCategory(int id)
        {
            System.Console.WriteLine($"Categoria con el ID {id} solicitada a las {DateTime.Now}.");
            var category = _categoryRepository.GetCategory(id);
            System.Console.WriteLine($"Respuesta con el ID {id} obtenida a las {DateTime.Now}.");

            if (category == null)            
            {
                return NotFound($"Category with id {id} not found.");
            }
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Ok(categoryDto);
        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            if (createCategoryDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_categoryRepository.CategoryExists(createCategoryDto.Name))
            {
                ModelState.AddModelError("Custom Error","La categoria ya existe");
                return BadRequest(ModelState);
            }
            var category = _mapper.Map<Category>(createCategoryDto);
            if (!_categoryRepository.CreateCategory(category))
            {
                ModelState.AddModelError("Custom Error", $"Algo salio mal al guardar el registro {category.Name}");
                return StatusCode(500, ModelState);
            }
            return CreatedAtRoute("GetCategory", new { id = category.Id }, category);
        }
        [HttpPatch("{id:int}", Name = "UpdateCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateCategory(int id,[FromBody] CreateCategoryDto updateCategoryDto)
        {
            if(!_categoryRepository.CategoryExists(id))
            {
                return NotFound($"Category with id {id} not found.");
            }
            if (updateCategoryDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_categoryRepository.CategoryExists(updateCategoryDto.Name))
            {
                ModelState.AddModelError("Custom Error","La categoria ya existe");
                return BadRequest(ModelState);
            }
            var category = _mapper.Map<Category>(updateCategoryDto);
            category.Id = id;
            if (!_categoryRepository.UpdateCategory(category))
            {
                ModelState.AddModelError("Custom Error", $"Algo salio mal al actualizar el registro {category.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "DeleteCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteCategory(int id)
        {
            if(!_categoryRepository.CategoryExists(id))
            {
                return NotFound($"Category with id {id} not found.");
            }
            var category = _categoryRepository.GetCategory(id);
            if (category == null)
            {
                return NotFound($"Category with id {id} not found.");
            }
            if (!_categoryRepository.DeleteCategory(category))
            {
                ModelState.AddModelError("Custom Error", $"Algo salio mal al eliminar el registro {category.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
    }
}
