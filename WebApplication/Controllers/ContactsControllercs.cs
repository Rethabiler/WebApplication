using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    public class ContractsController : Controller
    {
        private readonly IGlmsApiService _apiService;
        private readonly IWebHostEnvironment _environment;

        public ContractsController(IGlmsApiService apiService, IWebHostEnvironment environment)
        {
            _apiService = apiService;
            _environment = environment;
        }

        // GET: Contracts
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string? status)
        {
            var contracts = await _apiService.GetContractsAsync(status, startDate, endDate);

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;
            ViewBag.Statuses = new[] { "Draft", "Active", "Expired", "On Hold" };

            return View(contracts);
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var contract = await _apiService.GetContractAsync(id);
            if (contract == null) return NotFound();
            return View(contract);
        }

        // GET: Contracts/Create
        public async Task<IActionResult> Create()
        {
            var clients = await _apiService.GetClientsAsync();
            ViewBag.Clients = clients;
            ViewBag.Statuses = new[] { "Draft", "Active", "Expired", "On Hold" };
            ViewBag.ServiceLevels = new[] { "Basic", "Standard", "Premium" };
            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract, IFormFile? file)
        {
            if (file != null && file.Length > 0)
            {
                if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
                {
                    ModelState.AddModelError("", "Only PDF files are allowed.");
                    ViewBag.Clients = await _apiService.GetClientsAsync();
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

                contract.AgreementFilePath = "Uploads/Contracts/" + fileName;
            }

            ModelState.Remove("Client");
            ModelState.Remove("ServiceRequests");
            ModelState.Remove("AgreementFilePath");

            if (!ModelState.IsValid)
            {
                ViewBag.Clients = await _apiService.GetClientsAsync();
                ViewBag.Statuses = new[] { "Draft", "Active", "Expired", "On Hold" };
                ViewBag.ServiceLevels = new[] { "Basic", "Standard", "Premium" };
                return View(contract);
            }

            var success = await _apiService.CreateContractAsync(contract);
            if (!success)
            {
                ModelState.AddModelError("", "Failed to create contract.");
                ViewBag.Clients = await _apiService.GetClientsAsync();
                ViewBag.Statuses = new[] { "Draft", "Active", "Expired", "On Hold" };
                ViewBag.ServiceLevels = new[] { "Basic", "Standard", "Premium" };
                return View(contract);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _apiService.GetContractAsync(id);
            if (contract == null) return NotFound();

            ViewBag.Clients = await _apiService.GetClientsAsync();
            ViewBag.Statuses = new[] { "Draft", "Active", "Expired", "On Hold" };
            ViewBag.ServiceLevels = new[] { "Basic", "Standard", "Premium" };
            return View(contract);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract, IFormFile? file)
        {
            if (id != contract.Id) return NotFound();

            if (file != null && file.Length > 0)
            {
                if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
                {
                    ModelState.AddModelError("", "Only PDF files are allowed.");
                    ViewBag.Clients = await _apiService.GetClientsAsync();
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

                contract.AgreementFilePath = "Uploads/Contracts/" + fileName;
            }

            await _apiService.UpdateContractStatusAsync(id, contract.Status);
            return RedirectToAction(nameof(Index));
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _apiService.GetContractAsync(id);
            if (contract == null) return NotFound();
            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiService.DeleteContractAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Download PDF
        public IActionResult Download(string fileName)
        {
            string path = Path.Combine(_environment.WebRootPath, "Uploads", "Contracts", fileName);
            if (!System.IO.File.Exists(path)) return NotFound();

            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/pdf", fileName);
        }
    }
}