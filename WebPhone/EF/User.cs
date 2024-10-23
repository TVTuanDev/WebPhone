namespace WebPhone.EF
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string PasswordHash { get; set; } = null!;
        public bool EmailConfirmed { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public virtual ICollection<Bill> CustomerBills { get; set; } = new List<Bill>();
        public virtual ICollection<Bill> EmploymentBills { get; set; } = new List<Bill>();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<LogHistory> LogHistories { get; set; } = new List<LogHistory>();
        public virtual ICollection<PaymentLog> PaymentLogs { get; set; } = new List<PaymentLog>();
    }
}
