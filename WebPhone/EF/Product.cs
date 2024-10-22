namespace WebPhone.EF
{
    public class Product
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int Price { get; set; }
        public int? Discount { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public Guid? CategoryId { get; set; }
        public virtual CategoryProduct? CategoryProduct { get; set; }
        public virtual ICollection<BillInfo> BillInfos { get; set; } = new List<BillInfo>();
        public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }
}
