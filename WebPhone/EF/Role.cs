namespace WebPhone.EF
{
    public class Role
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = null!;
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
