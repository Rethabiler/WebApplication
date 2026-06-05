using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Models;

namespace GLMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContractsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/contracts
        // Supports filtering by status and date range
        [HttpGet]
        public async Task<IActionResult> GetContracts(
            [FromQuery] string? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var query = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(c => c.Status == status);

            if (startDate.HasValue)
                query = query.Where(c => c.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.EndDate <= endDate.Value);

            var contracts = await query
                .OrderByDescending(c => c.StartDate)
                .Select(c => new
                {
                    c.Id,
                    c.ClientId,
                    ClientName = c.Client!.Name,
                    c.StartDate,
                    c.EndDate,
                    c.Status,
                    c.ServiceLevel,
                    c.AgreementFilePath
                })
                .ToListAsync();

            return Ok(contracts);
        }

        // GET: api/contracts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContract(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound(new { message = "Contract not found." });

            return Ok(new
            {
                contract.Id,
                contract.ClientId,
                ClientName = contract.Client!.Name,
                contract.StartDate,
                contract.EndDate,
                contract.Status,
                contract.ServiceLevel,
                contract.AgreementFilePath,
                ServiceRequests = contract.ServiceRequests?.Select(sr => new
                {
                    sr.Id,
                    sr.Description,
                    sr.Cost,
                    sr.Status
                })
            });
        }

        // POST: api/contracts
        [HttpPost]
        public async Task<IActionResult> CreateContract([FromBody] CreateContractRequest request)
        {
            // Validate client exists
            var client = await _context.Clients.FindAsync(request.ClientId);
            if (client == null)
                return BadRequest(new { message = "Client not found." });

            var contract = new Contract
            {
                ClientId = request.ClientId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status,
                ServiceLevel = request.ServiceLevel
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetContract),
                new { id = contract.Id },
                new { contract.Id, message = "Contract created successfully." });
        }

        // PATCH: api/contracts/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
                return NotFound(new { message = "Contract not found." });

            // Validate status value
            var validStatuses = new[] { "Draft", "Active", "Expired", "On Hold" };
            if (!validStatuses.Contains(request.Status))
                return BadRequest(new { message = "Invalid status. Must be Draft, Active, Expired, or On Hold." });

            contract.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Contract status updated to '{request.Status}'." });
        }

        // DELETE: api/contracts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
                return NotFound(new { message = "Contract not found." });

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Contract deleted successfully." });
        }
    }

    // Request models
    public class CreateContractRequest
    {
        public int ClientId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Draft";
        public string ServiceLevel { get; set; } = string.Empty;
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}