using System.ComponentModel.DataAnnotations;

namespace WebPhone.Areas.Accounts.Models.AdminAccount
{
    public class UserDTO
    {
        public Guid Id { get; set; }

        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string UserName { get; set; } = null!;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Vui lòng nhập đúng định dạng email")]
        public string Email { get; set; } = null!;

        [Display(Name = "Mật khẩu")]
        //[Required(ErrorMessage = "{0} bắt buộc nhập")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "{0} phải từ {2} đến {1} ký tự")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Xác thực")]
        public bool EmailConfirmed { get; set; }
    }
}
