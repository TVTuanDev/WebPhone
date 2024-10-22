using System.ComponentModel.DataAnnotations;

namespace WebPhone.Areas.Admins.Models.Roles
{
    public class RoleDTO
    {
        public Guid Id { get; set; }

        [Display(Name = "Tên quyền")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string RoleName { get; set; } = null!;
    }
}
