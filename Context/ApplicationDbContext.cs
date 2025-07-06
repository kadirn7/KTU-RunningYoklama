using AttendanceApp.Models;
using Microsoft.EntityFrameworkCore;
namespace AttendanceApp.Context;
public sealed class ApplicationDbContext : DbContext
{


    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<User> Users {get;set;}
}