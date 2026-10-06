using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Domain.Entities;

namespace light_brary.Application.Interfaces
{
    public interface IBookCopyRepository : IGenericRepository<BookCopy>
    {
        Task<BookCopy?> GetByBarcodeAsync(string barcode);
        Task<IEnumerable<BookCopy>> GetByBookIdAsync(int bookId);
        Task<BookCopy?> GetWithBookAsync(int id);
    }
}