namespace FoxMield.Components.Models
{
    public class ApplicationUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Username { get; set; } = "";

        public string Email { get; set; } = "";

        public string PasswordHash { get; set; } = "";

        public DateTimeOffset CreatedAt { get; set; } =
            DateTimeOffset.UtcNow;
    }
}