using System;

namespace AttendanceApp.Models
{
    public class ActiveCode
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
} 