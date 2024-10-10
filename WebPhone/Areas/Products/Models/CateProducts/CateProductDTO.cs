namespace WebPhone.Areas.Products.Models.CateProducts
{
    public class CateProductDTO
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; } = null!;
        public Guid? IdParent { get; set; }
    }
}
