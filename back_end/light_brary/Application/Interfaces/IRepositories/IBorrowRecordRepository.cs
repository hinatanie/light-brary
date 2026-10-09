using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Domain.Entities;

namespace light_brary.Application.Interfaces
{
    public interface IBorrowRecordRepository : IGenericRepository<BorrowRecord>
    {
        Task<IEnumerable<BorrowRecord>> GetByUserIdAsync(Guid userId);
        Task<BorrowRecord?> GetWithDetailsAsync(Guid id);
        Task<IEnumerable<BorrowRecord>> GetAllWithDetailsAsync(Guid? userId = null, string? status = null, bool? overdue = null);
    }
}