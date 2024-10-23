using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebPhone.Areas.Bills.Models.Bills;
using WebPhone.Attributes;
using WebPhone.EF;

namespace WebPhone.Areas.Bills.Controllers
{
    [Area("Bills")]
    [Route("/bill/debt/")]
    [AppAuthorize("Administrator, Manager, Employment")]
    public class DebtsController : Controller
    {
        private readonly ILogger<DebtsController> _logger;
        private readonly AppDbContext _context;

        public DebtsController
            (
                ILogger<DebtsController> logger,
                AppDbContext context
            )
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var bill = await _context.Bills.Where(b => b.TotalPrice - b.PaymentPrice > 0).ToListAsync();

            return View(bill);
        }

        [HttpGet("edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var bill = await _context.Bills
                        .Include(b => b.Customer)
                        .Include(b => b.BillInfos)
                        .FirstOrDefaultAsync(b => b.Id == id);
            if (bill == null)
            {
                TempData["Message"] = "Error: Không tìm thấy hóa đơn";
                RedirectToAction(nameof(Index));
            }
            return View(bill);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, string payment)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin hóa đơn";
                return RedirectToAction(nameof(Edit), new { id });
            }

            string stringPayment = Regex.Replace(payment, @"\D", "");
            int.TryParse(stringPayment, out int paymentValue);

            var paymentLog = new PaymentLog
            {
                Price = paymentValue,
                CustomerId = bill.CustomerId ?? Guid.Empty,
                BillId = bill.Id
            };

            _context.PaymentLogs.Add(paymentLog);

            await _context.SaveChangesAsync();

            TempData["Message"] = "Success: Cập nhật công nợ thành công";

            return RedirectToAction(nameof(Index));
        }
    }
}
