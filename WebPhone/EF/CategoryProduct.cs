namespace WebPhone.EF
{
    public class CategoryProduct
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; } = null!;
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public Guid? IdParent { get; set; }
        public CategoryProduct? CateProductParent { get; set; }
        public ICollection<CategoryProduct> CateProductChildren { get; set; } = new List<CategoryProduct>();
        public ICollection<Product> Products { get; set;} = new List<Product>();
    }
}
