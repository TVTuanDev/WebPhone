namespace WebPhone.EF
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
