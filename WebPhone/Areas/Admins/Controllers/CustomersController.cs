using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPhone.Areas.Admins.Models.Customers;
using WebPhone.Attributes;
using WebPhone.EF;

namespace WebPhone.Areas.Admins.Controllers
{
    [Area("Admins")]
    [Route("/admin/customer/")]
    [AppAuthorize("Administrator")]
    public class CustomersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly int ITEM_PER_PAGE = 10;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            var users = await (from u in _context.Users
                               where !(from ur in _context.UserRoles
                                       select ur.UserId).Contains(u.Id)
                               select u).ToListAsync();

            var userList = new List<User>();
            int countPage;
            page = page < 1 ? 1 : page;

            // Có query truyền vào
            if (!string.IsNullOrEmpty(q))
            {
                int total = users.Where(u => u.Email.Contains(q)).Count();
                countPage = (int)Math.Ceiling((double)total / ITEM_PER_PAGE);
                countPage = countPage < 1 ? 1 : countPage;
                page = page > countPage ? countPage : page;
                userList = users.Where(u => u.Email.Contains(q))
                            .Skip((page - 1) * ITEM_PER_PAGE)
                            .Take(ITEM_PER_PAGE)
                            .Select(u => new User
                            {
                                Id = u.Id,
                                UserName = u.UserName,
                                Email = u.Email,
                                EmailConfirmed = u.EmailConfirmed,
                                CreateAt = u.CreateAt,
                                UpdateAt = u.UpdateAt,
                            }).ToList();
            }
            else
            {
                int total = users.Count();
                countPage = (int)Math.Ceiling((double)total / ITEM_PER_PAGE);
                countPage = countPage < 1 ? 1 : countPage;
                page = page > countPage ? countPage : page;
                userList = users.Skip((page - 1) * ITEM_PER_PAGE)
                            .Take(ITEM_PER_PAGE)
                            .Select(u => new User
                            {
                                Id = u.Id,
                                UserName = u.UserName,
                                Email = u.Email,
                                EmailConfirmed = u.EmailConfirmed,
                                CreateAt = u.CreateAt,
                                UpdateAt = u.UpdateAt,
                            }).ToList();
            }

            countPage = countPage < 1 ? 1 : countPage;
            ViewBag.CountPage = countPage;

            var customers = new List<CustomerDebtDTO>();

            foreach (var user in userList)
            {
                var billByUser = await _context.Bills.Where(b => b.CustomerId == user.Id).ToListAsync();
                var total = billByUser.Sum(b => b.TotalPrice);
                var payment = await _context.PaymentLogs.Where(p => p.CustomerId == user.Id).SumAsync(p => p.Price);
                var debt = total - payment;
                debt = debt < 0 ? 0 : debt;

                var customerDebt = new CustomerDebtDTO
                {
                    Customer = user,
                    Debt = debt,
                };

                customers.Add(customerDebt);
            }

            return View(customers);
        }

        [HttpGet("details")]
        public async Task<IActionResult> Details(Guid id)
        {
            var customer = await _context.Users
                            .Include(u => u.CustomerBills)
                            .Include(u => u.PaymentLogs)
                            .FirstOrDefaultAsync(u => u.Id == id);

            if(customer == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin khách hàng";
                return RedirectToAction(nameof(Index));
            }

            var customerBills = new List<CustomerBillDTO>();

            foreach (var bill in customer.CustomerBills)
            {
                var customerBill = new CustomerBillDTO
                {
                    BillDate = bill.CreateAt,
                    TotalPrice = bill.TotalPrice,
                    PaymentPrice = bill.PaymentPrice,
                    BillId = bill.Id
                };

                customerBills.Add(customerBill);
            }

            var paymentLogs = customer.PaymentLogs.OrderBy(p => p.CreateAt).ToList();
            if (paymentLogs.Count > 0) paymentLogs.RemoveAt(0);

            foreach (var payment in paymentLogs)
            {
                var customerBill = new CustomerBillDTO
                {
                    BillDate = payment.CreateAt,
                    PaymentPrice = payment.Price,
                    IsPayment = true,
                    BillId = payment.BillId
                };

                customerBills.Add(customerBill);
            }

            ViewBag.CustomerBills = customerBills.OrderByDescending(cb => cb.BillDate);

            return View(customer);
        }

        [HttpGet("bill/details")]
        public async Task<IActionResult> DetailBill(Guid id)
        {
            var bill = await _context.Bills
                        .Include(b => b.Customer)
                        .Include(b => b.BillInfos)
                        .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null)
            {
                TempData["Message"] = "Error: Không tìm thấy hóa đơn";
                return RedirectToAction(nameof(Index));
            }

            var paymentLogs = await _context.PaymentLogs
                                .Where(p => p.BillId == bill.Id)
                                .ToListAsync();

            bill.PaymentPrice = paymentLogs.Sum(p => p.Price);

            ViewBag.PaymentLogs = paymentLogs.OrderByDescending(p => p.CreateAt);

            return View(bill);
        }
    }
}
