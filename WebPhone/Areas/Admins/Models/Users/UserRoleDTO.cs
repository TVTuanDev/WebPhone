using WebPhone.EF;

namespace WebPhone.Areas.Admins.Models.Users
{
    public class UserRoleDTO
    {
        public Guid UserId { get; set; }
        public List<Guid> SelectedRole { get; set; } = new List<Guid>();
    }
}
