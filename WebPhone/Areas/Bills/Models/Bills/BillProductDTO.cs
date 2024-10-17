using WebPhone.EF;

namespace WebPhone.Areas.Bills.Models.Bills
{
    public class BillProductDTO
    {
        public Product Product { get; set; } = new Product();
        public int Quantity { get; set; }
    }
}
