

namespace WebApplication.Models
{
    public class Client
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;

        public ICollection<Contract>? Contracts { get; set; }
    }
}