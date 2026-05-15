using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Models;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    using WebApplication.Services;

    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;

        public ServiceRequestsController(ApplicationDbContext context, ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        // ──────────────────────────────────────────────────────────────
        // INDEX — list all service requests
        // URL: /ServiceRequests
        // ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            // Include Contract and its Client so we can display names
            var requests = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c!.Client)
                .ToListAsync();

            return View(requests);
        }

        // ──────────────────────────────────────────────────────────────
        // DETAILS — show one service request
        // URL: /ServiceRequests/Details/5
        // ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            var sr = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sr == null) return NotFound();

            return View(sr);
        }

        // ──────────────────────────────────────────────────────────────
        // CREATE (GET) — show the form
        // Only shows contracts that are NOT Expired or On Hold
        // URL: /ServiceRequests/Create
        // ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Create()
        {
            // ── WORKFLOW ENFORCEMENT ──
            // Only Active and Draft contracts can have service requests
            decimal rate = await _currencyService.GetUsdToZarRateAsync();
            ViewBag.UsdToZarRate = rate;

            var validContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status != "Expired" && c.Status != "On Hold")
                .ToListAsync();

            ViewBag.Contracts = validContracts;
            ViewBag.Statuses = new[] { "Pending", "In Progress", "Completed", "Cancelled" };

            return View();
        }

        // ──────────────────────────────────────────────────────────────
        // CREATE (POST) — save the service request
        // URL: /ServiceRequests/Create  [POST]
        // ──────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest serviceRequest)
        {
            // ── WORKFLOW ENFORCEMENT (server-side) ──
            // Always re-check on POST — never trust the client alone
            var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);

            if (contract == null)
            {
                ModelState.AddModelError("ContractId", "Selected contract does not exist.");
            }
            else if (contract.Status == "Expired" || contract.Status == "On Hold")
            {
                ModelState.AddModelError("",
                    $"Cannot create a Service Request: the contract is '{contract.Status}'.");
            }

            ModelState.Remove("Contract");

            if (!ModelState.IsValid)
            {
                ViewBag.Contracts = await _context.Contracts
                    .Include(c => c.Client)
                    .Where(c => c.Status != "Expired" && c.Status != "On Hold")
                    .ToListAsync();
                ViewBag.Statuses = new[] { "Pending", "In Progress", "Completed", "Cancelled" };
                return View(serviceRequest);
            }

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ──────────────────────────────────────────────────────────────
        // EDIT (GET)
        // URL: /ServiceRequests/Edit/5
        // ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);
            if (sr == null) return NotFound();

            ViewBag.Contracts = await _context.Contracts
                .Include(c => c.Client)
                .ToListAsync();
            ViewBag.Statuses = new[] { "Pending", "In Progress", "Completed", "Cancelled" };

            return View(sr);
        }

        // ──────────────────────────────────────────────────────────────
        // EDIT (POST)
        // URL: /ServiceRequests/Edit/5  [POST]
        // ──────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.Id) return NotFound();

            ModelState.Remove("Contract");

            if (!ModelState.IsValid)
            {
                ViewBag.Contracts = await _context.Contracts
                    .Include(c => c.Client)
                    .ToListAsync();
                ViewBag.Statuses = new[] { "Pending", "In Progress", "Completed", "Cancelled" };
                return View(serviceRequest);
            }

            _context.Update(serviceRequest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ──────────────────────────────────────────────────────────────
        // DELETE (GET) — confirmation page
        // URL: /ServiceRequests/Delete/5
        // ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Delete(int id)
        {
            var sr = await _context.ServiceRequests
                .Include(s => s.Contract)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sr == null) return NotFound();

            return View(sr);
        }

        // ──────────────────────────────────────────────────────────────
        // DELETE (POST) — perform deletion
        // URL: /ServiceRequests/Delete/5  [POST]
        // ──────────────────────────────────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);
            if (sr != null)
            {
                _context.ServiceRequests.Remove(sr);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}