using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using light_brary.Entities;
using light_brary.Data;

namespace light_brary.Repositories
{
    public class BookCopyRepository(AppDbContext context) : GenericRepository<BookCopy>(context)
    {
        public async Task<IEnumerable<BookCopy>> GetAllWithBookAsync()
        {
            return await _context.BookCopies
                .Include(bc => bc.Book)
                .OrderBy(bc => bc.Book.Title)
                .ToListAsync();
        }

        public async Task<BookCopy?> GetByBarcodeAsync(string barcode)
        {
            return await _context.BookCopies
                .Include(bc => bc.Book)
                .FirstOrDefaultAsync(bc => bc.Barcode == barcode);
        }

        public async Task<IEnumerable<BookCopy>> GetByBookIdAsync(Guid bookId)
        {
            return await _context.BookCopies
                .Include(bc => bc.Book)
                .Where(bc => bc.BookId == bookId)
                .OrderBy(bc => bc.Id)
                .ToListAsync();
        }

        public async Task<BookCopy?> GetWithBookAsync(Guid id)
        {
            return await _context.BookCopies
                .Include(bc => bc.Book)
                .FirstOrDefaultAsync(bc => bc.Id == id);
        }

        public async Task<bool> HasBorrowHistoryAsync(Guid id)
        {
            return await _context.BorrowDetails
                .AnyAsync(bd => bd.BookCopyId == id);
        }
    }
}
