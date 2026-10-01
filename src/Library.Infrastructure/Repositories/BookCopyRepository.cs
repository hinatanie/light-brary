using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public class BookCopyRepository : IBookCopyRepository
{
    private readonly LibraryDbContext _context;

    public BookCopyRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<BookCopy>> GetByBookIdAsync(int bookId)
    {
        return await _context.BookCopies
            .Include(bc => bc.Book)
            .AsNoTracking()
            .Where(bc => bc.BookId == bookId)
            .OrderBy(bc => bc.Barcode)
            .ToListAsync();
    }

    public async Task<BookCopy?> GetByIdAsync(int id)
    {
        return await _context.BookCopies
            .Include(bc => bc.Book)
            .FirstOrDefaultAsync(bc => bc.Id == id);
    }

    public async Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null)
    {
        return await _context.BookCopies
            .AnyAsync(bc => bc.Barcode == barcode && (excludeId == null || bc.Id != excludeId));
    }

    public async Task<bool> BookExistsAsync(int bookId)
    {
        return await _context.Books.AnyAsync(b => b.Id == bookId);
    }

    public async Task<BookCopy> AddAsync(BookCopy bookCopy)
    {
        await _context.BookCopies.AddAsync(bookCopy);
        await _context.SaveChangesAsync();
        return bookCopy;
    }

    public async Task UpdateAsync(BookCopy bookCopy)
    {
        _context.BookCopies.Update(bookCopy);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(BookCopy bookCopy)
    {
        _context.BookCopies.Remove(bookCopy);
        await _context.SaveChangesAsync();
    }
}
