using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Permissions;
using System.Text;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiEcommerce.Repository;

public class UserRepository :IUserRepository
{   
    public readonly ApplicationDbContext Db;
    private string? _secretKey;
    
    public UserRepository(ApplicationDbContext db, IConfiguration configuration)
    {
        Db = db;
        _secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
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
        var user = await Db.Users.FirstOrDefaultAsync<User>(u => u.Username.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());
        if(user == null)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "UserName no encontrado"
            };
        }
        if(!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password))
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Password incorrecto"
            };
        }
        
        var handlerToken = new JwtSecurityTokenHandler();
        if(string.IsNullOrWhiteSpace(_secretKey))
        {
            throw new InvalidOperationException("SecretKey no esta configurada");
        }
        var key = Encoding.UTF8.GetBytes(_secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
            }
            ),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handlerToken.CreateToken(tokenDescriptor);
        return new UserLoginResponseDto()
        {
            Token = handlerToken.WriteToken(token),
            User = new UserRegisterDto()
            {
                Username = user.Username,
                Name = user.Name,
                Role = user.Role,
                Password = user.Password ?? ""
            },
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
