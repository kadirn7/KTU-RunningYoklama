namespace AttendanceApp.Models;

public sealed class User{
    public int Id {get; set;}
    public required string Username{get;set;}
    public required string Email{get;set;}
    public required string PasswordHash{get;set;}
    public required string FullName{get;set;}
    public DateTime CreatedAt{get;set;}
    public bool IsActive { get; set; } = true;
    public string Role { get; set; } = "User";
}