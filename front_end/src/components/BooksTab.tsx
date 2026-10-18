import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import type { Book, Category } from '../types';
import { booksApi } from '../api';
import { Plus, Search, Edit2, Trash2, BookOpen, Layers, X, Image as ImageIcon } from 'lucide-react';

interface BooksTabProps {
  books: Book[];
  categories: Category[];
  onRefresh: () => void;
  onNotify: (text: string, type?: 'success' | 'error') => void;
  onViewCopies: (bookId: string) => void;
  onViewBook: (bookId: string) => void;
  readOnly?: boolean;
}

export const BooksTab: React.FC<BooksTabProps> = ({
  books,
  categories,
  onRefresh,
  onNotify,
  onViewCopies,
  onViewBook,
  readOnly = false,
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [categoryFilter, setCategoryFilter] = useState<string>('all');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingBook, setEditingBook] = useState<Book | null>(null);

  // Form states
  const [title, setTitle] = useState('');
  const [author, setAuthor] = useState('');
  const [isbn, setIsbn] = useState('');
  const [publishYear, setPublishYear] = useState<number | ''>('');
  const [imageUrl, setImageUrl] = useState('');
  const [categoryId, setCategoryId] = useState<string>(String(categories[0]?.id || ''));
  const [submitting, setSubmitting] = useState(false);

  const openCreateModal = () => {
    setEditingBook(null);
    setTitle('');
    setAuthor('');
    setIsbn('');
    setPublishYear(new Date().getFullYear());
    setImageUrl('');
    setCategoryId(String(categories[0]?.id || ''));
    setIsModalOpen(true);
  };

  const openEditModal = (book: Book) => {
    setEditingBook(book);
    setTitle(book.title);
    setAuthor(book.author);
    setIsbn(book.isbn || '');
    setPublishYear(book.publishYear || '');
    setImageUrl(book.imageUrl || '');
    setCategoryId(String(book.categoryId));
    setIsModalOpen(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      const payload = {
        title: title.trim(),
        author: author.trim(),
        isbn: isbn.trim() || undefined,
        publishYear: publishYear ? Number(publishYear) : undefined,
        imageUrl: imageUrl.trim() || undefined,
        categoryId: categoryId,
      };

      if (editingBook) {
        await booksApi.update(editingBook.id, payload);
        onNotify(`Đã cập nhật sách "${title}" thành công!`);
      } else {
        await booksApi.create(payload);
        onNotify(`Đã thêm mới đầu sách "${title}" thành công!`);
      }
      setIsModalOpen(false);
      onRefresh();
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi lưu thông tin sách', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (book: Book) => {
    if (!window.confirm(`Bạn có chắc chắn muốn xóa đầu sách "${book.title}"?`)) return;
    try {
      await booksApi.delete(book.id);
      onNotify(`Đã xóa đầu sách "${book.title}"`);
      onRefresh();
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi xóa sách', 'error');
    }
  };

  const filteredBooks = books.filter((b) => {
    const matchesSearch =
      b.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      b.author.toLowerCase().includes(searchTerm.toLowerCase()) ||
      (b.isbn && b.isbn.toLowerCase().includes(searchTerm.toLowerCase()));
    const matchesCategory =
      categoryFilter === 'all' || b.categoryId === categoryFilter;
    return matchesSearch && matchesCategory;
  });

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
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>

        <select
          className="filter-select"
          value={categoryFilter}
          onChange={(e) => setCategoryFilter(e.target.value)}
        >
          <option value="all">Tất cả thể loại ({books.length})</option>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </select>
      </div>

      <div className="table-card">
        {filteredBooks.length === 0 ? (
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
                {filteredBooks.map((b, index) => (
                  <tr key={b.id} className="book-row" onClick={() => onViewBook(String(b.id))}>
                    <td>{index + 1}</td>
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

      {/* Modal Thêm / Sửa Sách */}
      {isModalOpen && (
        <div className="modal-backdrop" onClick={() => setIsModalOpen(false)}>
          <div className="modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>{editingBook ? '✏️ Cập Nhật Đầu Sách' : '📖 Thêm Đầu Sách Mới'}</h3>
              <button className="btn-icon" onClick={() => setIsModalOpen(false)}>
                <X size={18} />
              </button>
            </div>
            <form onSubmit={handleSubmit} className="form-content">
              <div className="form-group">
                <label>Tên sách *</label>
                <input
                  type="text"
                  required
                  placeholder="Ví dụ: Thiết Kế Clean Architecture với C#"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                />
              </div>

              <div className="form-group">
                <label>Tác giả *</label>
                <input
                  type="text"
                  required
                  placeholder="Ví dụ: Robert C. Martin"
                  value={author}
                  onChange={(e) => setAuthor(e.target.value)}
                />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Mã ISBN</label>
                  <input
                    type="text"
                    placeholder="978-0134494166"
                    value={isbn}
                    onChange={(e) => setIsbn(e.target.value)}
                  />
                </div>

                <div className="form-group">
                  <label>Năm xuất bản</label>
                  <input
                    type="number"
                    min={1000}
                    max={2100}
                    placeholder="2026"
                    value={publishYear}
                    onChange={(e) => setPublishYear(e.target.value ? Number(e.target.value) : '')}
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Thể loại *</label>
                <select
                  required
                  value={categoryId}
                  onChange={(e) => setCategoryId(e.target.value)}
                >
                  {categories.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                  <ImageIcon size={14} /> Đường dẫn ảnh bìa sách (Image URL)
                </label>
                <input
                  type="url"
                  placeholder="https://example.com/anh-bia-sach.jpg"
                  value={imageUrl}
                  onChange={(e) => setImageUrl(e.target.value)}
                />
                {imageUrl && (
                  <div style={{ marginTop: '8px', display: 'flex', alignItems: 'center', gap: '10px' }}>
                    <span style={{ fontSize: '12px', color: 'var(--text-muted)' }}>Xem trước:</span>
                    <img
                      src={imageUrl}
                      alt="Ảnh bìa xem trước"
                      style={{ width: '48px', height: '64px', objectFit: 'cover', borderRadius: '6px', border: '1px solid var(--border-color)' }}
                      onError={(e) => {
                        (e.target as HTMLElement).style.display = 'none';
                      }}
                    />
                  </div>
                )}
              </div>

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setIsModalOpen(false)}
                >
                  Hủy
                </button>
                <button type="submit" className="btn btn-primary" disabled={submitting}>
                  {submitting ? 'Đang lưu...' : editingBook ? 'Cập Nhật' : 'Tạo Sách Mới'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
