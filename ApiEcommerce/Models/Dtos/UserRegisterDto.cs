using System;
using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models.Dtos;

public class UserRegisterDto
{    
    public string? ID { get; set;}
    public string? Username { get; set;}
    public string? Password { get; set;}
    public string? Role { get; set;}
}
