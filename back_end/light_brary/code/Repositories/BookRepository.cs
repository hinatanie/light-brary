using Microsoft.EntityFrameworkCore;
using light_brary.Entities;
using light_brary.Data;

using light_brary.Enums;

namespace light_brary.Repositories
{
    public class BookRepository(AppDbContext context) : GenericRepository<Book>(context)
    {
        public Task<bool> HasBooksInCategoryAsync(Guid categoryId)
        {
            return _context.Books.AnyAsync(book => book.CategoryId == categoryId);
        }

        public async Task<Book?> GetByIsbnAsync(string isbn)
        {
            return await _context.Books
                .FirstOrDefaultAsync(b => b.ISBN == isbn);
        }

        public async Task<Book?> GetWithDetailsAsync(Guid id)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.BookCopies)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<(IEnumerable<Book> Books, int TotalCount)> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? keyword = null,
            Guid? categoryId = null,
            string? author = null,
            bool? onlyAvailable = null,
            string sortBy = "newest")
        {
            var query = _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                .Include(b => b.BookCopies)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(kw) || b.ISBN.ToLower().Contains(kw) || b.Author.ToLower().Contains(kw));
            }

            if (categoryId.HasValue && categoryId.Value != Guid.Empty)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                var auth = author.Trim().ToLower();
                query = query.Where(b => b.Author.ToLower().Contains(auth));
            }

            if (onlyAvailable.HasValue && onlyAvailable.Value)
            {
                query = query.Where(b => b.BookCopies.Any(c => c.Status == BookCopyStatus.Available));
            }

            var totalCount = await query.CountAsync();
            var offset = ((long)pageNumber - 1) * pageSize;
            if (offset >= totalCount)
                return ([], totalCount);

            var orderedQuery = sortBy switch
            {
                "title" => query.OrderBy(b => b.Title),
                "available" => query.OrderByDescending(b => b.BookCopies.Count(c => c.Status == BookCopyStatus.Available)),
                "year" => query.OrderByDescending(b => b.PublishYear ?? 0),
                _ => query.OrderByDescending(b => b.CreatedAt)
            };

            var books = await orderedQuery
                .ThenByDescending(b => b.Id)
                .Skip((int)offset)
                .Take(pageSize)
                .ToListAsync();

            return (books, totalCount);
        }
    }
}
