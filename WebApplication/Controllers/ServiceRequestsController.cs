using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly IGlmsApiService _apiService;

        public ServiceRequestsController(IGlmsApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index()
        {
            var requests = await _apiService.GetServiceRequestsAsync();
            return View(requests);
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var sr = await _apiService.GetServiceRequestAsync(id);
            if (sr == null) return NotFound();
            return View(sr);
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create()
        {
            var contracts = await _apiService.GetContractsAsync();
            var validContracts = contracts
                .Where(c => c.Status != "Expired" && c.Status != "On Hold")
                .ToList();

            ViewBag.Contracts = validContracts;
            ViewBag.Statuses = new[] { "Pending", "In Progress", "Completed", "Cancelled" };

            return View();
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest serviceRequest)
        {
            ModelState.Remove("Contract");

            if (!ModelState.IsValid)
            {
                var contracts = await _apiService.GetContractsAsync();
                ViewBag.Contracts = contracts.Where(c => c.Status != "Expired" && c.Status != "On Hold").ToList();
                ViewBag.Statuses = new[] { "Pending", "In Progress", "Completed", "Cancelled" };
                return View(serviceRequest);
            }

            var success = await _apiService.CreateServiceRequestAsync(serviceRequest);

            if (!success)
            {
                ModelState.AddModelError("", "Failed to create service request. The contract may be Expired or On Hold.");
                var contracts = await _apiService.GetContractsAsync();
                ViewBag.Contracts = contracts.Where(c => c.Status != "Expired" && c.Status != "On Hold").ToList();
                ViewBag.Statuses = new[] { "Pending", "In Progress", "Completed", "Cancelled" };
                return View(serviceRequest);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceRequests/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var sr = await _apiService.GetServiceRequestAsync(id);
            if (sr == null) return NotFound();
            return View(sr);
        }

        // POST: ServiceRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiService.DeleteServiceRequestAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}