using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;

    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await _context.Books
            .Include(b => b.Category)
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(Guid id)
    {
        return await _context.Books
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<bool> CategoryExistsAsync(Guid categoryId)
    {
        return await _context.Categories.AnyAsync(c => c.Id == categoryId);
    }

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        return await _context.Books
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Isbn == isbn);
    }

    public async Task<bool> HasBorrowHistoryAsync(Guid bookId)
    {
        return await _context.BorrowDetails
            .AnyAsync(bd => bd.BookCopy != null && bd.BookCopy.BookId == bookId);
    }

    public async Task<Book> AddAsync(Book book)
    {
        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task UpdateAsync(Book book)
    {
        _context.Books.Update(book);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Book book)
    {
        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
    }
}
