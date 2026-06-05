using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using WebApplication.Data;
using WebApplication.Models;

namespace WebApplication.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All endpoints require a valid JWT token
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── GET PAYMENT HISTORY ──
        // GET: api/payments
        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            // Get the logged-in user's ID from JWT claims
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            // Admins see all payments, employees see only their own
            var query = _context.Payments
                .Include(p => p.AppUser)
                .AsQueryable();

            if (role != "Admin")
                query = query.Where(p => p.AppUserId == userId);

            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.Recipient,
                    p.Bank,
                    p.Currency,
                    p.Amount,
                    p.SwiftCode,
                    p.Status,
                    p.CreatedAt,
                    CreatedBy = p.AppUser!.FullName
                })
                .ToListAsync();

            return Ok(payments);
        }

        // ── SUBMIT A PAYMENT ──
        // POST: api/payments
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            // ── INPUT WHITELISTING WITH REGEX ──

            // Recipient: letters and spaces only
            if (!Regex.IsMatch(request.Recipient, @"^[a-zA-Z\s]{2,100}$"))
                return BadRequest(new { message = "Invalid recipient name." });

            // Bank: letters, spaces, and ampersand
            if (!Regex.IsMatch(request.Bank, @"^[a-zA-Z\s&]{2,100}$"))
                return BadRequest(new { message = "Invalid bank name." });

            // Currency: exactly 3 uppercase letters (e.g. USD, EUR, ZAR)
            if (!Regex.IsMatch(request.Currency, @"^[A-Z]{3}$"))
                return BadRequest(new { message = "Currency must be a 3-letter code (e.g. USD, EUR, ZAR)." });

            // Amount: must be positive
            if (request.Amount <= 0)
                return BadRequest(new { message = "Amount must be greater than zero." });

            // SWIFT code: 8 or 11 alphanumeric characters
            if (!Regex.IsMatch(request.SwiftCode, @"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$"))
                return BadRequest(new { message = "Invalid SWIFT code format." });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var payment = new Payment
            {
                AppUserId = userId,
                Recipient = request.Recipient,
                Bank = request.Bank,
                Currency = request.Currency,
                Amount = request.Amount,
                SwiftCode = request.SwiftCode,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Payment submitted successfully.", payment.Id });
        }

        // ── GET SINGLE PAYMENT ──
        // GET: api/payments/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            var payment = await _context.Payments
                .Include(p => p.AppUser)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return NotFound(new { message = "Payment not found." });

            // Employees can only see their own payments
            if (role != "Admin" && payment.AppUserId != userId)
                return Forbid();

            return Ok(new
            {
                payment.Id,
                payment.Recipient,
                payment.Bank,
                payment.Currency,
                payment.Amount,
                payment.SwiftCode,
                payment.Status,
                payment.CreatedAt,
                CreatedBy = payment.AppUser!.FullName
            });
        }
    }

    // ── REQUEST MODEL ──
    public class CreatePaymentRequest
    {
        public string Recipient { get; set; } = string.Empty;
        public string Bank { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string SwiftCode { get; set; } = string.Empty;
    }
}
