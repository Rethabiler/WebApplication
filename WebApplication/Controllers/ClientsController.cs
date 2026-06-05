using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    public class ClientsController : Controller
    {
        // No more _context — we now call the API instead of the DB directly
        private readonly IGlmsApiService _apiService;

        public ClientsController(IGlmsApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Clients
        public async Task<IActionResult> Index(string? search)
        {
            var clients = await _apiService.GetClientsAsync(search);
            ViewBag.Search = search;
            return View(clients);
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var client = await _apiService.GetClientAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            ViewBag.Regions = new[] { "Western Cape", "Gauteng", "KwaZulu-Natal", "Eastern Cape", "Limpopo", "Mpumalanga", "North West", "Free State", "Northern Cape" };
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            ModelState.Remove("Contracts");

            if (!ModelState.IsValid)
            {
                ViewBag.Regions = new[] { "Western Cape", "Gauteng", "KwaZulu-Natal", "Eastern Cape", "Limpopo", "Mpumalanga", "North West", "Free State", "Northern Cape" };
                return View(client);
            }

            var success = await _apiService.CreateClientAsync(client);

            if (!success)
            {
                ModelState.AddModelError("", "Failed to create client. Please try again.");
                ViewBag.Regions = new[] { "Western Cape", "Gauteng", "KwaZulu-Natal", "Eastern Cape", "Limpopo", "Mpumalanga", "North West", "Free State", "Northern Cape" };
                return View(client);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _apiService.GetClientAsync(id);
            if (client == null) return NotFound();

            ViewBag.Regions = new[] { "Western Cape", "Gauteng", "KwaZulu-Natal", "Eastern Cape", "Limpopo", "Mpumalanga", "North West", "Free State", "Northern Cape" };
            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id) return NotFound();

            ModelState.Remove("Contracts");

            if (!ModelState.IsValid)
            {
                ViewBag.Regions = new[] { "Western Cape", "Gauteng", "KwaZulu-Natal", "Eastern Cape", "Limpopo", "Mpumalanga", "North West", "Free State", "Northern Cape" };
                return View(client);
            }

            var success = await _apiService.UpdateClientAsync(id, client);

            if (!success)
            {
                ModelState.AddModelError("", "Failed to update client. Please try again.");
                ViewBag.Regions = new[] { "Western Cape", "Gauteng", "KwaZulu-Natal", "Eastern Cape", "Limpopo", "Mpumalanga", "North West", "Free State", "Northern Cape" };
                return View(client);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _apiService.GetClientAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiService.DeleteClientAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
