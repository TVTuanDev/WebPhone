using System.ComponentModel.DataAnnotations;
using WebPhone.EF;

namespace WebPhone.Areas.Bills.Models.Bills
{
    public class BillDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public DiscountStyle DiscountStyle { get; set; }
        public int DiscountValue { get; set; }
        public int PaymentValue { get; set; }
        public List<Guid?> ProductId { get; set; } = new List<Guid?>();
        public List<int> Quantities { get; set; } = new List<int>();
    }
}
