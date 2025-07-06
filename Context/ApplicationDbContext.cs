using AttendanceApp.Models;
using Microsoft.EntityFrameworkCore;
namespace AttendanceApp.Context;
public sealed class ApplicationDbContext : DbContext
{


    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<User> Users {get;set;}
    public DbSet<ActiveCode> ActiveCodes {get;set;}
    public DbSet<AttendanceApp.Models.Attendance> Attendances {get;set;}
    public DbSet<AttendanceApp.Models.DailyAttendance> DailyAttendances {get;set;}
}