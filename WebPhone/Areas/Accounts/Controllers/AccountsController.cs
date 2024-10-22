using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPhone.Areas.Accounts.Models.Accounts;
using WebPhone.EF;
using WebPhone.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using WebPhone.Services;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace WebPhone.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    [Route("/customer/")]
    public class AccountsController : Controller
    {
        private readonly ILogger<AccountsController> _logger;
        private readonly AppDbContext _context;
        private readonly SendMailService _sendMail;
        private readonly IMemoryCache _cache;

        public AccountsController
            (
                ILogger<AccountsController> logger,
                AppDbContext context,
                SendMailService sendMail,
                IMemoryCache cache
            )
        {
            _logger = logger;
            _context = context;
            _sendMail = sendMail;
            _cache = cache;
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            if (CheckLogin())
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                    return View(registerDTO);
                }

                if (!CheckRegexMail(registerDTO.Email))
                {
                    TempData["Message"] = "Error: Không đúng định dạng email";
                    return View(registerDTO);
                }

                var userByEmail = await _context.Users.FirstOrDefaultAsync(c => c.Email == registerDTO.Email);
                if (userByEmail is not null)
                {
                    TempData["Message"] = "Error: Email đã được sử dụng";
                    return View(registerDTO);
                }

                var user = new User
                {
                    UserName = registerDTO.UserName,
                    Email = registerDTO.Email,
                    PhoneNumber = registerDTO.PhoneNumber,
                    Address = registerDTO.Address,
                    PasswordHash = PasswordManager.HashPassword(registerDTO.Password)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Gửi code gửi mail xác thực
                await SendMailConfirmCode(registerDTO.Email);

                TempData["Message"] = "Success: Mã xác thực đã được gửi qua hòm thư";

                return RedirectToAction("ConfirmEmail", new { userId = user.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return View(registerDTO);
            }
        }

        [HttpGet("login")]
        public IActionResult Login(string? returnUrl)
        {
            if (CheckLogin())
                return RedirectToAction("Index", "Home");

            returnUrl ??= Url.Content("/");
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO loginDTO, string? returnUrl)
        {
            returnUrl ??= Url.Content("/");
            ViewData["ReturnUrl"] = returnUrl;

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                    return View(loginDTO);
                }

                if (!CheckRegexMail(loginDTO.Email))
                {
                    TempData["Message"] = "Error: Không đúng định dạng email";
                    return View(loginDTO);
                }

                var user = await _context.Users.FirstOrDefaultAsync(c => c.Email == loginDTO.Email);
                if (user is null)
                {
                    TempData["Message"] = "Error: Thông tin tài khoản không chính xác";
                    return View(loginDTO);
                }

                if (!PasswordManager.VerifyPassword(loginDTO.Password, user.PasswordHash))
                {
                    TempData["Message"] = "Error: Thông tin tài khoản không chính xác";
                    return View(loginDTO);
                }

                if (!user.EmailConfirmed)
                {
                    // Gửi code gửi mail xác thực
                    await SendMailConfirmCode(user.Email);

                    TempData["Message"] = "Success: Email chưa được xác thực, vui lòng xác thực email qua hòm thư";
                    return RedirectToAction("ConfirmEmail", new { userId = user.Id });
                }

                // Add cookie xác thực
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                };

                var listRoleName = await (from r in _context.Roles
                                          join ur in _context.UserRoles on r.Id equals ur.RoleId
                                          where ur.UserId == user.Id
                                          select r.RoleName).ToListAsync();

                foreach (var roleName in listRoleName)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleName));
                }

                var claimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = loginDTO.RememberMe, // Cookie lưu lại khi đóng trình duyệt
                    //ExpiresUtc = DateTime.UtcNow.AddDays(30), // Thời gian tồn tại cookie
                };

                // Đăng nhập
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimIdentity),
                    authProperties);

                return LocalRedirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return View(loginDTO);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Đăng xuất và xóa cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Accounts");  // Chuyển hướng đến trang login
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin người dùng";
                return RedirectToAction(nameof(Register));
            }

            ViewBag.Email = user.Email;

            return View();
        }

        [HttpPost("confirm-email")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEmail(EmailConfirmed emailConfirmed)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                    return View(emailConfirmed);
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailConfirmed.Email);
                if (user == null)
                {
                    TempData["Message"] = "Error: Không thấy thông tin người dùng";
                    return View(emailConfirmed);
                }

                if (!VerifyEmail(emailConfirmed.Email, emailConfirmed.Code))
                {
                    TempData["Message"] = "Error: Mã xác thực không đúng hoặc đã hết hạn";
                    return View(emailConfirmed);
                }

                user.EmailConfirmed = true;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Xác thực tài khoản thành công";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return View(emailConfirmed);
            }
        }

        [HttpGet("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost("forgot-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            ViewData["Email"] = email;

            if (!CheckRegexMail(email))
            {
                TempData["Message"] = "Error: Không đúng định dạng email";
                return View();
            }

            var userByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (userByEmail == null)
            {
                TempData["Message"] = "Error: Email chưa được đăng ký";
                return View();
            }

            // Gửi code gửi mail forgot password
            await SendForgotPasswordCode(email);

            TempData["Message"] = "Success: Mã xác thực đã được gửi qua hòm thư";

            return RedirectToAction("ConfirmForgotPassword", new { userId = userByEmail.Id });
        }

        [HttpGet("forgot-password/confirm")]
        public async Task<IActionResult> ConfirmForgotPassword(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin người dùng";
                return View();
            }

            ViewData["Email"] = user.Email;

            return View();
        }

        [HttpPost("forgot-password/confirm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                return View(forgotPasswordDTO);
            }

            if(!VerifyEmail(forgotPasswordDTO.Email, forgotPasswordDTO.Code))
            {
                TempData["Message"] = "Error: Mã xác thực không đúng hoặc đã hết hạn";
                return View(forgotPasswordDTO);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDTO.Email);
            if (user == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin người dùng";
                return View(forgotPasswordDTO);
            }

            user.PasswordHash = PasswordManager.HashPassword(forgotPasswordDTO.Password);
            user.UpdateAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Success: Đổi mật khẩu mới thành công";

            return RedirectToAction(nameof(Login));
        }

        private bool CheckLogin() => User.Identity!.IsAuthenticated;

        private bool CheckRegexMail(string email)
        {
            string pattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
            Regex regex = new Regex(pattern);
            if (regex.IsMatch(email)) return true;
            return false;
        }

        private async Task SendMailConfirmCode(string email)
        {
            var code = RandomGenerator.RandomCode(8);
            var htmlMessage = $@"<h3>Bạn đã đăng ký tài khoản trên WebPhone</h3>
                    <p>Tiếp tục đăng ký với WebPhone bằng cách nhập mã bên dưới:</p>
                    <h1>{code}</h1>
                    <p>Mã xác minh sẽ hết hạn sau 10 phút.</p>
                    <p><b>Nếu bạn không yêu cầu mã,</b> bạn có thể bỏ qua tin nhắn này.</p>";

            await _sendMail.SendMailAsync(email, "Xác thực tài khoản", htmlMessage);

            var emailConfirm = new EmailConfirmed
            {
                Email = email,
                Code = code,
            };

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

            _cache.Set(email, emailConfirm, cacheEntryOptions);
        }

        private async Task SendForgotPasswordCode(string email)
        {
            var code = RandomGenerator.RandomCode(8);
            var htmlMessage = $@"<h3>Bạn quên mật khẩu tài khoản WebPhone</h3>
                    <p>Lấy lại mật khẩu WebPhone bằng cách nhập mã bên dưới:</p>
                    <h1>{code}</h1>
                    <p>Mã xác minh sẽ hết hạn sau 10 phút.</p>
                    <p><b>Nếu bạn không yêu cầu mã,</b> bạn có thể bỏ qua tin nhắn này.</p>";

            await _sendMail.SendMailAsync(email, "Quên mật khẩu", htmlMessage);

            var emailConfirm = new EmailConfirmed
            {
                Email = email,
                Code = code,
            };

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

            _cache.Set(email, emailConfirm, cacheEntryOptions);
        }

        private bool VerifyEmail(string email, string code)
        {
            var emailConfirm = _cache.Get<EmailConfirmed>(email);

            if(emailConfirm == null) return false;

            if (emailConfirm.Email != email) return false;

            if (emailConfirm.Code != code) return false;

            _cache.Remove(email);

            return true;
        }
    }
}
