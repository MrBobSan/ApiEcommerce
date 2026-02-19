using ApiEcommerce.Repository.IRepository;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.HttpLogging;

namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUsers()
        {
            var users = _userRepository.GetUsers();
            var usersDtop = _mapper.Map<List<UserDto>>(users);
            return Ok(usersDtop);
        }
        [HttpGet("{id:int}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUser(int id)
        {
            var user = _userRepository.GetUser(id);
            if(user == null)
            {
                return NotFound($"User with id {id} not found.");
            }
            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }


        [HttpPost(Name = "RegisterUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
        {
            if (createUserDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(string.IsNullOrWhiteSpace(createUserDto.Username))
            {
                ModelState.AddModelError("Custom Error","El nombre de usuario no puede estar vacío");
                return BadRequest(ModelState);
            }
            if (!_userRepository.IsUniqueUser(createUserDto.Username))
            {
                ModelState.AddModelError("Custom Error","El usuario ya existe");
                return BadRequest(ModelState);
            }
            var createdUser = await _userRepository.Register(createUserDto);
            if(createdUser == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear el usuario");
            }
            return CreatedAtRoute("GetUser", new { id = createdUser.Id }, createdUser);
        }

        [HttpPost("Login", Name = "LoginUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
        {
            if (userLoginDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userRepository.Login(userLoginDto);
            if(user == null)
            {
                return Unauthorized("Credenciales inválidas");
            }
            return  Ok(user);
        }
    }
}
