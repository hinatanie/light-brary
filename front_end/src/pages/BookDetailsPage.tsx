import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ArrowLeft, BookOpen, Layers, RefreshCw } from 'lucide-react';
import { ApiError, booksApi } from '../api';
import type { Book } from '../types';

export const BookDetailsPage = ({ portalPath }: { portalPath: '/admin' | '/staff' }) => {
  const { bookId } = useParams<{ bookId: string }>();
  const [result, setResult] = useState<{ id: string; book?: Book; error?: string } | null>(null);
  const [failedImage, setFailedImage] = useState<string | null>(null);

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
            <Link className="btn btn-primary" to={`${portalPath}/book-copies?bookId=${encodeURIComponent(String(book.id))}`}>
              <Layers size={18} /> Xem bản sao
            </Link>
          </div>
        </article>
      ) : null}
    </div>
  );
};
