using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPhone.Areas.Accounts.Models.Accounts;
using WebPhone.EF;
using WebPhone.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace WebPhone.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    [Route("/customer/")]
    public class AccountsController : Controller
    {
        private readonly ILogger<AccountsController> _logger;
        private readonly AppDbContext _context;

        public AccountsController
            (
                ILogger<AccountsController> logger,
                AppDbContext context
            )
        {
            _logger = logger;
            _context = context;
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
                    PasswordHash = PasswordManager.HashPassword(registerDTO.Password)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Tạo tài khoản thành công";

                return RedirectToAction("Login");
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

                // Add cookie xác thực
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email)
                };

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

        private bool CheckLogin() => User.Identity!.IsAuthenticated;
    }
}
