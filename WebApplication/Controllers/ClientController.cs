using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class ClientController : Controller
    {
        // _context is our database connection — injected automatically by ASP.NET
        private readonly ApplicationDbContext _context;

        public ClientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ──────────────────────────────────────────────────────────────
        // INDEX — show all clients
        // URL: /Clients
        // ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            // .ToListAsync() runs: SELECT * FROM Clients
            var clients = await _context.Clients.ToListAsync();
            return View(clients);
        }

        // ──────────────────────────────────────────────────────────────
        // DETAILS — show one client + their contracts
        // URL: /Clients/Details/5
        // ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            // .Include() = SQL JOIN — loads the related Contracts too
            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.Id == id);

            // Return 404 if the ID doesn't exist
            if (client == null)
                return NotFound();

            return View(client);
        }

        // ──────────────────────────────────────────────────────────────
        // CREATE (GET) — show the empty form
        // URL: /Clients/Create
        // ──────────────────────────────────────────────────────────────
        public IActionResult Create()
        {
            // No async needed — we're not touching the DB here,
            // just showing a blank form
            return View();
        }

        // ──────────────────────────────────────────────────────────────
        // CREATE (POST) — receive and save the form
        // URL: /Clients/Create  [POST]
        // ──────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken] // prevents cross-site request forgery attacks
        public async Task<IActionResult> Create(Client client)
        {
            // ModelState.IsValid checks all [Required] attributes on the model.
            // We remove "Contracts" because that navigation property is never
            // submitted by the form, so it would always fail validation.
            ModelState.Remove("Contracts");

            if (!ModelState.IsValid)
            {
                // Something was wrong (e.g. empty Name field).
                // Return the same view so the user sees the error messages.
                return View(client);
            }

            _context.Clients.Add(client);       // stages the INSERT
            await _context.SaveChangesAsync();   // executes it

            // PRG pattern: redirect after POST so refreshing doesn't re-submit
            return RedirectToAction(nameof(Index));
        }

        // ──────────────────────────────────────────────────────────────
        // EDIT (GET) — load the client and show a pre-filled form
        // URL: /Clients/Edit/5
        // ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
                return NotFound();

            return View(client);
        }

        // ──────────────────────────────────────────────────────────────
        // EDIT (POST) — save the changes
        // URL: /Clients/Edit/5  [POST]
        // ──────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            // Safety check: the ID in the URL must match the form data
            if (id != client.Id)
                return NotFound();

            ModelState.Remove("Contracts");

            if (!ModelState.IsValid)
                return View(client);

            // _context.Update() marks ALL properties as modified → generates UPDATE
            _context.Update(client);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ──────────────────────────────────────────────────────────────
        // DELETE (GET) — show a confirmation page
        // URL: /Clients/Delete/5
        // ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
                return NotFound();

            // We only show the record — actual deletion happens on POST
            return View(client);
        }

        // ──────────────────────────────────────────────────────────────
        // DELETE (POST) — perform the actual deletion
        // ActionName("Delete") means the URL still looks like /Clients/Delete/5
        // but the method is named DeleteConfirmed to avoid a C# naming conflict
        // ──────────────────────────────────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}