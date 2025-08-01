using System;
using API.DTOs;
using API.Entities;
using API.Interfaces;

namespace API.Extensions;

public static class AppUserExtensions
{
    public static UserDto ToDto(this AppUser user, ITokenService tokenService)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            ImageUrl = user.ImageUrl,
            DisplayName = user.DisplayName,
            Token = tokenService.CreateToken(user) // Placeholder for JWT token generation
        };
    }
}
