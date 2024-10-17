namespace WebPhone.Areas.Accounts.Models.AdminAccount
{
    public class CustomerDTO
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
    }
}
