using System.ComponentModel.DataAnnotations;

namespace WebPhone.Areas.Products.Models.Products
{
    public class ProductDTO
    {
        public Guid Id { get; set; }

        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string ProductName { get; set; } = null!;

        [Display(Name = "Ảnh đại diện")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public IFormFile Avatar { get; set; } = null!;

        [Display(Name = "Thông tin sản phẩm")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string Description { get; set; } = null!;

        [Display(Name = "Giá sản phẩm")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public int Price { get; set; }

        [Display(Name = "Giá giảm")]
        public int? Discount { get; set; }

        [Display(Name = "Danh mục sản phẩm")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public Guid CategoryId { get; set; }
    }
}
