using System.Collections.Generic;
using System.Threading.Tasks;
using light_brary.DTOs.Requests;
using light_brary.DTOs.Responses;

namespace light_brary.Services.Interfaces
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