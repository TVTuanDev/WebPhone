using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebPhone.Areas.Admins.Models.Users;
using WebPhone.Attributes;
using WebPhone.EF;
using WebPhone.Models;

namespace WebPhone.Areas.Admins.Controllers
{
    [Area("Admins")]
    [Route("/admin/user/")]
    [AppAuthorize("Administrator, Manager")]
    public class UsersController : Controller
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _context;

        private readonly int ITEM_PER_PAGE = 10;

        public UsersController
            (
                ILogger<UsersController> logger, 
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
                userList = await _context.Users
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

            var roles = await _context.Roles.Select(r => new { r.Id, r.RoleName }).ToListAsync();
            ViewBag.RoleList = new SelectList(roles, "Id", "RoleName");

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
                PhoneNumber = u.PhoneNumber,
                Address = u.Address,
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
                    PhoneNumber = userDTO.PhoneNumber,
                    Address = userDTO.Address,
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
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Address = user.Address ?? string.Empty,
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
                user.PhoneNumber = userDTO.PhoneNumber;
                user.Address = userDTO.Address;
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
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
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

                string[] address = { "Hà Nội", "Hồ Chí Minh", "Thanh Hóa", "Lạng Sơn", "Quảng Ninh", "Bắc Ninh", "Cần Thơ" };

                var countUser = await _context.Users.CountAsync();

                for (int i = 1; i <= count; i++)
                {
                    var user = new User
                    {
                        UserName = $"Account {++countUser}",
                        Email = $"emailclone{countUser}@gmail.com",
                        PhoneNumber = $"0{RandomGenerator.RandomNumber(9)}",
                        Address = address[random.Next(address.Length)],
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

        [HttpPost("filter-name")]
        public async Task<JsonResult> FilterCusomterByName(string name)
        {
            name = string.IsNullOrEmpty(name) ? "" : name;

            var users = await _context.Users
                            .Where(u => u.UserName.Contains(name))
                            .Take(100)
                            .Select(u => new User
                            {
                                Id = u.Id,
                                UserName = u.UserName,
                                Email = u.Email,
                                PhoneNumber = u.PhoneNumber,
                                Address = u.Address,
                            })
                            .ToListAsync();

            return Json(new
            {
                Success = true,
                Message = "Success",
                Data = users
            });
        }

        [HttpPost("create-customer")]
        public async Task<JsonResult> CreateCustomer(CustomerDTO customerDTO)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    Success = false,
                    Message = "Vui lòng nhập đầy đủ thông tin"
                });

            if (!CheckRegexMail(customerDTO.Email))
                return Json(new
                {
                    Success = false,
                    Message = "Vui lòng nhập đúng định dạng email"
                });

            if (!CheckRegexPhoneNumber(customerDTO.PhoneNumber))
                return Json(new
                {
                    Success = false,
                    Message = "Số điện thoại không hợp lệ"
                });

            var user = new User
            {
                UserName = customerDTO.CustomerName,
                Email = customerDTO.Email,
                PhoneNumber = customerDTO.PhoneNumber,
                Address = customerDTO.Address,
                PasswordHash = PasswordManager.HashPassword("123456"),
                EmailConfirmed = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            customerDTO.Id = user.Id;

            return Json(new
            {
                Success = true,
                Message = "Success",
                Data = customerDTO
            });
        }

        [HttpGet("authorize")]
        public async Task<IActionResult> UserAuthorization(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if(user == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin tài khoản";
                return RedirectToAction(nameof(Index));
            }

            var selectRole = await _context.UserRoles
                                .Where(ur => ur.UserId == user.Id)
                                .Select(ur => ur.RoleId)
                                .ToListAsync();

            var userRoleDTO = new UserRoleDTO
            {
                UserId = user.Id,
                SelectedRole = selectRole
            };

            ViewBag.User = user;
            ViewBag.Roles = await _context.Roles.ToListAsync();

            return View(userRoleDTO);
        }

        [HttpPost("authorize")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserAuthorization(UserRoleDTO userRoleDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Thông tin không chính xác";
                    return RedirectToAction(nameof(Index));
                }

                var user = await _context.Users.FindAsync(userRoleDTO.UserId);
                if (user == null)
                {
                    TempData["Message"] = "Error: Không tìm thấy thông tin tài khoản";
                    return RedirectToAction(nameof(Index));
                }

                var userRole = await _context.UserRoles.ToListAsync();

                var userRoleByUser = userRole.Where(ur => ur.UserId == user.Id)
                                    .Select(ur => ur.RoleId).ToList();

                foreach (Guid guidId in userRoleByUser)
                {
                    // Nếu user role mới không chứa user role cũ
                    // Xóa user role cũ
                    if (!userRoleDTO.SelectedRole.Contains(guidId))
                    {
                        var userRoleRemove = userRole.FirstOrDefault(ur => ur.RoleId == guidId);
                        if(userRoleRemove == null)
                        {
                            TempData["Message"] = "Error: Không tìm thấy thông tin quyền";
                            return RedirectToAction(nameof(UserAuthorization));
                        }
                        _context.UserRoles.Remove(userRoleRemove);
                    }
                }

                foreach (Guid guidId in userRoleDTO.SelectedRole)
                {
                    // Nếu user role mới ko có trong user role cũ
                    // Thêm user role mới
                    if (!userRoleByUser.Contains(guidId))
                    {
                        var userRoleNew = new UserRole
                        {
                            UserId = user.Id,
                            RoleId = guidId,
                        };
                        _context.UserRoles.Add(userRoleNew);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Phân quyền thành công";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool CheckRegexMail(string email)
        {
            string pattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
            Regex regex = new Regex(pattern);
            if (regex.IsMatch(email)) return true;
            return false;
        }

        private bool CheckRegexPhoneNumber(string phoneNumber)
        {
            string pattern = @"^0[3|5|7|8|9][0-9]{8}$";
            Regex regex = new Regex(pattern);
            if (regex.IsMatch(phoneNumber)) return true;
            return false;
        }
    }
}
