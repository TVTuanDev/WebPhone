using WebPhone.EF;

namespace WebPhone.Areas.Admins.Models.Customers
{
    public class CustomerDebtDTO
    {
        public User Customer { get; set; } = null!;
        public int Debt { get; set; }
    }
}
