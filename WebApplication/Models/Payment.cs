namespace WebApplication.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int AppUserId { get; set; }

        public string Recipient { get; set; } = string.Empty;

        public string Bank { get; set; } = string.Empty;

        // e.g. USD, EUR, ZAR
        public string Currency { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        // SWIFT code for international payments
        public string SwiftCode { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public AppUser? AppUser { get; set; }
    }
}