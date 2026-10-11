using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Entities;

namespace light_brary.Repositories.Interfaces
{
    public interface IBookCopyRepository : IGenericRepository<BookCopy>
    {
        Task<BookCopy?> GetByBarcodeAsync(string barcode);
        Task<IEnumerable<BookCopy>> GetByBookIdAsync(Guid bookId);
        Task<BookCopy?> GetWithBookAsync(Guid id);
        Task<bool> HasBorrowHistoryAsync(Guid id);
    }
}