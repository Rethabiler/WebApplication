using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ContractsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Contracts — with search/filter by DateRange and Status
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string? status)
        {
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            // Filter by Status
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(c => c.Status == status);

            // Filter by Start Date
            if (startDate.HasValue)
                query = query.Where(c => c.StartDate >= startDate.Value);

            // Filter by End Date
            if (endDate.HasValue)
                query = query.Where(c => c.EndDate <= endDate.Value);

            // Pass filter values back to view
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;
            ViewBag.Statuses = new[] { "Draft", "Active", "Expired", "On Hold" };

            return View(await query.OrderByDescending(c => c.StartDate).ToListAsync());
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)           // loads the related Client
                .Include(c => c.ServiceRequests)  // loads related ServiceRequests
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound();

            return View(contract);
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
                return NotFound();

            // Dropdown needs the list of clients
            ViewBag.Clients = await _context.Clients.ToListAsync();
            ViewBag.Statuses = new[] { "Draft", "Active", "Expired", "On Hold" };
            ViewBag.ServiceLevels = new[] { "Basic", "Standard", "Premium" };

            return View(contract);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract, IFormFile? file)
        {
            if (id != contract.Id)
                return NotFound();

            var existing = await _context.Contracts.FindAsync(id);

            if (existing == null)
                return NotFound();

            // Handle new file upload if provided
            if (file != null && file.Length > 0)
            {
                if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
                {
                    ModelState.AddModelError("", "Only PDF files are allowed.");
                    ViewBag.Clients = await _context.Clients.ToListAsync();
                    ViewBag.Statuses = new[] { "Draft", "Active", "Expired", "On Hold" };
                    ViewBag.ServiceLevels = new[] { "Basic", "Standard", "Premium" };
                    return View(contract);
                }

                string uploadsFolder = Path.Combine(_environment.WebRootPath, "Uploads", "Contracts");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid().ToString() + ".pdf";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                existing.AgreementFilePath = "Uploads/Contracts/" + fileName;
            }

            // Update the other fields
            existing.ClientId = contract.ClientId;
            existing.StartDate = contract.StartDate;
            existing.EndDate = contract.EndDate;
            existing.Status = contract.Status;
            existing.ServiceLevel = contract.ServiceLevel;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound();

            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract != null)
            {
                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}