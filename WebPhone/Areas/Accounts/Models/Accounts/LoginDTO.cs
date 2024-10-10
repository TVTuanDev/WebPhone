using System.ComponentModel.DataAnnotations;

namespace WebPhone.Areas.Accounts.Models.Accounts
{
    public class LoginDTO
    {
        [Display(Name = "Email")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        [EmailAddress(ErrorMessage = "Vui lòng nhập đúng định dạng email")]
        public string Email { get; set; } = null!;

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự")]
        public string Password { get; set; } = null!;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }
}
