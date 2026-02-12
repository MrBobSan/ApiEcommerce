using System;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ApiEcommerce.Repository;

public class UserRepository :IUserRepository
{   
    private readonly ApplicationDbContext _db;
    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }
    public ICollection<User> GetUsers()
    {
        return _db.Users.ToList(); 
    }
    public User? GetUser(int id)
    {
        if(id <= 0) 
        { 
            return null;
        } 
        return _db.Users.FirstOrDefault(u => u.Id == id);
    }
    public bool IsUniqueUser(string username)
    {
        if(string.IsNullOrEmpty(username))
        {
            return false;
        }
        return !_db.Users.Any(u => u.Name.ToLower().Trim() == username.ToLower().Trim());
    }
    public async Task<UserLoginResponseDto> Login(UserLoginDto user)
    {
        var userFromDb = await _db.Users.FirstOrDefaultAsync(u => u.Name.ToLower().Trim() == user.Username.ToLower().Trim()); 
        if(userFromDb == null) 
        { 
            return new UserLoginResponseDto{Success = false, Message = "Invalid username"}; 
        } // Here you would normally check the password, but since we don't have one, we'll skip that step. return new UserLoginResponseDto { Success = true, Message = "Login successful", User = userFromDb }; } public async Task<User> Register(CreateUserDto user) { var newUser = new User { Name = user.Username }; _db.Users.Add(newUser); await _db.SaveChangesAsync(); return newUser;
    }
    public async Task<User> Register(CreateUserDto user) 
    { 
        var newUser = new User { Name = user.Username }; 
        _db.Users.Add(newUser); 
        await _db.SaveChangesAsync(); 
        return newUser; 
    }

}
