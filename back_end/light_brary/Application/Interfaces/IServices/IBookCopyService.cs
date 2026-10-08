using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.Application.DTOs.BookCopies;

namespace light_brary.Application.Interfaces
{
    public interface IBookCopyService
    {
        Task<IEnumerable<BookCopyResponse>> GetByBookIdAsync(int bookId);
        Task<BookCopyResponse> GetByIdAsync(int id);
        Task<BookCopyResponse> GetByBarcodeAsync(string barcode);
        Task<BookCopyResponse> CreateAsync(int bookId, SaveBookCopyRequest request);
        Task<BookCopyResponse> UpdateAsync(int id, SaveBookCopyRequest request);
        Task DeleteAsync(int id);
        Task<BookCopyResponse> MarkAvailableAsync(int id);
        Task<BookCopyResponse> MarkLostAsync(int id);
    }
}