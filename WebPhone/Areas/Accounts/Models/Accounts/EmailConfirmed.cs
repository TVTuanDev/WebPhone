using System.ComponentModel.DataAnnotations;

namespace WebPhone.Areas.Accounts.Models.Accounts
{
    public class EmailConfirmed
    {
        public string Email { get; set; } = null!;

        [Display(Name = "Mã xác thực")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string Code { get; set; } = null!;

    }
}
