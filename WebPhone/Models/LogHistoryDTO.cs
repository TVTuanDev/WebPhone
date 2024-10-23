using WebPhone.EF;

namespace WebPhone.Models
{
    public class LogHistoryDTO
    {
        public ActionLog Action { get; set; }
        public string EntityName { get; set; } = null!;
        public Guid EntityId { get; set; }
        public string? OldValue { get; set; }
        public string NewValue { get; set; } = null!;
        public Guid EmploymentId { get; set; }
    }
}
