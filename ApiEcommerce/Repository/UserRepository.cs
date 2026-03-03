using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Permissions;
using System.Text;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace ApiEcommerce.Repository;

public class UserRepository :IUserRepository
{   
    public readonly ApplicationDbContext Db;
    private string? _secretKey;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;


    public UserRepository(ApplicationDbContext db, IConfiguration configuration,
        UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
    {
        Db = db;
        _secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }
    public User? GetUser(int id)
    {
        if(id <= 0) 
        { 
            return null;
        } 
        return Db.Users.FirstOrDefault(u => u.Id == id);
    }

    public ICollection<User> GetUsers()
    {
        return Db.Users.OrderBy(u =>u.Username).ToList();
    }

    public bool IsUniqueUser(string username)
    {
        if(string.IsNullOrEmpty(username))
        {
            return false;
        }
        return !Db.Users.Any(u => !string.IsNullOrEmpty(u.Name) && u.Name.ToLower().Trim() == username.ToLower().Trim());
    }
    public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
    {
        if(string.IsNullOrEmpty(userLoginDto.Username))
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "UserName es requerido"
            };
        }
        var user = await Db.ApplicationUsers.FirstOrDefaultAsync<ApplicationUser>(u=> u.UserName != null && u.UserName.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());
        if(user == null)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "UserName no encontrado"
            };
        }
        if(userLoginDto.Password == null)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Password requerido"
            };
        }
        bool isValid = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);
        if(!isValid)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Credenciales incorrectas"
            };
        }
        
        var handlerToken = new JwtSecurityTokenHandler();
        if(string.IsNullOrWhiteSpace(_secretKey))
        {
            throw new InvalidOperationException("SecretKey no esta configurada");
        }
        var roles = await _userManager.GetRolesAsync(user);
        var key = Encoding.UTF8.GetBytes(_secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.UserName?? string.Empty),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty)
            }
            ),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handlerToken.CreateToken(tokenDescriptor);
        return new UserLoginResponseDto()
        {
            Token = handlerToken.WriteToken(token),
            User = _mapper.Map<UserDataDto>(user),
            Message = "Usuario logueado correctamente"
        };
    }
    public async Task<User> Register(CreateUserDto createUserDto) 
    { 
        var encryptedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
        var user = new User
        {
            Username = createUserDto.Username ?? "No username",
            Name = createUserDto.Name,
            Role = createUserDto.Role,
            Password = encryptedPassword
        };
        Db.Users.Add(user);
        await Db.SaveChangesAsync();
        return user; 
    }

}
