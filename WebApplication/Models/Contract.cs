namespace WebApplication.Models
{
    public class Contract
    {
        public int Id { get; set; }

        public int ClientId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string ServiceLevel { get; set; } = string.Empty;

        public string? AgreementFilePath { get; set; }

        public Client? Client { get; set; }
       

        public ICollection<ServiceRequest>? ServiceRequests { get; set; }
    }
}