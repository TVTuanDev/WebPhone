using System.ComponentModel.DataAnnotations;

namespace WebPhone.Areas.Warehouses.Models.Warehouses
{
    public class WarehouseDTO
    {
        public Guid Id { get; set; }

        [Display(Name = "Tên kho")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string WarehouseName { get; set; } = null!;

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string Address { get; set; } = null!;

        [Display(Name = "Sức chứa tối đa")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public int Capacity { get; set; }
    }
}
