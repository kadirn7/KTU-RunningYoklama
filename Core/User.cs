using System.ComponentModel.DataAnnotations;

namespace Core;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public string FullName { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public bool IsActive { get; set; } = true;
    
    public string Role { get; set; } = "User"; // Admin, User
    
    public User() { }
    
    public User(string username, string email, string passwordHash, string fullName)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
    }
} 