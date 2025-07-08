using System.Text.Json;
using Core;
using AttendanceApp.Context;
using AttendanceApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public class UserRepository
{
    private readonly ApplicationDbContext _db;
    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<AttendanceApp.Models.User>> GetAllAsync()
    {
        return await _db.Users.ToListAsync();
    }

    public async Task<AttendanceApp.Models.User?> GetByUsernameAsync(string username)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<AttendanceApp.Models.User?> GetByEmailAsync(string email)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task AddAsync(AttendanceApp.Models.User user)
    {
        if (await _db.Users.AnyAsync(u => u.Username.ToLower() == user.Username.ToLower()))
            throw new InvalidOperationException("Username already exists");
        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == user.Email.ToLower()))
            throw new InvalidOperationException("Email already exists");
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AttendanceApp.Models.User user)
    {
        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (existingUser == null)
            throw new InvalidOperationException("User not found");
        existingUser.Username = user.Username;
        existingUser.Email = user.Email;
        existingUser.FullName = user.FullName;
        existingUser.IsActive = user.IsActive;
        existingUser.Role = user.Role;
        existingUser.PasswordHash = user.PasswordHash;
        await _db.SaveChangesAsync();
    }

    public async Task InitializeDefaultAdminAsync(IConfiguration config)
    {
        var adminSection = config.GetSection("AdminUser");
        var username = adminSection["Username"];
        var email = adminSection["Email"];
        var password = adminSection["Password"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Admin kullanıcı bilgileri eksik!");

        if (!await _db.Users.AnyAsync(u => u.Username == username))
        {
            var adminUser = new AttendanceApp.Models.User
            {
                Username = username,
                Email = email,
                PasswordHash = PasswordService.HashPassword(password),
                FullName = "Yönetici",
                Role = "Admin",
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            _db.Users.Add(adminUser);
            await _db.SaveChangesAsync();
        }
    }
} 