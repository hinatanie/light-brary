import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft, BookOpen, Edit2, Layers, RefreshCw, Trash2 } from 'lucide-react';
import { ApiError, booksApi, categoriesApi } from '../api';
import type { Book, Category } from '../types';
import { BookFormModal } from '../components/BookFormModal';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../contexts/ToastContext';

export const BookDetailsPage = ({ portalPath }: { portalPath: '/admin' | '/staff' }) => {
  const { bookId } = useParams<{ bookId: string }>();
  const navigate = useNavigate();
  const { hasRole } = useAuth();
  const { showToast } = useToast();
  const [result, setResult] = useState<{ id: string; book?: Book; error?: string } | null>(null);
  const [failedImage, setFailedImage] = useState<string | null>(null);
  const [categories, setCategories] = useState<Category[]>([]);
  const [editingBook, setEditingBook] = useState<Book | null>(null);
  const [busy, setBusy] = useState(false);
  const canEdit = hasRole('Admin') || hasRole('Librarian');
  const canDelete = hasRole('Admin');

  useEffect(() => {
    if (!bookId) return;
    let cancelled = false;

    booksApi.getById(bookId).then(
      (book) => {
        if (!cancelled) setResult({ id: bookId, book });
      },
      (error: unknown) => {
        if (!cancelled) {
          setResult({
            id: bookId,
            error: error instanceof ApiError && error.status === 404
              ? 'Không tìm thấy sách này. Sách có thể đã bị xóa.'
              : 'Không thể tải thông tin sách. Vui lòng tải lại trang để thử lại.',
          });
        }
      },
    );

    return () => { cancelled = true; };
  }, [bookId]);

  const book = result?.id === bookId ? result?.book : undefined;

  const handleEdit = async () => {
    if (!book || busy) return;
    setBusy(true);
    try {
      setCategories(await categoriesApi.getAll());
      setEditingBook(book);
    } catch (error: unknown) {
      showToast(error instanceof Error ? error.message : 'Không thể tải thể loại sách', 'error');
    } finally {
      setBusy(false);
    }
  };

  const handleDelete = async () => {
    if (!book || busy || !window.confirm(`Bạn có chắc chắn muốn xóa đầu sách "${book.title}"?`)) return;
    setBusy(true);
    try {
      await booksApi.delete(book.id);
      showToast(`Đã xóa đầu sách "${book.title}"`);
      navigate(`${portalPath}/books`, { replace: true });
    } catch (error: unknown) {
      showToast(error instanceof Error ? error.message : 'Lỗi khi xóa sách', 'error');
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="tab-container">
      <div className="section-header">
        <h2>📖 Thông Tin Đầu Sách</h2>
        <Link className="btn btn-secondary" to={`${portalPath}/books`}>
          <ArrowLeft size={18} /> Quay lại danh sách
        </Link>
      </div>

      {result?.id !== bookId ? (
        <div className="loading-banner" role="status">
          <RefreshCw size={18} className="spin" /> Đang tải thông tin sách...
        </div>
      ) : result?.error ? (
        <div className="empty-state" role="alert"><p>{result.error}</p></div>
      ) : book ? (
        <article className="table-card book-details">
          <div className="book-details-cover">
            {book.imageUrl && failedImage !== book.imageUrl ? (
              <img src={book.imageUrl} alt={`Bìa sách ${book.title}`} onError={() => setFailedImage(book.imageUrl ?? null)} />
            ) : (
              <BookOpen size={64} aria-label="Chưa có ảnh bìa" />
            )}
          </div>
          <div className="book-details-info">
            <h3>{book.title}</h3>
            <dl className="book-details-fields">
              <dt>Tác giả</dt><dd>{book.author}</dd>
              <dt>Thể loại</dt><dd>{book.categoryName || '—'}</dd>
              <dt>ISBN</dt><dd>{book.isbn || '—'}</dd>
              <dt>Năm xuất bản</dt><dd>{book.publishYear ?? '—'}</dd>
              <dt>Tổng bản sao</dt><dd>{book.totalCopies ?? 0} cuốn</dd>
              <dt>Có sẵn trên kệ</dt><dd>{book.availableCopies ?? 0} cuốn</dd>
            </dl>
            <div className="book-details-actions">
              <Link className="btn btn-primary" to={`${portalPath}/book-copies?bookId=${encodeURIComponent(String(book.id))}`}>
                <Layers size={18} /> Xem bản sao
              </Link>
              {canEdit && (
                <button className="btn btn-secondary" disabled={busy} onClick={handleEdit}>
                  <Edit2 size={18} /> Chỉnh sửa
                </button>
              )}
              {canDelete && (
                <button className="btn-action btn-danger" disabled={busy} onClick={handleDelete}>
                  <Trash2 size={18} /> Xóa
                </button>
              )}
            </div>
          </div>
        </article>
      ) : null}
      {editingBook && (
        <BookFormModal
          book={editingBook}
          categories={categories}
          onClose={() => setEditingBook(null)}
          onSaved={(saved) => setResult({ id: String(saved.id), book: saved })}
          onNotify={showToast}
        />
      )}
    </div>
  );
};
