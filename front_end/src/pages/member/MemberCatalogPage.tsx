import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Book, Category, BookCopy } from '../../types';
import { booksApi, categoriesApi, bookCopiesApi, borrowRecordsApi } from '../../api';
import { useToast } from '../../contexts/ToastContext';
import {
  Search,
  BookOpen,
  CheckCircle2,
  AlertCircle,
  Bookmark,
  Layers,
  Calendar,
  RefreshCw,
  X,
  Clock,
  Sparkles
} from 'lucide-react';

export const MemberCatalogPage: React.FC = () => {
  const navigate = useNavigate();
  const { showToast } = useToast();

  const [loading, setLoading] = useState(true);
  const [books, setBooks] = useState<Book[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);

  // Search & Filter state
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  const [onlyAvailable, setOnlyAvailable] = useState<boolean>(false);
  const [sortBy, setSortBy] = useState<'title' | 'available' | 'year'>('available');

  // Copy Modal state
  const [viewingCopiesBook, setViewingCopiesBook] = useState<Book | null>(null);
  const [bookCopies, setBookCopies] = useState<BookCopy[]>([]);
  const [loadingCopies, setLoadingCopies] = useState(false);

  // Borrow Modal state
  const [borrowModalBook, setBorrowModalBook] = useState<Book | null>(null);
  const [selectedCopyId, setSelectedCopyId] = useState<string>('');
  const [availableCopiesForBorrow, setAvailableCopiesForBorrow] = useState<BookCopy[]>([]);
  const [dueDate, setDueDate] = useState<string>(() => {
    const d = new Date();
    d.setDate(d.getDate() + 14);
    return d.toISOString().split('T')[0];
  });
  const [borrowNote, setBorrowNote] = useState('');
  const [borrowSubmitting, setBorrowSubmitting] = useState(false);

  // Load catalog & categories
  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [booksRes, catsRes] = await Promise.all([
        booksApi.getAll({ onlyAvailable: onlyAvailable || undefined }),
        categoriesApi.getAll(),
      ]);
      setBooks(booksRes);
      setCategories(catsRes);
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi tải danh mục sách', 'error');
    } finally {
      setLoading(false);
    }
  }, [onlyAvailable, showToast]);

  useEffect(() => {
    loadData();
  }, [loadData]);

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

  // Open Borrow Modal
  const handleOpenBorrowModal = async (book: Book, preselectedCopyId?: string | number) => {
    setBorrowModalBook(book);
    setSelectedCopyId(preselectedCopyId ? String(preselectedCopyId) : '');
    setBorrowNote('');
    // Reset default due date (+14 days)
    const d = new Date();
    d.setDate(d.getDate() + 14);
    setDueDate(d.toISOString().split('T')[0]);

    // Load available copies for selection
    try {
      const copies = await bookCopiesApi.getByBookId(book.id);
      const avail = copies.filter((c) => c.status === 'Available');
      setAvailableCopiesForBorrow(avail);
    } catch {
      setAvailableCopiesForBorrow([]);
    }
  };

  // Quick date selector helper
  const setQuickDueDate = (days: number) => {
    const d = new Date();
    d.setDate(d.getDate() + days);
    setDueDate(d.toISOString().split('T')[0]);
  };

  // Submit borrow request
  const handleConfirmBorrow = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!borrowModalBook) return;

    if (!dueDate) {
      showToast('Vui lòng chọn ngày hẹn trả', 'error');
      return;
    }

    setBorrowSubmitting(true);
    try {
      await borrowRecordsApi.borrowBookForMember({
        bookId: borrowModalBook.id,
        bookCopyId: selectedCopyId ? Number(selectedCopyId) : undefined,
        dueDate: new Date(dueDate).toISOString(),
        note: borrowNote.trim() || undefined,
      });

      showToast(`Đã mượn thành công sách "${borrowModalBook.title}"!`, 'success');
      setBorrowModalBook(null);
      if (viewingCopiesBook) {
        setViewingCopiesBook(null);
      }
      // Refresh catalog data to update stock counts
      await loadData();
    } catch (err: any) {
      showToast(err.message || 'Mượn sách không thành công. Vui lòng thử lại!', 'error');
    } finally {
      setBorrowSubmitting(false);
    }
  };

  // Filtered & Sorted books
  const filteredBooks = useMemo(() => {
    return books
      .filter((book) => {
        const matchesSearch =
          book.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
          book.author.toLowerCase().includes(searchTerm.toLowerCase()) ||
          (book.isbn && book.isbn.toLowerCase().includes(searchTerm.toLowerCase()));

        const matchesCat =
          selectedCategory === 'all' || String(book.categoryId) === String(selectedCategory);

        const matchesAvail = !onlyAvailable || (book.availableCopies ?? 0) > 0;

        return matchesSearch && matchesCat && matchesAvail;
      })
      .sort((a, b) => {
        if (sortBy === 'available') {
          return (b.availableCopies ?? 0) - (a.availableCopies ?? 0);
        }
        if (sortBy === 'year') {
          return (b.publishYear ?? 0) - (a.publishYear ?? 0);
        }
        return a.title.localeCompare(b.title);
      });
  }, [books, searchTerm, selectedCategory, onlyAvailable, sortBy]);

  // Statistics
  const totalBooksCount = books.length;
  const inStockBooksCount = books.filter((b) => (b.availableCopies ?? 0) > 0).length;
  const totalAvailableCopies = books.reduce((acc, b) => acc + (b.availableCopies ?? 0), 0);

  return (
    <div className="tab-container">
      {/* Page Header */}
      <div className="section-header">
        <div>
          <h2>📚 Tra Cứu & Mượn Sách Trực Tuyến</h2>
          <p className="section-desc">
            Khám phá các đầu sách hiện có tại thư viện, theo dõi số lượng sách còn trên kệ và đăng ký mượn ngay.
          </p>
        </div>
        <div style={{ display: 'flex', gap: '8px' }}>
          <button
            className="btn btn-secondary"
            onClick={loadData}
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
            <span className="stat-label">Tổng đầu sách</span>
            <span className="stat-value">{totalBooksCount}</span>
          </div>
        </div>

        <div className="stat-card" style={{ cursor: 'default' }}>
          <div className="stat-icon" style={{ backgroundColor: '#dcfce7', color: '#16a34a' }}>
            <CheckCircle2 size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-label">Sách còn trên kệ</span>
            <span className="stat-value">{inStockBooksCount}</span>
          </div>
        </div>

        <div className="stat-card" style={{ cursor: 'default' }}>
          <div className="stat-icon" style={{ backgroundColor: '#fef3c7', color: '#d97706' }}>
            <Sparkles size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-label">Bản sao sẵn sàng mượn</span>
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
            onChange={(e) => setSearchTerm(e.target.value)}
          />
          {searchTerm && (
            <button
              className="btn-icon"
              onClick={() => setSearchTerm('')}
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
            onChange={(e) => setSelectedCategory(e.target.value)}
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
            onChange={(e) => setSortBy(e.target.value as any)}
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
            onChange={(e) => setOnlyAvailable(e.target.checked)}
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
      ) : filteredBooks.length === 0 ? (
        <div className="empty-state">
          <BookOpen size={48} className="empty-icon" />
          <p>Không tìm thấy sách nào phù hợp với bộ lọc hiện tại</p>
          <button
            className="btn btn-secondary"
            onClick={() => {
              setSearchTerm('');
              setSelectedCategory('all');
              setOnlyAvailable(false);
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
          {filteredBooks.map((book) => {
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

                  <div style={{ fontSize: '13px', color: '#64748b', marginBottom: '14px', display: 'flex', flexDirection: 'column', gap: '4px' }}>
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
                    style={{ flex: 1, fontSize: '13px', justifyContent: 'center' }}
                    onClick={() => handleOpenCopiesModal(book)}
                  >
                    <Layers size={15} /> Xem bản sao
                  </button>

                  <button
                    className="btn btn-primary"
                    style={{
                      flex: 1,
                      fontSize: '13px',
                      justifyContent: 'center',
                      opacity: isAvailable ? 1 : 0.6,
                      cursor: isAvailable ? 'pointer' : 'not-allowed',
                    }}
                    disabled={!isAvailable}
                    onClick={() => handleOpenBorrowModal(book)}
                  >
                    <BookOpen size={15} /> Mượn sách
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* ── MODAL 1: VIEW SHELF COPIES ────────────────────────────────────────── */}
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
                    Danh sách các cuốn sách vật lý đang được quản lý. Bạn có thể chọn ngay một bản sao có trạng thái <strong>Sẵn sàng</strong> để mượn.
                  </p>
                  <table className="custom-table">
                    <thead>
                      <tr>
                        <th>Mã vạch (Barcode)</th>
                        <th>Tình trạng vật lý</th>
                        <th>Trạng thái</th>
                        <th style={{ textAlign: 'right' }}>Hành động</th>
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
                              {isAvail && <span className="badge badge-available">Sẵn sàng</span>}
                              {copy.status === 'Borrowed' && <span className="badge badge-borrowed">Đang mượn</span>}
                              {copy.status === 'Lost' && <span className="badge badge-lost">Đã mất</span>}
                            </td>
                            <td style={{ textAlign: 'right' }}>
                              {isAvail ? (
                                <button
                                  className="btn btn-action btn-copy"
                                  onClick={() => handleOpenBorrowModal(viewingCopiesBook, copy.id)}
                                >
                                  <BookOpen size={13} /> Mượn cuốn này
                                </button>
                              ) : (
                                <span style={{ fontSize: '12px', color: '#94a3b8' }}>Không khả dụng</span>
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

      {/* ── MODAL 2: BORROW CONFIRMATION MODAL ─────────────────────────────────── */}
      {borrowModalBook && (
        <div className="modal-backdrop" onClick={() => !borrowSubmitting && setBorrowModalBook(null)}>
          <div className="modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Đăng Ký Mượn Sách</h3>
              <button
                className="btn-icon"
                disabled={borrowSubmitting}
                onClick={() => setBorrowModalBook(null)}
              >
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleConfirmBorrow}>
              <div className="form-content">
                {/* Book Details Summary Box */}
                <div
                  style={{
                    backgroundColor: '#fffbeb',
                    border: '1px solid #fef3c7',
                    borderRadius: '10px',
                    padding: '14px 16px',
                    display: 'flex',
                    gap: '12px',
                    alignItems: 'center',
                  }}
                >
                  <div
                    style={{
                      backgroundColor: '#f59e0b',
                      color: '#fff',
                      padding: '10px',
                      borderRadius: '8px',
                      display: 'flex',
                    }}
                  >
                    <BookOpen size={24} />
                  </div>
                  <div>
                    <h4 style={{ margin: '0 0 4px 0', fontSize: '15px', color: '#92400e' }}>
                      {borrowModalBook.title}
                    </h4>
                    <p style={{ margin: 0, fontSize: '13px', color: '#b45309' }}>
                      Tác giả: {borrowModalBook.author} • Còn {borrowModalBook.availableCopies ?? 0} cuốn khả dụng
                    </p>
                  </div>
                </div>

                {/* Copy Selection */}
                <div className="form-group">
                  <label>Bản sao muốn mượn</label>
                  <select
                    value={selectedCopyId}
                    onChange={(e) => setSelectedCopyId(e.target.value)}
                  >
                    <option value="">✨ Tự động chọn bản sao sẵn có tốt nhất</option>
                    {availableCopiesForBorrow.map((c) => (
                      <option key={c.id} value={c.id}>
                        Mã vạch: {c.barcode} ({c.conditionNote || 'Tình trạng tốt'})
                      </option>
                    ))}
                  </select>
                  <small style={{ color: '#64748b', fontSize: '12px' }}>
                    Nếu bạn để mặc định, hệ thống sẽ tự động gán một cuốn sách có sẵn trên kệ cho bạn.
                  </small>
                </div>

                {/* Due Date & Quick Buttons */}
                <div className="form-group">
                  <label>
                    <Calendar size={14} /> Ngày hẹn trả sách *
                  </label>
                  <input
                    type="date"
                    required
                    min={new Date().toISOString().split('T')[0]}
                    value={dueDate}
                    onChange={(e) => setDueDate(e.target.value)}
                  />

                  {/* Quick duration buttons */}
                  <div style={{ display: 'flex', gap: '8px', marginTop: '6px' }}>
                    <button
                      type="button"
                      className="btn-action"
                      onClick={() => setQuickDueDate(7)}
                    >
                      <Clock size={12} /> +7 ngày
                    </button>
                    <button
                      type="button"
                      className="btn-action"
                      style={{ borderColor: '#f59e0b', color: '#b45309', backgroundColor: '#fffbeb' }}
                      onClick={() => setQuickDueDate(14)}
                    >
                      <Clock size={12} /> +14 ngày (chuẩn)
                    </button>
                    <button
                      type="button"
                      className="btn-action"
                      onClick={() => setQuickDueDate(30)}
                    >
                      <Clock size={12} /> +30 ngày
                    </button>
                  </div>
                </div>

                {/* Note */}
                <div className="form-group">
                  <label>Ghi chú mượn (nếu có)</label>
                  <textarea
                    rows={2}
                    placeholder="Ví dụ: Mượn phục vụ viết luận văn, mượn đọc tại nhà..."
                    value={borrowNote}
                    onChange={(e) => setBorrowNote(e.target.value)}
                  />
                </div>
              </div>

              {/* Modal Actions */}
              <div
                style={{
                  padding: '16px 24px',
                  borderTop: '1px solid #e2e8f0',
                  display: 'flex',
                  justifyContent: 'flex-end',
                  gap: '10px',
                  backgroundColor: '#f8fafc',
                }}
              >
                <button
                  type="button"
                  className="btn btn-secondary"
                  disabled={borrowSubmitting}
                  onClick={() => setBorrowModalBook(null)}
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={borrowSubmitting}
                >
                  {borrowSubmitting ? (
                    <>
                      <RefreshCw size={16} className="spin" /> Đang tạo phiếu mượn...
                    </>
                  ) : (
                    <>
                      <CheckCircle2 size={16} /> Xác nhận mượn sách
                    </>
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
