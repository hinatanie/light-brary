import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Book, BookCopy, BookSort } from '../../types';
import { bookCopiesApi } from '../../api';
import { usePagedBooks } from '../../hooks/usePagedBooks';
import { Pagination } from '../../components/Pagination';
import { useToast } from '../../contexts/ToastContext';
import {
  Search,
  BookOpen,
  CheckCircle2,
  AlertCircle,
  Bookmark,
  Layers,
  RefreshCw,
  X,
  Sparkles
} from 'lucide-react';

export const MemberCatalogPage: React.FC = () => {
  const navigate = useNavigate();
  const { showToast } = useToast();

  const { items: books, categories, query, pageNumber, pageSize, totalCount, totalPages,
    loading, error, setFilters, setPage, refresh } = usePagedBooks('available');
  const searchTerm = query.keyword ?? '';
  const selectedCategory = query.categoryId ?? 'all';
  const onlyAvailable = query.onlyAvailable ?? false;
  const sortBy = query.sortBy;

  // Copy Modal state
  const [viewingCopiesBook, setViewingCopiesBook] = useState<Book | null>(null);
  const [bookCopies, setBookCopies] = useState<BookCopy[]>([]);
  const [loadingCopies, setLoadingCopies] = useState(false);

  // Open Copies Modal
  const handleOpenCopiesModal = async (book: Book) => {
    setViewingCopiesBook(book);
    setLoadingCopies(true);
    try {
      const copies = await bookCopiesApi.getByBookId(book.id);
      setBookCopies(copies);
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi tải danh sách bản sao', 'error');
      setBookCopies([]);
    } finally {
      setLoadingCopies(false);
    }
  };

  // Statistics
  const totalBooksCount = totalCount;
  const inStockBooksCount = books.filter((b) => (b.availableCopies ?? 0) > 0).length;
  const totalAvailableCopies = books.reduce((acc, b) => acc + (b.availableCopies ?? 0), 0);

  return (
    <div className="tab-container">
      {/* Page Header */}
      <div className="section-header">
        <div>
          <h2>📚 Tra Cứu Danh Mục Sách</h2>
          <p className="section-desc">
            Khám phá các đầu sách hiện có tại thư viện và kiểm tra tình trạng sách còn trên kệ. Để mượn sách, quý độc giả vui lòng liên hệ trực tiếp tại Quầy Thủ Thư.
          </p>
        </div>
        <div style={{ display: 'flex', gap: '8px' }}>
          <button
            className="btn btn-secondary"
            onClick={refresh}
            disabled={loading}
          >
            <RefreshCw size={16} className={loading ? 'spin' : ''} /> Làm mới
          </button>
          <button
            className="btn btn-primary"
            onClick={() => navigate('/member')}
          >
            <BookOpen size={16} /> Sách đang mượn của tôi
          </button>
        </div>
      </div>

      {/* Overview Stats */}
      <div className="stats-grid" style={{ marginBottom: '20px' }}>
        <div className="stat-card" style={{ cursor: 'default' }}>
          <div className="stat-icon" style={{ backgroundColor: '#e0f2fe', color: '#0284c7' }}>
            <BookOpen size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-label">Tổng đầu sách phù hợp</span>
            <span className="stat-value">{totalBooksCount}</span>
          </div>
        </div>

        <div className="stat-card" style={{ cursor: 'default' }}>
          <div className="stat-icon" style={{ backgroundColor: '#dcfce7', color: '#16a34a' }}>
            <CheckCircle2 size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-label">Sách còn trên kệ (trang này)</span>
            <span className="stat-value">{inStockBooksCount}</span>
          </div>
        </div>

        <div className="stat-card" style={{ cursor: 'default' }}>
          <div className="stat-icon" style={{ backgroundColor: '#fef3c7', color: '#d97706' }}>
            <Sparkles size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-label">Bản sao sẵn sàng mượn (trang này)</span>
            <span className="stat-value">{totalAvailableCopies}</span>
          </div>
        </div>
      </div>

      {/* Search & Filter Toolbar */}
      <div
        className="filter-bar"
        style={{
          display: 'flex',
          flexWrap: 'wrap',
          gap: '12px',
          alignItems: 'center',
          backgroundColor: '#fff',
          padding: '16px',
          borderRadius: '12px',
          border: '1px solid #e2e8f0',
          marginBottom: '20px',
        }}
      >
        {/* Search Input */}
        <div className="search-input-wrap" style={{ flex: '1 1 260px' }}>
          <Search size={18} className="search-icon" />
          <input
            type="text"
            placeholder="Tìm theo tên sách, tác giả hoặc ISBN..."
            value={searchTerm}
            onChange={(e) => setFilters({ keyword: e.target.value })}
          />
          {searchTerm && (
            <button
              className="btn-icon"
              onClick={() => setFilters({ keyword: '' })}
              style={{ position: 'absolute', right: '8px', top: '50%', transform: 'translateY(-50%)' }}
            >
              <X size={16} />
            </button>
          )}
        </div>

        {/* Category Dropdown */}
        <div style={{ flex: '0 1 200px' }}>
          <select
            className="filter-select"
            value={selectedCategory}
            onChange={(e) => setFilters({ categoryId: e.target.value === 'all' ? undefined : e.target.value })}
            style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', border: '1px solid #cbd5e1' }}
          >
            <option value="all">📂 Tất cả thể loại</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </div>

        {/* Sort By Dropdown */}
        <div style={{ flex: '0 1 180px' }}>
          <select
            className="filter-select"
            value={sortBy}
            onChange={(e) => setFilters({ sortBy: e.target.value as BookSort })}
            style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', border: '1px solid #cbd5e1' }}
          >
            <option value="available">⚡ Sẵn có nhiều nhất</option>
            <option value="title">🔤 Tên sách (A-Z)</option>
            <option value="year">📅 Năm xuất bản mới</option>
          </select>
        </div>

        {/* Only Available Toggle */}
        <label
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
            fontSize: '13px',
            fontWeight: 600,
            color: '#334155',
            cursor: 'pointer',
            padding: '8px 12px',
            borderRadius: '8px',
            backgroundColor: onlyAvailable ? '#ecfdf5' : '#f8fafc',
            border: `1px solid ${onlyAvailable ? '#a7f3d0' : '#e2e8f0'}`,
            userSelect: 'none',
          }}
        >
          <input
            type="checkbox"
            checked={onlyAvailable}
            onChange={(e) => setFilters({ onlyAvailable: e.target.checked })}
            style={{ accentColor: '#10b981', cursor: 'pointer' }}
          />
          <span>Chỉ hiện sách còn bản sao</span>
        </label>
      </div>

      {/* Loading indicator */}
      {loading ? (
        <div className="loading-banner">
          <RefreshCw size={18} className="spin" />
          <span>Đang tải danh mục sách và số lượng tồn kho...</span>
        </div>
      ) : error ? (
        <div className="empty-state" role="alert">
          <p>{error}</p>
          <button className="btn btn-secondary" onClick={refresh}>Thử lại</button>
        </div>
      ) : books.length === 0 ? (
        <div className="empty-state">
          <BookOpen size={48} className="empty-icon" />
          <p>Không tìm thấy sách nào phù hợp với bộ lọc hiện tại</p>
          <button
            className="btn btn-secondary"
            onClick={() => {
              setFilters({ keyword: '', categoryId: undefined, onlyAvailable: false });
            }}
          >
            Xóa bộ lọc tìm kiếm
          </button>
        </div>
      ) : (
        /* Books Catalog Grid */
        <div
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))',
            gap: '20px',
          }}
        >
          {books.map((book) => {
            const total = book.totalCopies ?? 0;
            const available = book.availableCopies ?? 0;
            const isAvailable = available > 0;
            const categoryObj = categories.find((c) => String(c.id) === String(book.categoryId));
            const categoryName = book.categoryName || categoryObj?.name || 'Khác';

            return (
              <div
                key={book.id}
                style={{
                  backgroundColor: '#fff',
                  border: '1px solid #e2e8f0',
                  borderRadius: '14px',
                  display: 'flex',
                  flexDirection: 'column',
                  overflow: 'hidden',
                  boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.05)',
                  transition: 'transform 0.15s ease, box-shadow 0.15s ease',
                }}
              >
                {/* Book Card Top Banner */}
                <div
                  style={{
                    padding: '16px 20px',
                    borderBottom: '1px solid #f1f5f9',
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'flex-start',
                    gap: '12px',
                    backgroundColor: '#fafaf9',
                  }}
                >
                  <span className="category-tag">
                    <Bookmark size={12} style={{ display: 'inline', marginRight: '4px', verticalAlign: '-1px' }} />
                    {categoryName}
                  </span>

                  {isAvailable ? (
                    <span className="badge badge-available">
                      <CheckCircle2 size={12} style={{ display: 'inline', marginRight: '4px', verticalAlign: '-1px' }} />
                      Còn {available} / {total} cuốn
                    </span>
                  ) : (
                    <span className="badge badge-lost">
                      <AlertCircle size={12} style={{ display: 'inline', marginRight: '4px', verticalAlign: '-1px' }} />
                      Hết sách (0/{total})
                    </span>
                  )}
                </div>

                {/* Book Card Body */}
                <div style={{ padding: '20px', flex: 1, display: 'flex', flexDirection: 'column' }}>
                  <div style={{ display: 'flex', gap: '14px', marginBottom: '14px' }}>
                    {book.imageUrl ? (
                      <img
                        src={book.imageUrl}
                        alt={book.title}
                        onError={(e) => {
                          (e.currentTarget as HTMLImageElement).style.display = 'none';
                        }}
                        style={{
                          width: '68px',
                          height: '92px',
                          objectFit: 'cover',
                          borderRadius: '8px',
                          boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
                          flexShrink: 0,
                          backgroundColor: '#f1f5f9',
                          border: '1px solid #e2e8f0',
                        }}
                      />
                    ) : null}
                    <div style={{ flex: 1 }}>
                      <h3
                        style={{
                          fontSize: '17px',
                          fontWeight: 700,
                          color: '#0f172a',
                          marginBottom: '8px',
                          lineHeight: '1.4',
                        }}
                      >
                        {book.title}
                      </h3>

                      <div style={{ fontSize: '13px', color: '#64748b', display: 'flex', flexDirection: 'column', gap: '4px' }}>
                        <div>
                          <strong>Tác giả:</strong> {book.author}
                        </div>
                        {book.isbn && (
                          <div>
                            <strong>ISBN:</strong> <span style={{ fontFamily: 'monospace' }}>{book.isbn}</span>
                          </div>
                        )}
                        {book.publishYear && (
                          <div>
                            <strong>Năm XB:</strong> {book.publishYear}
                          </div>
                        )}
                      </div>
                    </div>
                  </div>

                  {/* Stock Availability Bar */}
                  <div style={{ marginTop: 'auto', paddingTop: '12px', borderTop: '1px dashed #e2e8f0' }}>
                    <div
                      style={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        fontSize: '12px',
                        marginBottom: '6px',
                        color: '#475569',
                      }}
                    >
                      <span>Khả dụng trên kệ:</span>
                      <strong style={{ color: isAvailable ? '#15803d' : '#dc2626' }}>
                        {available} cuốn sẵn sàng ({total} tổng số)
                      </strong>
                    </div>

                    <div
                      style={{
                        width: '100%',
                        height: '6px',
                        backgroundColor: '#e2e8f0',
                        borderRadius: '3px',
                        overflow: 'hidden',
                      }}
                    >
                      <div
                        style={{
                          height: '100%',
                          width: `${total > 0 ? Math.min(100, Math.round((available / total) * 100)) : 0}%`,
                          backgroundColor: isAvailable ? '#10b981' : '#ef4444',
                          transition: 'width 0.3s ease',
                        }}
                      />
                    </div>
                  </div>
                </div>

                {/* Book Card Actions */}
                <div
                  style={{
                    padding: '12px 20px',
                    backgroundColor: '#f8fafc',
                    borderTop: '1px solid #f1f5f9',
                    display: 'flex',
                    gap: '8px',
                  }}
                >
                  <button
                    className="btn btn-secondary"
                    style={{ width: '100%', fontSize: '13px', justifyContent: 'center' }}
                    onClick={() => handleOpenCopiesModal(book)}
                  >
                    <Layers size={15} /> Xem bản sao trên kệ ({book.availableCopies ?? 0} cuốn khả dụng)
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* ── MODAL 1: VIEW SHELF COPIES ────────────────────────────────────────── */}
      <Pagination page={pageNumber} pageSize={pageSize} totalPages={totalPages}
        totalCount={totalCount} disabled={loading || !!error} onPageChange={setPage} />

      {viewingCopiesBook && (
        <div className="modal-backdrop" onClick={() => setViewingCopiesBook(null)}>
          <div
            className="modal-box modal-lg"
            onClick={(e) => e.stopPropagation()}
            style={{ maxWidth: '680px' }}
          >
            <div className="modal-header">
              <div>
                <h3 style={{ margin: 0, fontSize: '18px' }}>
                  📖 Bản Sao Trên Kệ: {viewingCopiesBook.title}
                </h3>
                <span style={{ fontSize: '13px', color: '#64748b' }}>
                  Tác giả: {viewingCopiesBook.author}
                </span>
              </div>
              <button className="btn-icon" onClick={() => setViewingCopiesBook(null)}>
                <X size={20} />
              </button>
            </div>

            <div className="form-content">
              {loadingCopies ? (
                <div className="loading-banner">
                  <RefreshCw size={18} className="spin" />
                  <span>Đang tải thông tin bản sao...</span>
                </div>
              ) : bookCopies.length === 0 ? (
                <div className="empty-state" style={{ padding: '24px 0' }}>
                  <Layers size={36} className="empty-icon" />
                  <p>Hiện chưa có bản sao nào được nhập vào hệ thống cho đầu sách này.</p>
                </div>
              ) : (
                <>
                  <p style={{ fontSize: '13px', color: '#475569', margin: '0 0 12px 0' }}>
                    Danh sách các cuốn sách vật lý đang được quản lý trên kệ thư viện:
                  </p>
                  <table className="custom-table">
                    <thead>
                      <tr>
                        <th>Mã vạch (Barcode)</th>
                        <th>Tình trạng vật lý</th>
                        <th>Trạng thái kệ</th>
                        <th style={{ textAlign: 'right' }}>Hướng dẫn mượn</th>
                      </tr>
                    </thead>
                    <tbody>
                      {bookCopies.map((copy) => {
                        const isAvail = copy.status === 'Available';
                        return (
                          <tr key={copy.id}>
                            <td>
                              <span className="barcode-badge">{copy.barcode}</span>
                            </td>
                            <td>
                              <span style={{ fontSize: '13px', color: '#334155' }}>
                                {copy.conditionNote || 'Tốt / Bình thường'}
                              </span>
                            </td>
                            <td>
                              {isAvail && <span className="badge badge-available">Có sẵn trên kệ</span>}
                              {copy.status === 'Borrowed' && <span className="badge badge-borrowed">Đang được mượn</span>}
                              {copy.status === 'Lost' && <span className="badge badge-lost">Đã báo mất</span>}
                            </td>
                            <td style={{ textAlign: 'right' }}>
                              {isAvail ? (
                                <span style={{ fontSize: '12px', color: '#059669', fontWeight: 500 }}>
                                  Liên hệ Quầy Thủ Thư
                                </span>
                              ) : (
                                <span style={{ fontSize: '12px', color: 'var(--text-muted)' }}>
                                  Chưa thể mượn
                                </span>
                              )}
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </>
              )}
            </div>

            <div
              style={{
                padding: '16px 24px',
                borderTop: '1px solid #e2e8f0',
                display: 'flex',
                justifyContent: 'flex-end',
                backgroundColor: '#f8fafc',
              }}
            >
              <button className="btn btn-secondary" onClick={() => setViewingCopiesBook(null)}>
                Đóng
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
