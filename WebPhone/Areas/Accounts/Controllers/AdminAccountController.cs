using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebPhone.Areas.Accounts.Models.AdminAccount;
using WebPhone.EF;
using WebPhone.Models;

namespace WebPhone.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    [Route("/admin/users/")]
    [Authorize]
    public class AdminAccountController : Controller
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _context;

        private readonly int ITEM_PER_PAGE = 10;

        public AdminAccountController
            (
                ILogger<AdminAccountController> logger,
                AppDbContext context
            )
        {
            _logger = logger;
            _context = context;
        }

        #region CURD User
        [HttpGet]
        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            var userList = new List<User>();
            int countPage;
            page = page < 1 ? 1 : page;

            // Có query truyền vào
            if (!string.IsNullOrEmpty(q))
            {
                int total = await _context.Users.Where(u => u.Email.Contains(q)).CountAsync();
                countPage = (int)Math.Ceiling((double)total / ITEM_PER_PAGE);
                countPage = countPage < 1 ? 1 : countPage;
                page = page > countPage ? countPage : page;
                userList = await _context.Users
                            .Where(u => u.Email.Contains(q))
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
                            }).ToListAsync();
            }
            else
            {
                int total = await _context.Users.CountAsync();
                countPage = (int)Math.Ceiling((double)total / ITEM_PER_PAGE);
                countPage = countPage < 1 ? 1 : countPage;
                page = page > countPage ? countPage : page;
                userList = await  _context.Users
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
                            }).ToListAsync();
            }

            countPage = countPage < 1 ? 1 : countPage;
            ViewBag.CountPage = countPage;

            return View(userList);
        }

        [HttpGet("details")]
        public async Task<IActionResult> Details(Guid? id)
        {
            var user = await _context.Users.Select(u => new User
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                EmailConfirmed = u.EmailConfirmed,
                CreateAt = u.CreateAt,
                UpdateAt = u.UpdateAt,
            }).FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                TempData["Message"] = "Error: Không tìm thấy người dùng";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserDTO userDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                    return View(userDTO);
                }

                if (userDTO.Password == null)
                {
                    TempData["Message"] = "Error: Mật khẩu bắt buộc nhập";
                    return View(userDTO);
                }

                var user = new User
                {
                    UserName = userDTO.UserName,
                    Email = userDTO.Email,
                    EmailConfirmed = true,
                    PasswordHash = PasswordManager.HashPassword(userDTO.Password)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Thêm mới tài khoản thành công";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return View(userDTO);
            }
        }

        [HttpGet("edit")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin người dùng";
                return RedirectToAction(nameof(Index));
            }

            var userDTO = new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
            };

            return View(userDTO);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UserDTO userDTO)
        {
            try
            {
                if (id != userDTO.Id)
                {
                    TempData["Message"] = "Error: Thông tin không hợp lệ";
                    return View(userDTO);
                }

                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                    return View(userDTO);
                }

                var user = await _context.Users.FindAsync(userDTO.Id);
                if (user == null)
                {
                    TempData["Message"] = "Error: Không tìm thấy thông tin người dùng";
                    return View(userDTO);
                }

                user.UserName = userDTO.UserName;
                user.EmailConfirmed = userDTO.EmailConfirmed;
                user.UpdateAt = DateTime.UtcNow;

                // Có sự thay đổi về email
                if (user.Email != userDTO.Email)
                {
                    // Kiểm tra xem email mới đã được đăng ký chưa
                    var userByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDTO.Email);
                    if (userByEmail != null)
                    {
                        TempData["Message"] = "Error: Email đã được sử dụng";
                        return View(userDTO);
                    }

                    user.Email = userDTO.Email;
                }

                // Admin đổi password mới
                if (userDTO.Password != null)
                {
                    user.PasswordHash = PasswordManager.HashPassword(userDTO.Password);
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Cập nhật thông tin thành công";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return View(userDTO);
                throw;
            }
        }

        [HttpGet("delete")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            var user = await _context.Users
                .Select(u => new User
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    EmailConfirmed = u.EmailConfirmed,
                    CreateAt = u.CreateAt,
                    UpdateAt = u.UpdateAt
                })
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin người dùng";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        [HttpPost("delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin người dùng";
                return RedirectToAction(nameof(Index));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Success: Xóa tài khoản thành công";
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Random user
        [HttpGet("random/{count}")]
        public async Task<IActionResult> RamdomUser(int count)
        {
            try
            {
                Random random = new Random();

                var countUser = await _context.Users.CountAsync();

                for (int i = 1; i <= count; i++)
                {
                    var user = new User
                    {
                        UserName = $"Account {++countUser}",
                        Email = $"emailclone{countUser}@gmail.com",
                        PasswordHash = PasswordManager.HashPassword("123456"),
                    };

                    _context.Users.Add(user);
                }
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Thêm tài khoản thành công";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    // 2601: Cannot insert duplicate key row
                    // 2627: Violation of UNIQUE KEY constraint
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                    {
                        TempData["Message"] = "Error: Có email bị trùng";
                        return RedirectToAction(nameof(Index));
                    }
                }

                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet("random/delete/{count}")]
        public async Task<IActionResult> DeleteUser(int count)
        {
            try
            {
                var users = await _context.Users
                            .Where(u => u.Email.Contains("emailclone"))
                            .OrderByDescending(u => u.Email)
                            .Take(count)
                            .ToListAsync();

                _context.Users.RemoveRange(users);

                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Xóa tài khoản thành công";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion
    }
}
