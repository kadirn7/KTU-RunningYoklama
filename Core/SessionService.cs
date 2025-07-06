using System.Security.Cryptography;
using System.Text;

namespace Core;

public class SessionService
{
    private static readonly Dictionary<string, UserSession> _sessions = new();
    
    public static string CreateSession(AttendanceApp.Models.User user)
    {
        var sessionId = GenerateSessionId();
        var session = new UserSession
        {
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role,
            CreatedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddHours(24) // 24 saat geçerli
        };
        
        _sessions[sessionId] = session;
        return sessionId;
    }
    
    public static UserSession? GetSession(string? sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            return null;
            
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            if (session.ExpiresAt > DateTime.Now)
            {
                return session;
            }
            else
            {
                _sessions.Remove(sessionId);
            }
        }
        return null;
    }
    
    public static void RemoveSession(string sessionId)
    {
        _sessions.Remove(sessionId);
    }
    
    private static string GenerateSessionId()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

public class UserSession
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
} 