using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Entities;

namespace light_brary.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber);
        Task<User?> GetByIdWithRolesAsync(Guid id);
        Task<User?> GetByPhoneNumberWithRolesAsync(string phoneNumber);
        Task<IEnumerable<User>> GetAllWithRolesAsync(string? keyword = null, string? role = null);
        Task<bool> HasBorrowHistoryAsync(Guid userId);
    }
}