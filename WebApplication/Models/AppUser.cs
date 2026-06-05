namespace WebApplication.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        // Full name of the employee
        public string FullName { get; set; } = string.Empty;

        // Username for login — no registration, admin creates these
        public string Username { get; set; } = string.Empty;

        // Stored as BCrypt hash — never plain text
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Employee";

        // Navigation property
        public ICollection<Payment>? Payments { get; set; }
    }
}