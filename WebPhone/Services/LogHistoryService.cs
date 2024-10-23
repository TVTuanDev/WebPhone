using WebPhone.EF;
using WebPhone.Models;

namespace WebPhone.Services
{
    public class LogHistoryService
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _context;

        public LogHistoryService
            (
                AppDbContext context, 
                ILogger<LogHistoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogChanges(LogHistoryDTO logHistoryDTO)
        {
            try
            {
                var logHistory = new LogHistory
                {
                    EntityId = logHistoryDTO.EntityId,
                    EmploymentId = logHistoryDTO.EmploymentId,
                    Action = logHistoryDTO.Action,
                    EntityName = logHistoryDTO.EntityName,
                    OldValue = logHistoryDTO.OldValue,
                    NewValue = logHistoryDTO.NewValue,
                };

                _context.LogHistories.Add(logHistory);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
