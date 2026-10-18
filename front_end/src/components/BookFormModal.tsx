import { useState, type FormEvent } from 'react';
import { X, Image as ImageIcon } from 'lucide-react';
import type { Book, Category } from '../types';
import { booksApi } from '../api';

interface BookFormModalProps {
  book?: Book | null;
  categories: Category[];
  onClose: () => void;
  onSaved: (book: Book) => void;
  onNotify: (text: string, type?: 'success' | 'error') => void;
}

export const BookFormModal = ({ book, categories, onClose, onSaved, onNotify }: BookFormModalProps) => {
  const [title, setTitle] = useState(book?.title ?? '');
  const [author, setAuthor] = useState(book?.author ?? '');
  const [isbn, setIsbn] = useState(book?.isbn ?? '');
  const [publishYear, setPublishYear] = useState<number | ''>(() => book ? book.publishYear ?? '' : new Date().getFullYear());
  const [imageUrl, setImageUrl] = useState(book?.imageUrl ?? '');
  const [categoryId, setCategoryId] = useState(String(book?.categoryId ?? categories[0]?.id ?? ''));
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (submitting) return;
    setSubmitting(true);
    try {
      const payload = {
        title: title.trim(),
        author: author.trim(),
        isbn: isbn.trim() || undefined,
        publishYear: publishYear ? Number(publishYear) : undefined,
        imageUrl: imageUrl.trim() || undefined,
        categoryId,
      };
      const saved = book
        ? await booksApi.update(book.id, payload)
        : await booksApi.create(payload);
      onNotify(book ? `Đã cập nhật sách "${saved.title}" thành công!` : `Đã thêm mới đầu sách "${saved.title}" thành công!`);
      onSaved(saved);
      onClose();
    } catch (error: unknown) {
      onNotify(error instanceof Error ? error.message : 'Lỗi khi lưu thông tin sách', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="modal-backdrop" onClick={() => { if (!submitting) onClose(); }}>
      <div className="modal-box" role="dialog" aria-modal="true" aria-labelledby="book-form-title" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3 id="book-form-title">{book ? '✏️ Cập Nhật Đầu Sách' : '📖 Thêm Đầu Sách Mới'}</h3>
          <button className="btn-icon" aria-label="Đóng" disabled={submitting} onClick={onClose}>
            <X size={18} />
          </button>
        </div>
        <form onSubmit={handleSubmit} className="form-content">
          <div className="form-group">
            <label htmlFor="book-title">Tên sách *</label>
            <input
              type="text"
              required
              placeholder="Ví dụ: Thiết Kế Clean Architecture với C#"
              id="book-title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
            />
          </div>

          <div className="form-group">
            <label htmlFor="book-author">Tác giả *</label>
            <input
              type="text"
              required
              placeholder="Ví dụ: Robert C. Martin"
              id="book-author"
              value={author}
              onChange={(e) => setAuthor(e.target.value)}
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="book-isbn">Mã ISBN</label>
              <input
                type="text"
                placeholder="978-0134494166"
                id="book-isbn"
                value={isbn}
                onChange={(e) => setIsbn(e.target.value)}
              />
            </div>

            <div className="form-group">
              <label htmlFor="book-year">Năm xuất bản</label>
              <input
                type="number"
                min={1000}
                max={2100}
                placeholder="2026"
                id="book-year"
                value={publishYear}
                onChange={(e) => setPublishYear(e.target.value ? Number(e.target.value) : '')}
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="book-category">Thể loại *</label>
            <select
              required
              id="book-category"
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
            <label htmlFor="book-image" style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <ImageIcon size={14} /> Đường dẫn ảnh bìa sách (Image URL)
            </label>
            <input
              type="url"
              placeholder="https://example.com/anh-bia-sach.jpg"
              id="book-image"
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
              disabled={submitting}
              onClick={onClose}
            >
              Hủy
            </button>
            <button type="submit" className="btn btn-primary" disabled={submitting}>
              {submitting ? 'Đang lưu...' : book ? 'Cập Nhật' : 'Tạo Sách Mới'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
