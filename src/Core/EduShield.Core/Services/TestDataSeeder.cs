using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;

namespace EduShield.Core.Services;

public interface ITestDataSeeder
{
    Task SeedUsersAsync();
}

public class TestDataSeeder : ITestDataSeeder
{
    private readonly IUserRepository _userRepository;

    public TestDataSeeder(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task SeedUsersAsync()
    {
        var users = new List<Entities.User>
        {
            new()
            {
                Email = "iamsaquibanwar@gmail.com",
                Name = "Saquib Admin",
                Role = UserRole.Admin,
                IsActive = true
            },
            new()
            {
                Email = "saquibanwar01@gmail.com",
                Name = "Saquib Student",
                Role = UserRole.Student,
                IsActive = true
            },
            new()
            {
                Email = "saquibedu@gmail.com",
                Name = "Saquib Faculty",
                Role = UserRole.Faculty,
                IsActive = true
            },
            new()
            {
                Email = "kirakryto9ite@gmail.com",
                Name = "Saquib Parent",
                Role = UserRole.Parent,
                IsActive = true
            },
            new()
            {
                Email = "techtonicwave.business@gmail.com",
                Name = "Saquib Dev Admin",
                Role = UserRole.DevAuth,
                IsActive = true
            }
        };

        foreach (var user in users)
        {
            if (!await _userRepository.ExistsAsync(user.Email))
            {
                await _userRepository.CreateAsync(user);
            }
        }
    }
}
