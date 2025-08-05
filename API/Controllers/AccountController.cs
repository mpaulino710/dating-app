using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(AppDbContext context, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")] // localhost:5001/api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        // Check if the email already exists
        if (await EmailExists(registerDto.Email))
        {
            return BadRequest("Email already exists");
        }

        // generate aleatory salt and hash the password
        using var hmac = new HMACSHA512();

        // Create new user
        var user = new AppUser
        {
            DisplayName = registerDto.DisplayName,
            Email = registerDto.Email,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)), // Placeholder for password hash
            PasswordSalt = hmac.Key,  // Placeholder for password salt
            Member = new Member
            {
                DisplayName = registerDto.DisplayName,
                Gender = registerDto.Gender,
                City = registerDto.City,
                Country = registerDto.Country,
                DateOfBirth = registerDto.DateOfBirth
            }
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.ToDto(tokenService); // Convert the user to UserDto and return it
    }

    [HttpPost("login")] // localhost:5001/api/account/login
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        // Check if the user exists
        var user = await context.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == loginDto.Email.ToLower());

        if (user == null)
        {
            return Unauthorized("Invalid email or password");
        }

        // Verify the password
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
            {
                return Unauthorized("Invalid email or password");
            }
        }

        return user.ToDto(tokenService); // Convert the user to UserDto and return it
    }

    private async Task<bool> EmailExists(string email)
    {
        return await context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
    }
}
