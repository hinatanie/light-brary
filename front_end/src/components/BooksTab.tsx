import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import type { Book } from '../types';
import { booksApi } from '../api';
import { Plus, Search, Edit2, Trash2, BookOpen, Layers, RefreshCw } from 'lucide-react';
import { BookFormModal } from './BookFormModal';
import { usePagedBooks } from '../hooks/usePagedBooks';
import { Pagination } from './Pagination';

interface BooksTabProps {
  onNotify: (text: string, type?: 'success' | 'error') => void;
  onViewCopies: (bookId: string) => void;
  onViewBook: (bookId: string) => void;
  readOnly?: boolean;
}

export const BooksTab: React.FC<BooksTabProps> = ({
  onNotify,
  onViewCopies,
  onViewBook,
  readOnly = false,
}) => {
  const { items: books, categories, query, pageNumber, pageSize, totalPages, totalCount,
    loading, error, setFilters, setPage, refresh } = usePagedBooks();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingBook, setEditingBook] = useState<Book | null>(null);

  const openCreateModal = () => {
    setEditingBook(null);
    setIsModalOpen(true);
  };

  const openEditModal = (book: Book) => {
    setEditingBook(book);
    setIsModalOpen(true);
  };

  const handleDelete = async (book: Book) => {
    if (!window.confirm(`Bạn có chắc chắn muốn xóa đầu sách "${book.title}"?`)) return;
    try {
      await booksApi.delete(book.id);
      onNotify(`Đã xóa đầu sách "${book.title}"`);
      refresh();
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi xóa sách', 'error');
    }
  };

  return (
    <div className="tab-container">
      <div className="section-header">
        <div>
          <h2>📚 {readOnly ? 'Tra Cứu Đầu Sách' : 'Danh Mục Đầu Sách'}</h2>
          <p className="section-desc">
            {readOnly ? 'Tra cứu thông tin tựa sách, tác giả, mã ISBN và tình trạng bản sao' : 'Quản lý danh sách các tựa sách, tác giả, mã ISBN và thể loại'}
          </p>
        </div>
        {!readOnly && (
          <button className="btn btn-primary" onClick={openCreateModal}>
            <Plus size={18} /> Thêm Đầu Sách Mới
          </button>
        )}
      </div>

      <div className="filter-bar">
        <div className="search-input-wrapper">
          <Search size={18} className="search-icon" />
          <input
            type="text"
            placeholder="Tìm theo tên sách, tác giả, ISBN..."
            value={query.keyword ?? ''}
            onChange={(e) => setFilters({ keyword: e.target.value })}
          />
        </div>

        <select
          className="filter-select"
          value={query.categoryId ?? 'all'}
          onChange={(e) => setFilters({ categoryId: e.target.value === 'all' ? undefined : e.target.value })}
        >
          <option value="all">Tất cả thể loại</option>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </select>
      </div>

      <div className="table-card">
        {loading ? (
          <div className="loading-banner" role="status">
            <RefreshCw size={18} className="spin" /> Đang tải danh mục đầu sách...
          </div>
        ) : error ? (
          <div className="empty-state" role="alert">
            <p>{error}</p>
            <button className="btn btn-secondary" onClick={refresh}>Thử lại</button>
          </div>
        ) : books.length === 0 ? (
          <div className="empty-state">
            <BookOpen size={48} color="#94a3b8" />
            <p>Không tìm thấy đầu sách nào phù hợp.</p>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="custom-table">
              <thead>
                <tr>
                  <th>STT</th>
                  <th>ID</th>
                  <th>Bìa sách</th>
                  <th>Tiêu đề sách</th>
                  <th>Tác giả</th>
                  <th>Thể loại</th>
                  <th>ISBN</th>
                  <th>Năm XB</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {books.map((b, index) => (
                  <tr key={b.id} className="book-row" onClick={() => onViewBook(String(b.id))}>
                    <td>{(pageNumber - 1) * pageSize + index + 1}</td>
                    <td>#{b.id}</td>
                    <td>
                      {b.imageUrl ? (
                        <img
                          src={b.imageUrl}
                          alt={b.title}
                          style={{
                            width: '40px',
                            height: '54px',
                            objectFit: 'cover',
                            borderRadius: '6px',
                            border: '1px solid var(--border-color)',
                            backgroundColor: 'var(--surface-muted)',
                            display: 'block',
                          }}
                          onError={(e) => {
                            (e.target as HTMLElement).style.display = 'none';
                          }}
                        />
                      ) : (
                        <div
                          style={{
                            width: '40px',
                            height: '54px',
                            borderRadius: '6px',
                            background: 'rgba(245, 158, 11, 0.1)',
                            border: '1px dashed #475569',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            color: 'var(--text-muted)',
                          }}
                          title="Chưa có ảnh bìa"
                        >
                          <BookOpen size={16} />
                        </div>
                      )}
                    </td>
                    <td>
                      <Link className="book-title-link" to={String(b.id)} onClick={(e) => e.stopPropagation()}>
                        <strong>{b.title}</strong>
                      </Link>
                    </td>
                    <td>{b.author}</td>
                    <td>
                      <span className="category-tag">{b.categoryName || '—'}</span>
                    </td>
                    <td><code>{b.isbn || '—'}</code></td>
                    <td>{b.publishYear || '—'}</td>
                    <td onClick={(e) => e.stopPropagation()}>
                      <div className="action-buttons">
                        <button
                          className="btn-action btn-copy"
                          title="Xem & Thêm bản sao vật lý"
                          onClick={() => onViewCopies(String(b.id))}
                        >
                          <Layers size={16} /> Bản sao
                        </button>
                        {!readOnly && (
                          <>
                            <button
                              className="btn-action"
                              title="Sửa"
                              onClick={() => openEditModal(b)}
                            >
                              <Edit2 size={16} />
                            </button>
                            <button
                              className="btn-action btn-danger"
                              title="Xóa"
                              onClick={() => handleDelete(b)}
                            >
                              <Trash2 size={16} />
                            </button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <Pagination page={pageNumber} pageSize={pageSize} totalPages={totalPages}
        totalCount={totalCount} disabled={loading || !!error} onPageChange={setPage} />

      {isModalOpen && (
        <BookFormModal
          book={editingBook}
          categories={categories}
          onClose={() => setIsModalOpen(false)}
          onSaved={refresh}
          onNotify={onNotify}
        />
      )}
    </div>
  );
};
