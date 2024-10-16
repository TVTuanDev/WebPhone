namespace WebPhone.EF
{
    public class Bill
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid EmploymentId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string EmploymentName { get; set; } = null!;
        public int Price { get; set; }
        public int? Discount { get; set; }
        public int TotalPrice { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public virtual User Customer { get; set; } = new User();
        public virtual User Employment { get; set; } = new User();
        public virtual ICollection<BillInfo> BillInfos { get; set; } = new List<BillInfo>();
    }
}
