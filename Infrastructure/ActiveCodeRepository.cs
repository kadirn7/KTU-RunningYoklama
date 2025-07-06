using System.Text.Json;

namespace Infrastructure;

public class ActiveCodeInfo
{
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class ActiveCodeRepository
{
    private readonly string _path = Path.Combine("Data", "activecode.json");

    public async Task<ActiveCodeInfo?> GetActiveCodeAsync()
    {
        if (!File.Exists(_path)) return null;
        var json = await File.ReadAllTextAsync(_path);
        return JsonSerializer.Deserialize<ActiveCodeInfo>(json);
    }

    public async Task SetActiveCodeAsync(string code, int minutes = 5)
    {
        var info = new ActiveCodeInfo
        {
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(minutes)
        };
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(info));
    }
} 