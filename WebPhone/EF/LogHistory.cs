namespace WebPhone.EF
{
    public class LogHistory
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public Guid EmploymentId { get; set; }
        public ActionLog Action {  get; set; }
        public string EntityName { get; set; } = null!;
        public string? OldValue { get; set; }
        public string NewValue { get; set; } = null!;
        public DateTime UpdateAt { get; set; }
        public User Employment { get; set; } = null!;
    }

    public enum ActionLog
    {
        Create,
        Update,
        Delete
    }
}
