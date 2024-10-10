using System.ComponentModel.DataAnnotations;

namespace WebPhone.Areas.Accounts.Models.Accounts
{
    public class RegisterDTO
    {
        [Display(Name = "Tên khách hàng")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string UserName { get; set; } = null!;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        [EmailAddress(ErrorMessage = "Vui lòng nhập đúng định dạng {0}")]
        public string Email { get; set; } = null!;

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự")]
        public string Password { get; set; } = null!;

        [Display(Name = "Nhập lại mật khẩu")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không chính xác")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
