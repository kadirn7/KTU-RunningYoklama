using AttendanceApp.Context;
using AttendanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class ActiveCodeRepository
{
    private readonly ApplicationDbContext _db;
    public ActiveCodeRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ActiveCode?> GetActiveCodeAsync()
    {
        return await _db.ActiveCodes.OrderByDescending(a => a.ExpiresAt).FirstOrDefaultAsync();
    }

    public async Task SetActiveCodeAsync(string code, int minutes = 1440)
    {
        // Önce eski kodları sil
        var all = _db.ActiveCodes.ToList();
        _db.ActiveCodes.RemoveRange(all);
        await _db.SaveChangesAsync();

        // Yeni kodu ekle
        var info = new ActiveCode
        {
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(minutes)
        };
        _db.ActiveCodes.Add(info);
        await _db.SaveChangesAsync();
    }
} 