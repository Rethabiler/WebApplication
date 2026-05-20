using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class ClientController : Controller
    {
      
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
          
            var clients = await _context.Clients.ToListAsync();
            return View(clients);
        }

        // ──────────────────────────────────────────────────────────────
        // DETAILS — show one client + their contracts
        // URL: /Clients/Details/5
        // ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
           
            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.Id == id);

            // Return 404 if the ID doesn't exist
            if (client == null)
                return NotFound();

            return View(client);
        }

       
        public IActionResult Create()
        {
            
            return View();
        }

        // ──────────────────────────────────────────────────────────────
        // CREATE (POST) — receive and save the form
        // URL: /Clients/Create  [POST]
        // ──────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Create(Client client)
        {
            
            ModelState.Remove("Contracts");

            if (!ModelState.IsValid)
            {
                
                return View(client);
            }

            _context.Clients.Add(client);       
            await _context.SaveChangesAsync();   

          
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
            // the ID in the URL must match the form data
            if (id != client.Id)
                return NotFound();

            ModelState.Remove("Contracts");

            if (!ModelState.IsValid)
                return View(client);

          
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

            
            return View(client);
        }

       
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
