using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPhone.Areas.Accounts.Models.Manager;
using WebPhone.EF;
using WebPhone.Models;

namespace WebPhone.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    [Route("/customer/")]
    [Authorize]
    public class ManagerController : Controller
    {
        private readonly ILogger<ManagerController> _logger;
        private readonly AppDbContext _context;

        public ManagerController
            (
                ILogger<ManagerController> logger,
                AppDbContext context
            )
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("info")]
        public async Task<IActionResult> InfoCustomer()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (email == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin";
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin";
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        [HttpGet("change-password")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost("change-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            try
            {
                var user = await GetUserLogin();
                if (user == null)
                {
                    TempData["Message"] = "Error: Không tìm thấy người dùng";
                    return RedirectToAction(nameof(InfoCustomer));
                }

                if (!PasswordManager.VerifyPassword(changePasswordDTO.OldPassword, user.PasswordHash))
                {
                    TempData["Message"] = "Error: Mật khẩu không chính xác";
                    return RedirectToAction(nameof(InfoCustomer));
                }

                user.PasswordHash = PasswordManager.HashPassword(changePasswordDTO.NewPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Cập nhật mật khẩu thành công";

                return RedirectToAction(nameof(InfoCustomer));
            }
            catch (Exception ex)
            {
               _logger.LogError(ex.Message);

                TempData["Message"] = "Error: Lỗi hệ thống";
                return RedirectToAction(nameof(InfoCustomer));
            }
        }

        private async Task<User> GetUserLogin()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user ?? new User();
        }
    }
}
