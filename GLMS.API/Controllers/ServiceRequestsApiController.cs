using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Models;

namespace GLMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceRequestsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/servicerequests
        [HttpGet]
        public async Task<IActionResult> GetServiceRequests([FromQuery] int? contractId)
        {
            var query = _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c!.Client)
                .AsQueryable();

            if (contractId.HasValue)
                query = query.Where(s => s.ContractId == contractId.Value);

            var requests = await query
                .Select(s => new
                {
                    s.Id,
                    s.ContractId,
                    ClientName = s.Contract!.Client!.Name,
                    s.Description,
                    s.Cost,
                    s.Status
                })
                .ToListAsync();

            return Ok(requests);
        }

        // GET: api/servicerequests/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceRequest(int id)
        {
            var sr = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sr == null)
                return NotFound(new { message = "Service request not found." });

            return Ok(new
            {
                sr.Id,
                sr.ContractId,
                ClientName = sr.Contract!.Client!.Name,
                sr.Description,
                sr.Cost,
                sr.Status
            });
        }

        // POST: api/servicerequests
        [HttpPost]
        public async Task<IActionResult> CreateServiceRequest([FromBody] ServiceRequest serviceRequest)
        {
            // ── WORKFLOW ENFORCEMENT ──
            var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);

            if (contract == null)
                return BadRequest(new { message = "Contract not found." });

            if (contract.Status == "Expired" || contract.Status == "On Hold")
                return BadRequest(new { message = $"Cannot create service request: contract is '{contract.Status}'." });

            ModelState.Remove("Contract");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetServiceRequest),
                new { id = serviceRequest.Id },
                new { serviceRequest.Id, message = "Service request created successfully." });
        }

        // PATCH: api/servicerequests/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateSrStatusRequest request)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);

            if (sr == null)
                return NotFound(new { message = "Service request not found." });

            var validStatuses = new[] { "Pending", "In Progress", "Completed", "Cancelled" };
            if (!validStatuses.Contains(request.Status))
                return BadRequest(new { message = "Invalid status." });

            sr.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Status updated to '{request.Status}'." });
        }

        // DELETE: api/servicerequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceRequest(int id)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);

            if (sr == null)
                return NotFound(new { message = "Service request not found." });

            _context.ServiceRequests.Remove(sr);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Service request deleted successfully." });
        }
    }

    public class UpdateSrStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
