using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using light_brary.Application.Interfaces;
using light_brary.Domain.Entities;
using light_brary.Infrastructure.Persistence;

namespace light_brary.Infrastructure.Repositories
{
    public class BookCopyRepository : GenericRepository<BookCopy>, IBookCopyRepository
    {
        public BookCopyRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<BookCopy?> GetByBarcodeAsync(string barcode)
        {
            return await _context.BookCopies
                .Include(bc => bc.Book)
                .FirstOrDefaultAsync(bc => bc.Barcode == barcode);
        }

        public async Task<IEnumerable<BookCopy>> GetByBookIdAsync(int bookId)
        {
            return await _context.BookCopies
                .Where(bc => bc.BookId == bookId)
                .ToListAsync();
        }

        public async Task<BookCopy?> GetWithBookAsync(int id)
        {
            return await _context.BookCopies
                .Include(bc => bc.Book)
                .FirstOrDefaultAsync(bc => bc.Id == id);
        }
    }
}