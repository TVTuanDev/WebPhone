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
                var payment = billByUser.Sum(b => b.PaymentPrice);
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

            return View(customer);
        }
    }
}
