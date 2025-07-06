namespace Core;

public class Attendance
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public DateTime Date { get; set; } // Sadece tarih kısmı (gün için)

    public Attendance(string username, string fullName, DateTime timestamp)
    {
        Username = username;
        FullName = fullName;
        Timestamp = timestamp;
        Date = timestamp.Date; // Sadece tarih kısmını al
    }

    public Attendance() { }
}

// Günlük yoklama grubu için yeni model
public class DailyAttendance
{
    public DateTime Date { get; set; }
    public List<Attendance> Attendances { get; set; } = new();
    public int TotalCount => Attendances.Count;
    
    public DailyAttendance(DateTime date)
    {
        Date = date.Date;
    }
}
