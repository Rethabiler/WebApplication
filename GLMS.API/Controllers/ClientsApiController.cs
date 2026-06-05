using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Models;

namespace GLMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/clients
        [HttpGet]
        public async Task<IActionResult> GetClients([FromQuery] string? search)
        {
            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search) || c.Region.Contains(search));

            var clients = await query
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Contact,
                    c.Details,
                    c.Region,
                    ContractCount = c.Contracts!.Count()
                })
                .ToListAsync();

            return Ok(clients);
        }

        // GET: api/clients/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                return NotFound(new { message = "Client not found." });

            return Ok(new
            {
                client.Id,
                client.Name,
                client.Contact,
                client.Details,
                client.Region,
                Contracts = client.Contracts?.Select(c => new
                {
                    c.Id,
                    c.StartDate,
                    c.EndDate,
                    c.Status,
                    c.ServiceLevel
                })
            });
        }

        // POST: api/clients
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] Client client)
        {
            ModelState.Remove("Contracts");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClient),
                new { id = client.Id },
                new { client.Id, message = "Client created successfully." });
        }

        // PUT: api/clients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] Client client)
        {
            if (id != client.Id)
                return BadRequest(new { message = "ID mismatch." });

            ModelState.Remove("Contracts");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Update(client);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Client updated successfully." });
        }

        // DELETE: api/clients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
                return NotFound(new { message = "Client not found." });

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Client deleted successfully." });
        }
    }
}
