using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebPhone.Areas.Admins.Models.Roles;
using WebPhone.Areas.Products.Models.Products;
using WebPhone.EF;

namespace WebPhone.Areas.Admins.Controllers
{
    [Area("Admins")]
    [Route("/admin/role")]
    public class RolesController : Controller
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _context;

        public RolesController
            (
                AppDbContext context,
                ILogger<RolesController> logger

            )
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles.ToListAsync();
            return View(roles);
        }

        [HttpGet("details")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Roles == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleDTO roleDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                    return View(roleDTO);
                }

                var role = new Role
                {
                    RoleName = roleDTO.RoleName,
                };

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Thêm mới quyền thành công";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                    {
                        TempData["Message"] = "Error: Tên quyền đã được sử dụng";
                        return View(roleDTO);
                    }
                }

                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return View(roleDTO);
            }
        }

        [HttpGet("edit")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin quyền";
                return RedirectToAction(nameof(Index));
            }

            var roleDTO = new RoleDTO
            {
                Id = role.Id,
                RoleName = role.RoleName,
            };

            return View(roleDTO);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, RoleDTO roleDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                    return View(roleDTO);
                }

                var role = await _context.Roles.FindAsync(roleDTO.Id);
                if(role == null)
                {
                    TempData["Message"] = "Error: Không tìm thấy thông tin quyền";
                    return View(roleDTO);
                }

                role.RoleName = roleDTO.RoleName;
                role.UpdateAt = DateTime.UtcNow;

                _context.Roles.Update(role);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Cập nhật thành công";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                    {
                        TempData["Message"] = "Error: Tên quyền đã được sử dụng";
                        return View(roleDTO);
                    }
                }

                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return View(roleDTO);
            }
        }

        [HttpGet("delete")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin";
                return RedirectToAction(nameof(Index));
            }

            return View(role);
        }

        [HttpPost("delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null)
                {
                    TempData["Message"] = "Error: Không tìm thấy thông tin";
                    return RedirectToAction(nameof(Index));
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Xóa quyền thành công";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return View();
            }
        }

        private bool RoleExists(Guid id)
        {
            return (_context.Roles?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
