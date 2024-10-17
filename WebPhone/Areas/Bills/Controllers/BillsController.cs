using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPhone.Areas.Bills.Models.Bills;
using WebPhone.EF;

namespace WebPhone.Areas.Bills.Controllers
{
    [Area("Bills")]
    [Route("/bill/")]
    [Authorize]
    public class BillsController : Controller
    {
        private readonly ILogger<BillsController> _logger;
        private readonly AppDbContext _context;

        public BillsController(ILogger<BillsController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Bills.Include(b => b.Customer).Include(b => b.Employment);
            return View(await appDbContext.ToListAsync());
        }

        [HttpGet("details")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Bills == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .Include(b => b.Customer)
                .Include(b => b.Employment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            ViewData["Customers"] = await _context.Users.Take(100).ToListAsync();
            ViewData["Products"] = await _context.Products.Take(100).ToListAsync();
            return View();
        }

        [HttpPost("create")]
        public async Task<JsonResult> Create(BillDTO billDTO)
        {
            try
            {
                var customer = await _context.Users.FirstOrDefaultAsync(u => u.Id == billDTO.CustomerId);
                if (customer == null)
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin khách hàng"
                    });

                // Lấy thông tin nhân viên
                var email = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var employment = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (employment == null)
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin nhân viên"
                    });

                if (billDTO.ProductId.Count != billDTO.Quantities.Count)
                    return Json(new
                    {
                        Success = false,
                        Message = "Dữ liệu số lượng sản phẩm không hợp lệ"
                    });

                var products = new List<BillProductDTO>();

                int price = 0;
                int discount = 0;
                int totalPrice = 0;

                // Tính tổng tiền sản phẩm
                for (int i = 0; i < billDTO.ProductId.Count; i++)
                {
                    var product = await _context.Products.FindAsync(billDTO.ProductId[i]);
                    if (product == null)
                        return Json(new
                        {
                            Success = false,
                            Message = "Không tìm thấy thông tin sản phẩm"
                        });

                    var billProduct = new BillProductDTO
                    {
                        Product = product,
                        Quantity = billDTO.Quantities[i]
                    };

                    products.Add(billProduct);
                    price += (product.Discount ?? product.Price) * billDTO.Quantities[i];
                }

                // Tính tiền giảm giá
                // Nếu kiểu giảm giá là phần trăm
                if (billDTO.DiscountStyle == 0)
                {
                    discount = (int)Math.Ceiling((double)price / 100 * billDTO.DiscountValue / 1000) * 1000;
                }
                // Nếu kiểu giảm giá là số tiền
                if (billDTO.DiscountStyle == 1)
                {
                    discount = (int)Math.Ceiling((double)billDTO.DiscountValue / 1000) * 1000;
                }

                discount = discount > price ? price : discount;

                totalPrice = price - discount;

                var bill = new Bill
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    CustomerName = customer.UserName,
                    Price = price,
                    Discount = discount,
                    TotalPrice = totalPrice,
                    PaymentPrice = billDTO.PaymentValue > totalPrice ? totalPrice : billDTO.PaymentValue,
                    EmploymentId = employment.Id,
                    EmploymentName = employment.UserName
                };

                _context.Bills.Add(bill);

                foreach (var item in products)
                {
                    var billInfo = new BillInfo
                    {
                        ProductId = item.Product.Id,
                        ProductName = item.Product.ProductName,
                        Quantity = item.Quantity,
                        Price = item.Product.Price,
                        Discount = item.Product.Discount,
                        BillId = bill.Id,
                    };

                    _context.BillInfos.Add(billInfo);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    Success = true,
                    Message = "Success",
                    Data = Url.Action(nameof(Export), new { id = bill.Id })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new
                {
                    Success = false,
                    Message = "Lỗi hệ thống"
                });
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export(Guid id)
        {
            var bill = await _context.Bills
                        .Include(b => b.Customer)
                        .Include(b => b.BillInfos)
                        .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null)
            {
                TempData["Message"] = "Error: Không tìm thấy hóa đơn";
                RedirectToAction(nameof(Create));
            }

            return View(bill);
        }

        [HttpGet("edit")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Bills == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "Id", bill.CustomerId);
            ViewData["EmploymentId"] = new SelectList(_context.Users, "Id", "Id", bill.EmploymentId);
            return View(bill);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,CustomerId,EmploymentId,CustomerName,EmploymentName,Price,Discount,TotalPrice,CreateAt,UpdateAt")] Bill bill)
        {
            if (id != bill.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bill);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillExists(bill.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "Id", bill.CustomerId);
            ViewData["EmploymentId"] = new SelectList(_context.Users, "Id", "Id", bill.EmploymentId);
            return View(bill);
        }

        [HttpGet("delete")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Bills == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .Include(b => b.Customer)
                .Include(b => b.Employment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }

        [HttpPost("delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Bills == null)
            {
                return Problem("Entity set 'AppDbContext.Bills'  is null.");
            }
            var bill = await _context.Bills.FindAsync(id);
            if (bill != null)
            {
                _context.Bills.Remove(bill);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BillExists(Guid id)
        {
            return (_context.Bills?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
