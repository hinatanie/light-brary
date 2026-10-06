using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Domain.Entities;

namespace light_brary.Application.Interfaces
{
    public interface IBorrowRecordRepository : IGenericRepository<BorrowRecord>
    {
        Task<IEnumerable<BorrowRecord>> GetByUserIdAsync(int userId);
        Task<BorrowRecord?> GetWithDetailsAsync(int id);
    }
}