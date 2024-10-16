using System.ComponentModel.DataAnnotations;

namespace WebPhone.Areas.Bills.Models.Bills
{
    public class BillDTO
    {
        public Guid Id { get; set; }

        [Display(Name = "Khách hàng")]
        public Guid? CustomerId { get; set; }

        [Display(Name = "Tên khách hàng")]
        public string CustomerName { get; set; } = null!;

        [Display(Name = "Giá")]
        [Range(0, int.MaxValue, ErrorMessage = "Giá trị phải lớn hơn 0")]
        public int Price { get; set; }

        [Display(Name = "Giảm giá")]
        [Range(0, int.MaxValue, ErrorMessage = "Giá trị phải lớn hơn 0")]
        public int? Discount { get; set; }

        [Display(Name = "Tổng tiền")]
        public int TotalPrice { get; set; }
    }
}
