namespace WebPhone.EF
{
    public class Warehouse
    {
        public Guid Id { get; set; }
        public string WarehouseName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int Capacity { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }
}
