using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedData(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var membersData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var members = JsonSerializer.Deserialize<List<SeedUserDto>>(membersData);

        if (members == null)
        {
            Console.WriteLine("No members found in the seed data.");
            return;
        }

        foreach (var member in members)
        {
            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                Id = member.Id,
                Email = member.Email,
                ImageUrl = member.ImageUrl,
                DisplayName = member.DisplayName,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd")),
                PasswordSalt = hmac.Key,
                Member = new Member
                {
                    Id = member.Id,
                    Description = member.Description,
                    DateOfBirth = member.DateOfBirth,
                    DisplayName = member.DisplayName,
                    Created = member.Created,
                    LastActive = member.LastActive,
                    ImageUrl = member.ImageUrl,
                    Gender = member.Gender,
                    City = member.City,
                    Country = member.Country
                }
            };

            user.Member.Photos.Add(new Photo
            {
                Url = member.ImageUrl!,
                MemberId = user.Id
            });

            context.Users.Add(user);
        }

        if (await context.SaveChangesAsync() > 0)
        {
            Console.WriteLine("Seed data successfully added to the database.");
        }
        else
        {
            Console.WriteLine("Failed to add seed data to the database.");
        }   
    }
}
