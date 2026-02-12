using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace ApiEcommerce.Models;

public class User
{
    public int Id { get; set;}
    public string Name { get; set;} = string.Empty;
    public string Username { get; set;} = string.Empty;
    public string Password { get; set;}  = string.Empty;
    public string Role { get; set;} = string.Empty;
}