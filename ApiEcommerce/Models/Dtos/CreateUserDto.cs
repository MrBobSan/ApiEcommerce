using System;
using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models.Dtos;

public class CreateUserDto
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set;} = string.Empty;
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set;} = string.Empty;
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set;}  = string.Empty;
    [Required(ErrorMessage ="Rol is required.")]
    public string Role { get; set;} = string.Empty;
}
