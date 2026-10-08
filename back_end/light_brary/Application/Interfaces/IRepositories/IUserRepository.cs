using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Domain.Entities;

namespace light_brary.Application.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber);
        Task<User?> GetByIdWithRolesAsync(int id);
        Task<User?> GetByPhoneNumberWithRolesAsync(string phoneNumber);
        Task<IEnumerable<User>> GetAllWithRolesAsync(string? keyword = null, string? role = null);
        Task<bool> HasBorrowHistoryAsync(int userId);
    }
}