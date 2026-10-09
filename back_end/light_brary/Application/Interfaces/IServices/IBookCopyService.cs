using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Application.DTOs.BookCopies;

namespace light_brary.Application.Interfaces
{
    public interface IBookCopyService
    {
        Task<IEnumerable<BookCopyResponse>> GetByBookIdAsync(Guid bookId);
        Task<BookCopyResponse> GetByIdAsync(Guid id);
        Task<BookCopyResponse> GetByBarcodeAsync(string barcode);
        Task<BookCopyResponse> CreateAsync(Guid bookId, SaveBookCopyRequest request);
        Task<BookCopyResponse> UpdateAsync(Guid id, SaveBookCopyRequest request);
        Task DeleteAsync(Guid id);
        Task<BookCopyResponse> MarkAvailableAsync(Guid id);
        Task<BookCopyResponse> MarkLostAsync(Guid id);
    }
}