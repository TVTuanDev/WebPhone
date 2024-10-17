namespace WebPhone.EF
{
    public class BillInfo
    {
        public Guid Id { get; set; }
        public Guid? ProductId { get; set; }
        public Guid BillId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Price { get; set; }
        public int? Discount { get; set; }
        public int Quantity { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public virtual Product? Product { get; set; }
        public virtual Bill? Bill { get; set; }
    }
}
