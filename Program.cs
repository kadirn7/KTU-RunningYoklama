using System.Security.Claims;
using Core;
using Microsoft.AspNetCore.Mvc;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using AttendanceApp.Context;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddDbContext<ApplicationDbContext>(options=>{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

var app = builder.Build();

// Production ayarları
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Constants
const string SESSION_COOKIE = "session";

// Initialize default admin user
// using (var scope = app.Services.CreateScope())
// {
//     var userRepo = scope.ServiceProvider.GetRequiredService<UserRepository>();
//     await userRepo.InitializeDefaultAdminAsync();
// }

// Register endpoint
app.MapPost("/api/register", async (HttpContext ctx, UserRepository userRepo, 
    [FromForm] string username, [FromForm] string email, [FromForm] string password, [FromForm] string fullName) =>
{
    try
    {
        // Validation
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || 
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
        {
            return Results.BadRequest("Tüm alanlar gereklidir");
        }

        if (password.Length < 6)
        {
            return Results.BadRequest("Şifre en az 6 karakter olmalıdır");
        }

        var user = new User(username, email, PasswordService.HashPassword(password), fullName);
        await userRepo.AddAsync(user);
        
        return Results.Ok(new { message = "Kullanıcı başarıyla oluşturuldu" });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception)
    {
        return Results.StatusCode(500);
    }
}).DisableAntiforgery();

// Login endpoint
app.MapPost("/api/login", async (HttpContext ctx, UserRepository userRepo, 
    [FromForm] string username, [FromForm] string password) =>
{
    try
    {
        var user = await userRepo.GetByUsernameAsync(username);
        if (user == null || !PasswordService.VerifyPassword(password, user.PasswordHash) || !user.IsActive)
    {
            return Results.Unauthorized();
        }

        var sessionId = SessionService.CreateSession(user);
        ctx.Response.Cookies.Append(SESSION_COOKIE, sessionId, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = ctx.Request.IsHttps,
            Expires = DateTime.Now.AddHours(24)
        });
        
        return Results.Ok(new { message = "Giriş başarılı", user = new { user.Username, user.FullName, user.Role } });
    }
    catch (Exception)
    {
        return Results.StatusCode(500);
    }
}).DisableAntiforgery();

// Logout endpoint
app.MapPost("/api/logout", (HttpContext ctx) =>
{
    var sessionId = ctx.Request.Cookies[SESSION_COOKIE];
    if (!string.IsNullOrEmpty(sessionId))
    {
        SessionService.RemoveSession(sessionId);
    }
    
    ctx.Response.Cookies.Delete(SESSION_COOKIE);
    return Results.Ok();
}).DisableAntiforgery();

// Set active code (admin only, 5 dakika süreli)
app.MapPost("/api/activecode", async (HttpContext ctx, ActiveCodeRepository codeRepo) =>
{
    var sessionId = ctx.Request.Cookies[SESSION_COOKIE];
    var session = SessionService.GetSession(sessionId);
    if (session == null || session.Role != "Admin")
    return Results.Unauthorized();
    var form = await ctx.Request.ReadFormAsync();
    var code = form["code"].ToString();
    if (string.IsNullOrWhiteSpace(code))
        return Results.BadRequest("Kod boş olamaz");
    await codeRepo.SetActiveCodeAsync(code, 5); // 5 dakika
    return Results.Ok(new { message = "Kod güncellendi", code });
}).DisableAntiforgery();

// Get active code (admin only)
app.MapGet("/api/activecode", async (HttpContext ctx, ActiveCodeRepository codeRepo) =>
{
    var sessionId = ctx.Request.Cookies[SESSION_COOKIE];
    var session = SessionService.GetSession(sessionId);
    if (session == null || session.Role != "Admin")
        return Results.Unauthorized();
    var info = await codeRepo.GetActiveCodeAsync();
    return Results.Ok(new { code = info?.Code, expiresAt = info?.ExpiresAt });
});

// Attendance endpoint (only code, name from session, kod süresi kontrolü)
app.MapPost("/api/attendance", async (HttpContext ctx, DailyAttendanceRepository repo, ActiveCodeRepository codeRepo, UserRepository userRepo) =>
{
    var sessionId = ctx.Request.Cookies[SESSION_COOKIE];
    var session = SessionService.GetSession(sessionId);
    if (session == null)
        return Results.Unauthorized();
    var form = await ctx.Request.ReadFormAsync();
    var code = form["code"].ToString();
    var info = await codeRepo.GetActiveCodeAsync();
    if (info == null || string.IsNullOrWhiteSpace(info.Code))
        return Results.BadRequest("Aktif kod yok");
    if (DateTime.UtcNow > info.ExpiresAt)
        return Results.BadRequest("Kodun süresi doldu");
    if (code != info.Code)
        return Results.BadRequest("Kod yanlış");
    
    // Kullanıcının tam adını al
    var user = await userRepo.GetByUsernameAsync(session.Username);
    var fullName = user?.FullName ?? session.Username;

    // Kullanıcı bugün yoklama verdi mi kontrol et
    if (await repo.HasAttendedTodayAsync(session.Username, DateTime.Now.Date))
    {
        return Results.BadRequest(new { message = "Zaten yoklamaya katıldınız. Keyifli koşular! 🏃‍♂️🏃‍♀️🎉" });
    }
    
    await repo.AddAttendanceAsync(new Attendance(session.Username, fullName, DateTime.Now));
    return Results.Ok();
}).DisableAntiforgery();

// Get daily attendance list (Admin only)
app.MapGet("/api/attendance", async (HttpContext ctx, DailyAttendanceRepository repo) =>
{
    var sessionId = ctx.Request.Cookies[SESSION_COOKIE];
    var session = SessionService.GetSession(sessionId);
    
    if (session == null || session.Role != "Admin")
        return Results.Unauthorized();

    var dailyAttendances = await repo.GetAllAsync();
    return Results.Ok(dailyAttendances);
});

// Get attendance by date (Admin only)
app.MapGet("/api/attendance/{date}", async (HttpContext ctx, DailyAttendanceRepository repo, string date) =>
{
    var sessionId = ctx.Request.Cookies[SESSION_COOKIE];
    var session = SessionService.GetSession(sessionId);
    
    if (session == null || session.Role != "Admin")
        return Results.Unauthorized();

    if (!DateTime.TryParse(date, out var parsedDate))
        return Results.BadRequest("Geçersiz tarih formatı");

    var dailyAttendance = await repo.GetByDateAsync(parsedDate);
    return Results.Ok(dailyAttendance);
});

// Get attendance status (user only)
app.MapGet("/api/attendance/status", async (HttpContext ctx, DailyAttendanceRepository repo) =>
{
    var sessionId = ctx.Request.Cookies[SESSION_COOKIE];
    var session = SessionService.GetSession(sessionId);
    if (session == null)
        return Results.Unauthorized();

    bool attended = await repo.HasAttendedTodayAsync(session.Username, DateTime.Now.Date);
    return Results.Ok(new { attended });
});

// Get all users for Excel export (Admin only)
app.MapGet("/api/users/export", async (HttpContext ctx, UserRepository userRepo) =>
{
    var sessionId = ctx.Request.Cookies[SESSION_COOKIE];
    var session = SessionService.GetSession(sessionId);
    
    if (session == null || session.Role != "Admin")
        return Results.Unauthorized();

    var users = await userRepo.GetAllAsync();
    var userList = users.Select(u => new { u.Username, u.FullName }).ToList();
    return Results.Ok(userList);
});

// Get current user info
app.MapGet("/api/user", async (HttpContext ctx, UserRepository userRepo) =>
{
    var sessionId = ctx.Request.Cookies[SESSION_COOKIE];
    var session = SessionService.GetSession(sessionId);
    
    if (session == null)
        return Results.Unauthorized();

    // Kullanıcının tam bilgilerini al
    var user = await userRepo.GetByUsernameAsync(session.Username);
    return Results.Ok(new { 
        session.Username, 
        session.Role,
        fullName = user?.FullName ?? session.Username
    });
});

app.MapPost("/api/reset-password", async (UserRepository userRepo, [FromForm] string username, [FromForm] string email, [FromForm] string newPassword) =>
{
    var user = await userRepo.GetByUsernameAsync(username);
    if (user == null || !user.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new { message = "Kullanıcı adı ve e-posta eşleşmiyor" });
    if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        return Results.BadRequest(new { message = "Yeni şifre en az 6 karakter olmalı" });
    user.PasswordHash = PasswordService.HashPassword(newPassword);
    await userRepo.UpdateAsync(user);
    return Results.Ok(new { message = "Şifreniz başarıyla güncellendi. Yeni şifrenizle giriş yapabilirsiniz." });
}).DisableAntiforgery();

// Kullanıcı profil bilgilerini getir
app.MapGet("/api/profile", async (HttpContext ctx, UserRepository userRepo) =>
{
    var sessionId = ctx.Request.Cookies[SESSION_COOKIE];
    var session = SessionService.GetSession(sessionId);
    if (session == null)
        return Results.Unauthorized();
    var user = await userRepo.GetByUsernameAsync(session.Username);
    if (user == null)
        return Results.NotFound();
    return Results.Ok(new { user.Username, user.FullName, user.Email });
});

// Kullanıcı profil bilgilerini güncelle
app.MapPost("/api/profile", async (HttpContext ctx, UserRepository userRepo) =>
{
    var sessionId = ctx.Request.Cookies[SESSION_COOKIE];
    var session = SessionService.GetSession(sessionId);
    if (session == null)
        return Results.Unauthorized();
    var user = await userRepo.GetByUsernameAsync(session.Username);
    if (user == null)
        return Results.NotFound();
    var form = await ctx.Request.ReadFormAsync();
    var fullName = form["fullName"].ToString();
    var email = form["email"].ToString();
    var oldPassword = form["oldPassword"].ToString();
    var newPassword = form["newPassword"].ToString();
    if (!string.IsNullOrWhiteSpace(fullName)) user.FullName = fullName;
    if (!string.IsNullOrWhiteSpace(email)) user.Email = email;
    if (!string.IsNullOrWhiteSpace(newPassword)) {
        if (string.IsNullOrWhiteSpace(oldPassword) || !PasswordService.VerifyPassword(oldPassword, user.PasswordHash))
            return Results.BadRequest(new { message = "Eski şifre yanlış" });
        if (newPassword.Length < 6)
            return Results.BadRequest(new { message = "Yeni şifre en az 6 karakter olmalı" });
        user.PasswordHash = PasswordService.HashPassword(newPassword);
    }
    await userRepo.UpdateAsync(user);
    return Results.Ok(new { message = "Profil güncellendi" });
}).DisableAntiforgery();

app.Run();
