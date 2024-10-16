using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPhone.EF;

namespace WebPhone.Areas.Bills.Controllers
{
    [Area("Bills")]
    [Route("/bill/")]
    [Authorize]
    public class BillsController : Controller
    {
        private readonly AppDbContext _context;

        public BillsController(AppDbContext context)
        {
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
            ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["Products"] = await _context.Products.Take(100).ToListAsync();
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerId,EmploymentId,CustomerName,EmploymentName,Price,Discount,TotalPrice,CreateAt,UpdateAt")] Bill bill)
        {
            if (ModelState.IsValid)
            {
                bill.Id = Guid.NewGuid();
                _context.Add(bill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "Id", bill.CustomerId);
            ViewData["EmploymentId"] = new SelectList(_context.Users, "Id", "Id", bill.EmploymentId);
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
