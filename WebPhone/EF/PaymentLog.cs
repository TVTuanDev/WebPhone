namespace WebPhone.EF
{
    public class PaymentLog
    {
        public Guid Id { get; set; }
        public Guid BillId { get; set; }
        public Guid CustomerId { get; set; }
        public int Price { get; set; }
        public DateTime CreateAt { get; set; }
        public virtual Bill Bill { get; set; } = null!;
        public virtual User Customer { get; set; } = null!;
    }
}
