using System.Text.Json;
using Core;

namespace Infrastructure;

public class UserRepository
{
    private readonly string _path = Path.Combine("Data", "users.json");

    public async Task<List<User>> GetAllAsync()
    {
        try
        {
            if (!File.Exists(_path)) return new List<User>();
            var json = await File.ReadAllTextAsync(_path);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading users: {ex.Message}");
            return new List<User>();
        }
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var users = await GetAllAsync();
        return users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var users = await GetAllAsync();
        return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task AddAsync(User user)
    {
        try
        {
            var users = await GetAllAsync();
            
            // Check if username or email already exists
            if (users.Any(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Username already exists");
                
            if (users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Email already exists");
            
            user.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
            users.Add(user);
            
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding user: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateAsync(User user)
    {
        try
        {
            var users = await GetAllAsync();
            var existingUser = users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser == null)
                throw new InvalidOperationException("User not found");
            
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.FullName = user.FullName;
            existingUser.IsActive = user.IsActive;
            existingUser.Role = user.Role;
            existingUser.PasswordHash = user.PasswordHash;
            
            await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating user: {ex.Message}");
            throw;
        }
    }

    public async Task InitializeDefaultAdminAsync()
    {
        var users = await GetAllAsync();
        if (!users.Any(u => u.Role == "Admin"))
        {
            var adminUser = new User(
                "admin",
                "admin@kosuapp.com",
                PasswordService.HashPassword("Admin123!"),
                "Sistem Yöneticisi"
            )
            {
                Role = "Admin",
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            
            await AddAsync(adminUser);
            Console.WriteLine("Default admin user created: admin / Admin123!");
        }
    }
} 