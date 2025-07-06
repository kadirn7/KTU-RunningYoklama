using System.Text.Json;
using Core;

namespace Infrastructure;

public class FileJsonRepository
{
    private readonly string _path = Path.Combine("Data", "yoklama.json");

    public async Task AddAsync(Attendance item)
    {
        try
        {
        var list = await GetAllAsync();
        list.Add(item);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            // Log the error for debugging
            Console.WriteLine($"Error adding attendance: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Attendance>> GetAllAsync()
    {
        try
    {
        if (!File.Exists(_path)) return new List<Attendance>();
        var json = await File.ReadAllTextAsync(_path);
        return JsonSerializer.Deserialize<List<Attendance>>(json) ?? new List<Attendance>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading attendance data: {ex.Message}");
            return new List<Attendance>();
        }
    }
}
