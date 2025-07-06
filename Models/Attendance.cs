using System;
using System.Collections.Generic;

namespace AttendanceApp.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public DateTime Date { get; set; }
    }

    public class DailyAttendance
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public List<Attendance> Attendances { get; set; } = new();
        public int TotalCount => Attendances.Count;
    }
} 