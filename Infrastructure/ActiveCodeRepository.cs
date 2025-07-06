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

    // Geliştirme aşamasında kodun süresi 1 gün ve Türkiye saatiyle kaydedilir
    public async Task SetActiveCodeAsync(string code, int minutes = 1440, bool useTurkeyTime = true)
    {
        // Önce eski kodları sil
        var all = _db.ActiveCodes.ToList();
        _db.ActiveCodes.RemoveRange(all);
        await _db.SaveChangesAsync();

        // Yeni kodu ekle
        var expiresAt = useTurkeyTime ? DateTime.UtcNow.AddHours(3).AddMinutes(minutes) : DateTime.UtcNow.AddMinutes(minutes);
        if (useTurkeyTime) {
            var nowTurkey = DateTime.UtcNow.AddHours(3);
            expiresAt = nowTurkey.AddMinutes(minutes);
        }
        var info = new ActiveCode
        {
            Code = code,
            ExpiresAt = expiresAt
        };
        _db.ActiveCodes.Add(info);
        await _db.SaveChangesAsync();
    }
} 