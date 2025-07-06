using Core;
using System.Text.Json;

namespace Infrastructure;

public class DailyAttendanceRepository
{
    private readonly string _dataPath = "Data";
    private readonly string _filePath;
    
    public DailyAttendanceRepository()
    {
        if (!Directory.Exists(_dataPath))
            Directory.CreateDirectory(_dataPath);
        _filePath = Path.Combine(_dataPath, "daily_attendances.json");
    }
    
    public async Task<List<DailyAttendance>> GetAllAsync()
    {
        if (!File.Exists(_filePath))
            return new List<DailyAttendance>();
            
        var json = await File.ReadAllTextAsync(_filePath);
        if (string.IsNullOrEmpty(json))
            return new List<DailyAttendance>();
            
        try
        {
            var dailyAttendances = JsonSerializer.Deserialize<List<DailyAttendance>>(json);
            return dailyAttendances ?? new List<DailyAttendance>();
        }
        catch
        {
            return new List<DailyAttendance>();
        }
    }
    
    public async Task AddAttendanceAsync(Attendance attendance)
    {
        var dailyAttendances = await GetAllAsync();
        var date = attendance.Date;
        
        var dailyAttendance = dailyAttendances.FirstOrDefault(da => da.Date == date);
        if (dailyAttendance == null)
        {
            dailyAttendance = new DailyAttendance(date);
            dailyAttendances.Add(dailyAttendance);
        }
        
        // Aynı kullanıcının o gün için yoklaması var mı kontrol et
        var existingAttendance = dailyAttendance.Attendances.FirstOrDefault(a => a.Username == attendance.Username);
        if (existingAttendance == null)
        {
            dailyAttendance.Attendances.Add(attendance);
        }
        
        await SaveAsync(dailyAttendances);
    }
    
    public async Task<DailyAttendance?> GetByDateAsync(DateTime date)
    {
        var dailyAttendances = await GetAllAsync();
        return dailyAttendances.FirstOrDefault(da => da.Date == date);
    }
    
    public async Task<List<string>> GetAllUsernamesAsync()
    {
        var dailyAttendances = await GetAllAsync();
        var usernames = new HashSet<string>();
        
        foreach (var daily in dailyAttendances)
        {
            foreach (var attendance in daily.Attendances)
            {
                usernames.Add(attendance.Username);
            }
        }
        
        return usernames.OrderBy(u => u).ToList();
    }
    
    public async Task<bool> HasAttendedTodayAsync(string username, DateTime date)
    {
        var dailyAttendances = await GetAllAsync();
        var dailyAttendance = dailyAttendances.FirstOrDefault(da => da.Date == date.Date);
        if (dailyAttendance == null) return false;
        return dailyAttendance.Attendances.Any(a => a.Username == username);
    }
    
    private async Task SaveAsync(List<DailyAttendance> dailyAttendances)
    {
        var json = JsonSerializer.Serialize(dailyAttendances, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(_filePath, json);
    }
} 