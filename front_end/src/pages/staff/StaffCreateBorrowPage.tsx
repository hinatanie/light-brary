import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Book, BookCopy, User, Category } from '../../types';
import { booksApi, categoriesApi, usersApi, bookCopiesApi, borrowRecordsApi } from '../../api';
import { useToast } from '../../contexts/ToastContext';
import {
  Search,
  BookOpen,
  User as UserIcon,
  Barcode,
  Trash2,
  CheckCircle2,
  RotateCcw,
  RefreshCw,
  Plus,
  ChevronDown,
  ChevronUp,
  Clock,
} from 'lucide-react';

interface SelectedCopyItem {
  copyId: string;
  barcode: string;
  bookId: string | number;
  bookTitle: string;
  bookAuthor?: string;
  conditionNote?: string | null;
}

export const StaffCreateBorrowPage: React.FC = () => {
  const { showToast } = useToast();
  const navigate = useNavigate();

  // ─── Data states ───
  const [loading, setLoading] = useState(true);
  const [books, setBooks] = useState<Book[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [users, setUsers] = useState<User[]>([]);

  // ─── Left column filter & search states ───
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  const [onlyAvailable, setOnlyAvailable] = useState(false);
  const [quickBarcodeInput, setQuickBarcodeInput] = useState('');
  const [scanningBarcode, setScanningBarcode] = useState(false);

  // Cached copies per book ID: bookId -> BookCopy[]
  const [copiesCache, setCopiesCache] = useState<Record<string, BookCopy[]>>({});
  const [loadingCopiesFor, setLoadingCopiesFor] = useState<Record<string, boolean>>({});
  const [expandedBookIds, setExpandedBookIds] = useState<Record<string, boolean>>({});

  // ─── Right column (Ticket builder) states ───
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [userSearchTerm, setUserSearchTerm] = useState('');
  const [isUserDropdownOpen, setIsUserDropdownOpen] = useState(false);

  // Due date default: +14 days
  const [dueDate, setDueDate] = useState<string>(() => {
    const d = new Date();
    d.setDate(d.getDate() + 14);
    return d.toISOString().split('T')[0];
  });
  const [selectedCopies, setSelectedCopies] = useState<SelectedCopyItem[]>([]);
  const [note, setNote] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [successResultModal, setSuccessResultModal] = useState<{
    recordId: string | number;
    userName: string;
    bookCount: number;
    dueDate: string;
  } | null>(null);

  // ─── Load initial master data ───
  const loadInitialData = useCallback(async () => {
    setLoading(true);
    try {
      const [fetchedBooks, fetchedCategories, fetchedUsers] = await Promise.all([
        booksApi.getAll(),
        categoriesApi.getAll().catch(() => [] as Category[]),
        usersApi.getAll().catch(() => [] as User[]),
      ]);
      setBooks(fetchedBooks);
      setCategories(fetchedCategories);
      setUsers(fetchedUsers);
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi tải dữ liệu khởi tạo', 'error');
    } finally {
      setLoading(false);
    }
  }, [showToast]);

  useEffect(() => {
    loadInitialData();
  }, [loadInitialData]);

  // ─── Fetch copies for a specific book when expanded ───
  const toggleExpandBook = async (bookId: string | number) => {
    const bIdStr = String(bookId);
    const isCurrentlyExpanded = Boolean(expandedBookIds[bIdStr]);

    if (!isCurrentlyExpanded && !copiesCache[bIdStr]) {
      // Need to fetch copies
      setLoadingCopiesFor((prev) => ({ ...prev, [bIdStr]: true }));
      try {
        const copies = await bookCopiesApi.getByBookId(bookId);
        setCopiesCache((prev) => ({ ...prev, [bIdStr]: copies }));
      } catch (err: any) {
        showToast(`Không thể tải bản sao của sách: ${err.message}`, 'error');
      } finally {
        setLoadingCopiesFor((prev) => ({ ...prev, [bIdStr]: false }));
      }
    }

    setExpandedBookIds((prev) => ({
      ...prev,
      [bIdStr]: !isCurrentlyExpanded,
    }));
  };

  // ─── Add copy to ticket ───
  const handleAddCopy = (copy: BookCopy, book: Book) => {
    const copyIdStr = String(copy.id);
    if (selectedCopies.some((item) => item.copyId === copyIdStr)) {
      showToast('Cuốn sách này đã có trong phiếu mượn!', 'info');
      return;
    }
    if (copy.status !== 'Available') {
      showToast('Bản sao này hiện không có sẵn để mượn (đang mượn hoặc báo mất)!', 'error');
      return;
    }

    const newItem: SelectedCopyItem = {
      copyId: copyIdStr,
      barcode: copy.barcode,
      bookId: book.id,
      bookTitle: book.title,
      bookAuthor: book.author,
      conditionNote: copy.conditionNote,
    };

    setSelectedCopies((prev) => [...prev, newItem]);
    showToast(`Đã thêm "${book.title}" (Mã: ${copy.barcode}) vào phiếu`, 'success');
  };

  // ─── Remove copy from ticket ───
  const handleRemoveCopy = (copyId: string) => {
    setSelectedCopies((prev) => prev.filter((item) => item.copyId !== copyId));
  };

  // ─── Barcode scanner fast add ───
  const handleBarcodeQuickAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    const barcode = quickBarcodeInput.trim();
    if (!barcode) return;

    if (selectedCopies.some((item) => item.barcode.toLowerCase() === barcode.toLowerCase())) {
      showToast(`Mã vạch "${barcode}" đã có trong phiếu mượn!`, 'info');
      setQuickBarcodeInput('');
      return;
    }

    setScanningBarcode(true);
    try {
      const copy = await bookCopiesApi.getByBarcode(barcode);
      if (!copy) {
        showToast(`Không tìm thấy bản sao với mã vạch: "${barcode}"`, 'error');
        return;
      }

      if (copy.status !== 'Available') {
        showToast(
          `Bản sao mã vạch "${barcode}" không khả dụng (Trạng thái: ${copy.status})!`,
          'error'
        );
        return;
      }

      // Find corresponding book title if available
      let bookTitle = copy.bookTitle;
      let bookAuthor = copy.bookAuthor;
      if (!bookTitle) {
        const foundBook = books.find((b) => String(b.id) === String(copy.bookId));
        if (foundBook) {
          bookTitle = foundBook.title;
          bookAuthor = foundBook.author;
        } else {
          bookTitle = `Sách #${copy.bookId}`;
        }
      }

      const newItem: SelectedCopyItem = {
        copyId: String(copy.id),
        barcode: copy.barcode,
        bookId: copy.bookId,
        bookTitle: bookTitle || 'Không rõ tên sách',
        bookAuthor: bookAuthor,
        conditionNote: copy.conditionNote,
      };

      setSelectedCopies((prev) => [...prev, newItem]);
      showToast(`Đã quét thành công mã vạch "${barcode}"!`, 'success');
      setQuickBarcodeInput('');
    } catch (err: any) {
      showToast(err.message || `Không tìm thấy mã vạch "${barcode}"`, 'error');
    } finally {
      setScanningBarcode(false);
    }
  };

  // ─── Due date preset buttons ───
  const setDueDateOffset = (days: number) => {
    const d = new Date();
    d.setDate(d.getDate() + days);
    setDueDate(d.toISOString().split('T')[0]);
  };

  // ─── Filtered books list ───
  const filteredBooks = useMemo(() => {
    return books.filter((book) => {
      const q = searchTerm.toLowerCase();
      const matchSearch =
        !searchTerm ||
        book.title.toLowerCase().includes(q) ||
        book.author.toLowerCase().includes(q) ||
        (book.isbn && book.isbn.toLowerCase().includes(q));

      const matchCategory =
        selectedCategory === 'all' || String(book.categoryId) === selectedCategory;

      const matchAvailable = !onlyAvailable || (book.availableCopies ?? 1) > 0;

      return matchSearch && matchCategory && matchAvailable;
    });
  }, [books, searchTerm, selectedCategory, onlyAvailable]);

  // ─── Filtered users for autocomplete dropdown ───
  const filteredUsers = useMemo(() => {
    if (!userSearchTerm.trim()) {
      return users.slice(0, 8);
    }
    const q = userSearchTerm.toLowerCase();
    return users
      .filter((u) => {
        return (
          u.fullName.toLowerCase().includes(q) ||
          u.phoneNumber.toLowerCase().includes(q) ||
          (u.email && u.email.toLowerCase().includes(q))
        );
      })
      .slice(0, 10);
  }, [users, userSearchTerm]);

  // ─── Reset the entire ticket builder ───
  const handleResetTicket = () => {
    setSelectedUser(null);
    setUserSearchTerm('');
    setSelectedCopies([]);
    setNote('');
    const d = new Date();
    d.setDate(d.getDate() + 14);
    setDueDate(d.toISOString().split('T')[0]);
  };

  // ─── Submit create borrow ticket ───
  const handleConfirmCreate = async () => {
    if (!selectedUser) {
      showToast('Vui lòng chọn độc giả cho phiếu mượn!', 'error');
      return;
    }
    if (selectedCopies.length === 0) {
      showToast('Vui lòng chọn ít nhất một cuốn sách vào phiếu mượn!', 'error');
      return;
    }
    if (!dueDate) {
      showToast('Vui lòng chọn ngày hẹn trả sách!', 'error');
      return;
    }

    const todayStr = new Date().toISOString().split('T')[0];
    if (dueDate < todayStr) {
      showToast('Ngày hẹn trả không thể là một ngày trong quá khứ!', 'error');
      return;
    }

    setSubmitting(true);
    try {
      const formattedDueDate = new Date(dueDate).toISOString();
      const res = await borrowRecordsApi.create({
        userId: selectedUser.id,
        dueDate: formattedDueDate,
        bookCopyIds: selectedCopies.map((item) => item.copyId),
        note: note.trim() || undefined,
      });

      showToast('Lập phiếu mượn thành công!', 'success');
      setSuccessResultModal({
        recordId: res.id,
        userName: selectedUser.fullName,
        bookCount: selectedCopies.length,
        dueDate: new Date(dueDate).toLocaleDateString('vi-VN'),
      });

      // Reload fresh data in background
      loadInitialData();
      setCopiesCache({});
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi lập phiếu mượn', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="tab-container">
        <div className="loading-banner">
          <RefreshCw size={20} className="spin" />
          <span>Đang tải danh mục sách và dữ liệu thủ thư...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="tab-container" style={{ maxWidth: '1600px', margin: '0 auto' }}>
      {/* Page Header */}
      <div className="section-header" style={{ marginBottom: '20px' }}>
        <div>
          <h2>✍️ Lập Phiếu Mượn Sách Tại Quầy</h2>
          <p className="section-desc">
            Dành riêng cho Thủ Thư: Tìm kiếm & quét mã vạch bản sao sách, chọn độc giả và xuất phiếu mượn nhanh chóng.
          </p>
        </div>
        <div style={{ display: 'flex', gap: '10px' }}>
          <button
            className="btn btn-secondary"
            onClick={() => navigate('/staff/borrows')}
          >
            Xem Danh Sách Phiếu
          </button>
        </div>
      </div>

      {/* Main Split Layout */}
      <div className="split-borrow-layout">
        {/* ══════════════════════════════════════════════════════════════════
            CỘT TRÁI: TÌM KIẾM, QUÉT MÃ VẠCH & CHỌN BẢN SAO SÁCH
            ══════════════════════════════════════════════════════════════════ */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          {/* Quick Barcode Scanner Bar */}
          <form className="barcode-scanner-bar" onSubmit={handleBarcodeQuickAdd}>
            <Barcode size={24} color="#0284c7" style={{ flexShrink: 0 }} />
            <input
              type="text"
              placeholder="Quét mã vạch hoặc nhập Barcode cuốn sách rồi ấn Enter..."
              value={quickBarcodeInput}
              onChange={(e) => setQuickBarcodeInput(e.target.value)}
              style={{
                border: 'none',
                outline: 'none',
                fontSize: '14px',
                width: '100%',
                fontWeight: 500,
              }}
              disabled={scanningBarcode}
            />
            <button
              type="submit"
              className="btn btn-primary"
              style={{ padding: '6px 14px', fontSize: '13px', whiteSpace: 'nowrap' }}
              disabled={scanningBarcode || !quickBarcodeInput.trim()}
            >
              {scanningBarcode ? (
                <>
                  <RefreshCw size={14} className="spin" /> Đang tìm...
                </>
              ) : (
                <>
                  <Plus size={14} /> Thêm nhanh
                </>
              )}
            </button>
          </form>

          {/* Catalog Filter Controls */}
          <div className="filter-bar" style={{ marginBottom: 0 }}>
            <div className="search-input-wrapper">
              <Search size={18} className="search-icon" />
              <input
                type="text"
                placeholder="Tìm tên sách, tác giả, ISBN..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>

            <select
              className="filter-select"
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
            >
              <option value="all">Tất cả thể loại ({categories.length})</option>
              {categories.map((c) => (
                <option key={c.id} value={String(c.id)}>
                  {c.name}
                </option>
              ))}
            </select>

            <label
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '8px',
                fontSize: '13px',
                fontWeight: 500,
                color: '#475569',
                cursor: 'pointer',
                whiteSpace: 'nowrap',
                userSelect: 'none',
              }}
            >
              <input
                type="checkbox"
                checked={onlyAvailable}
                onChange={(e) => setOnlyAvailable(e.target.checked)}
                style={{ width: '16px', height: '16px', accentColor: '#0284c7' }}
              />
              Chỉ sách còn trên kệ
            </label>
          </div>

          {/* Book Catalog Grid/List */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
            {filteredBooks.length === 0 ? (
              <div className="empty-state" style={{ background: '#fff', borderRadius: '12px', border: '1px solid #e2e8f0', padding: '40px 20px' }}>
                <BookOpen size={48} color="#94a3b8" />
                <p>Không tìm thấy đầu sách nào phù hợp với bộ lọc.</p>
              </div>
            ) : (
              filteredBooks.map((book) => {
                const bIdStr = String(book.id);
                const isExpanded = Boolean(expandedBookIds[bIdStr]);
                const isLoadingCopies = Boolean(loadingCopiesFor[bIdStr]);
                const copies = copiesCache[bIdStr] || [];
                const availableCount = book.availableCopies ?? 0;

                // Count how many copies of this book are already added to ticket
                const selectedFromThisBook = selectedCopies.filter(
                  (item) => String(item.bookId) === bIdStr
                ).length;

                return (
                  <div
                    key={book.id}
                    className={`book-card-picker ${selectedFromThisBook > 0 ? 'active' : ''}`}
                  >
                    <div style={{ display: 'flex', gap: '14px', alignItems: 'flex-start' }}>
                      {/* Book Thumbnail */}
                      <div
                        style={{
                          width: '56px',
                          height: '76px',
                          borderRadius: '6px',
                          backgroundColor: '#f1f5f9',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          flexShrink: 0,
                          overflow: 'hidden',
                          border: '1px solid #e2e8f0',
                        }}
                      >
                        {book.imageUrl ? (
                          <img
                            src={book.imageUrl}
                            alt={book.title}
                            style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                            onError={(e) => {
                              (e.target as HTMLElement).style.display = 'none';
                            }}
                          />
                        ) : (
                          <BookOpen size={24} color="#94a3b8" />
                        )}
                      </div>

                      {/* Book Meta */}
                      <div style={{ flex: 1, minWidth: 0 }}>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', flexWrap: 'wrap' }}>
                          <h4 style={{ margin: 0, fontSize: '15px', fontWeight: 600, color: '#0f172a' }}>
                            {book.title}
                          </h4>
                          {selectedFromThisBook > 0 && (
                            <span
                              className="status-badge"
                              style={{
                                backgroundColor: '#dbeafe',
                                color: '#1e40af',
                                fontSize: '11px',
                                padding: '2px 8px',
                              }}
                            >
                              ✓ Đã chọn {selectedFromThisBook} cuốn
                            </span>
                          )}
                        </div>

                        <div style={{ fontSize: '13px', color: '#64748b', marginTop: '3px' }}>
                          Tác giả: <strong style={{ color: '#334155' }}>{book.author}</strong>
                          {book.categoryName && (
                            <>
                              {' '}• Thể loại: <span style={{ color: '#0284c7' }}>{book.categoryName}</span>
                            </>
                          )}
                        </div>

                        <div style={{ display: 'flex', alignItems: 'center', gap: '16px', marginTop: '6px', fontSize: '12px' }}>
                          {book.isbn && <span style={{ color: '#64748b' }}>ISBN: {book.isbn}</span>}
                          <span
                            style={{
                              fontWeight: 600,
                              color: availableCount > 0 ? '#16a34a' : '#dc2626',
                            }}
                          >
                            {availableCount > 0
                              ? `Còn ${availableCount} bản khả dụng`
                              : 'Hết sách trên kệ'}
                          </span>
                        </div>
                      </div>

                      {/* Expand / View Copies Button */}
                      <button
                        className="btn btn-secondary"
                        style={{
                          fontSize: '12px',
                          padding: '6px 12px',
                          alignSelf: 'center',
                          whiteSpace: 'nowrap',
                        }}
                        onClick={() => toggleExpandBook(book.id)}
                      >
                        {isLoadingCopies ? (
                          <>
                            <RefreshCw size={14} className="spin" /> Đang tải...
                          </>
                        ) : isExpanded ? (
                          <>
                            Thu gọn <ChevronUp size={14} />
                          </>
                        ) : (
                          <>
                            Xem bản sao <ChevronDown size={14} />
                          </>
                        )}
                      </button>
                    </div>

                    {/* Expandable Copies List */}
                    {isExpanded && (
                      <div className="copies-selector-list">
                        <div
                          style={{
                            fontSize: '12px',
                            fontWeight: 600,
                            color: '#475569',
                            marginBottom: '4px',
                            display: 'flex',
                            justifyContent: 'space-between',
                          }}
                        >
                          <span>Danh sách bản sao vật lý trên kệ:</span>
                          <span>{copies.filter((c) => c.status === 'Available').length} khả dụng</span>
                        </div>

                        {copies.length === 0 ? (
                          <div style={{ fontSize: '13px', color: '#94a3b8', fontStyle: 'italic', padding: '6px' }}>
                            Không có bản sao nào được ghi nhận cho đầu sách này.
                          </div>
                        ) : (
                          copies.map((copy) => {
                            const copyIdStr = String(copy.id);
                            const isChosen = selectedCopies.some((item) => item.copyId === copyIdStr);
                            const isAvailable = copy.status === 'Available';

                            return (
                              <div key={copy.id} className="copy-row-item">
                                <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                                  <Barcode size={16} color="#64748b" />
                                  <span style={{ fontFamily: 'monospace', fontWeight: 600, fontSize: '13px' }}>
                                    {copy.barcode}
                                  </span>
                                  {copy.conditionNote && (
                                    <span style={{ fontSize: '12px', color: '#64748b', fontStyle: 'italic' }}>
                                      ({copy.conditionNote})
                                    </span>
                                  )}
                                  <span
                                    className="status-badge"
                                    style={{
                                      fontSize: '11px',
                                      padding: '2px 6px',
                                      backgroundColor:
                                        copy.status === 'Available'
                                          ? '#dcfce7'
                                          : copy.status === 'Borrowed'
                                          ? '#ffedd5'
                                          : '#fee2e2',
                                      color:
                                        copy.status === 'Available'
                                          ? '#15803d'
                                          : copy.status === 'Borrowed'
                                          ? '#c2410c'
                                          : '#b91c1c',
                                    }}
                                  >
                                    {copy.status === 'Available'
                                      ? 'Trên kệ'
                                      : copy.status === 'Borrowed'
                                      ? 'Đang mượn'
                                      : 'Báo mất'}
                                  </span>
                                </div>

                                <div>
                                  {isChosen ? (
                                    <button
                                      className="btn btn-danger"
                                      style={{ padding: '4px 8px', fontSize: '12px' }}
                                      onClick={() => handleRemoveCopy(copyIdStr)}
                                    >
                                      <Trash2 size={13} /> Bỏ chọn
                                    </button>
                                  ) : isAvailable ? (
                                    <button
                                      className="btn btn-primary"
                                      style={{ padding: '4px 10px', fontSize: '12px' }}
                                      onClick={() => handleAddCopy(copy, book)}
                                    >
                                      <Plus size={13} /> Chọn mượn
                                    </button>
                                  ) : (
                                    <button
                                      className="btn btn-secondary"
                                      disabled
                                      style={{ padding: '4px 8px', fontSize: '12px', opacity: 0.6 }}
                                    >
                                      Không khả dụng
                                    </button>
                                  )}
                                </div>
                              </div>
                            );
                          })
                        )}
                      </div>
                    )}
                  </div>
                );
              })
            )}
          </div>
        </div>

        {/* ══════════════════════════════════════════════════════════════════
            CỘT PHẢI: TICKET BUILDER (LẬP PHIẾU MƯỢN)
            ══════════════════════════════════════════════════════════════════ */}
        <div>
          <div className="ticket-builder-card">
            {/* Header */}
            <div className="ticket-header">
              <div>
                <h3 style={{ margin: 0, fontSize: '16px', fontWeight: 600 }}>
                  📋 Thông Tin Phiếu Mượn
                </h3>
                <span style={{ fontSize: '12px', opacity: 0.85 }}>
                  Khởi tạo bởi Quầy Thủ Thư
                </span>
              </div>
              <span
                style={{
                  background: 'rgba(255, 255, 255, 0.2)',
                  padding: '4px 10px',
                  borderRadius: '12px',
                  fontSize: '12px',
                  fontWeight: 600,
                }}
              >
                {selectedCopies.length} cuốn sách
              </span>
            </div>

            {/* Body */}
            <div className="ticket-body">
              {/* 1. Chọn Độc Giả */}
              <div>
                <div className="ticket-section-title">
                  <span>1. ĐỘC GIẢ (NGƯỜI MƯỢN) *</span>
                  {selectedUser && (
                    <button
                      type="button"
                      style={{
                        background: 'none',
                        border: 'none',
                        color: '#0284c7',
                        fontSize: '12px',
                        cursor: 'pointer',
                        padding: 0,
                      }}
                      onClick={() => {
                        setSelectedUser(null);
                        setIsUserDropdownOpen(true);
                      }}
                    >
                      Đổi độc giả khác
                    </button>
                  )}
                </div>

                {!selectedUser ? (
                  <div style={{ position: 'relative' }}>
                    <div className="search-input-wrapper" style={{ width: '100%' }}>
                      <UserIcon size={18} className="search-icon" />
                      <input
                        type="text"
                        placeholder="Gõ tên hoặc số điện thoại độc giả..."
                        value={userSearchTerm}
                        onChange={(e) => {
                          setUserSearchTerm(e.target.value);
                          setIsUserDropdownOpen(true);
                        }}
                        onFocus={() => setIsUserDropdownOpen(true)}
                        style={{ width: '100%' }}
                      />
                    </div>

                    {/* Autocomplete Dropdown List */}
                    {isUserDropdownOpen && (
                      <div
                        style={{
                          position: 'absolute',
                          top: '100%',
                          left: 0,
                          right: 0,
                          backgroundColor: '#fff',
                          border: '1px solid #cbd5e1',
                          borderRadius: '8px',
                          boxShadow: '0 10px 15px -3px rgba(0,0,0,0.1)',
                          zIndex: 50,
                          maxHeight: '220px',
                          overflowY: 'auto',
                          marginTop: '4px',
                        }}
                      >
                        {filteredUsers.length === 0 ? (
                          <div style={{ padding: '12px', fontSize: '13px', color: '#94a3b8', textAlign: 'center' }}>
                            Không tìm thấy độc giả khớp với từ khóa.
                          </div>
                        ) : (
                          filteredUsers.map((u) => (
                            <div
                              key={u.id}
                              style={{
                                padding: '10px 14px',
                                borderBottom: '1px solid #f1f5f9',
                                cursor: 'pointer',
                                transition: 'background-color 0.15s',
                              }}
                              onMouseEnter={(e) => ((e.currentTarget as HTMLElement).style.backgroundColor = '#f8fafc')}
                              onMouseLeave={(e) => ((e.currentTarget as HTMLElement).style.backgroundColor = '#fff')}
                              onClick={() => {
                                setSelectedUser(u);
                                setIsUserDropdownOpen(false);
                                setUserSearchTerm('');
                              }}
                            >
                              <div style={{ fontWeight: 600, fontSize: '14px', color: '#0f172a' }}>
                                {u.fullName}
                              </div>
                              <div style={{ fontSize: '12px', color: '#64748b', display: 'flex', gap: '12px', marginTop: '2px' }}>
                                <span>📞 {u.phoneNumber}</span>
                                {u.email && <span>✉️ {u.email}</span>}
                              </div>
                            </div>
                          ))
                        )}
                      </div>
                    )}
                  </div>
                ) : (
                  <div className="ticket-user-selected">
                    <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                      <div
                        style={{
                          width: '40px',
                          height: '40px',
                          borderRadius: '50%',
                          backgroundColor: '#e0f2fe',
                          color: '#0369a1',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          fontWeight: 700,
                        }}
                      >
                        {selectedUser.fullName.charAt(0).toUpperCase()}
                      </div>
                      <div>
                        <div style={{ fontWeight: 600, fontSize: '14px', color: '#0f172a' }}>
                          {selectedUser.fullName}
                        </div>
                        <div style={{ fontSize: '12px', color: '#64748b' }}>
                          SĐT: <strong>{selectedUser.phoneNumber}</strong>
                          {selectedUser.address && ` • ${selectedUser.address}`}
                        </div>
                      </div>
                    </div>
                  </div>
                )}
              </div>

              {/* 2. Ngày Hẹn Trả Sách */}
              <div>
                <div className="ticket-section-title">
                  <span>2. THỜI HẠN TRẢ SÁCH *</span>
                </div>
                <div style={{ display: 'flex', gap: '8px', marginBottom: '8px' }}>
                  <button
                    type="button"
                    className="btn btn-secondary"
                    style={{ flex: 1, padding: '6px', fontSize: '12px' }}
                    onClick={() => setDueDateOffset(7)}
                  >
                    +7 ngày
                  </button>
                  <button
                    type="button"
                    className="btn btn-secondary"
                    style={{ flex: 1, padding: '6px', fontSize: '12px' }}
                    onClick={() => setDueDateOffset(14)}
                  >
                    +14 ngày (chuẩn)
                  </button>
                  <button
                    type="button"
                    className="btn btn-secondary"
                    style={{ flex: 1, padding: '6px', fontSize: '12px' }}
                    onClick={() => setDueDateOffset(30)}
                  >
                    +30 ngày
                  </button>
                </div>
                <div style={{ position: 'relative' }}>
                  <input
                    type="date"
                    className="form-input"
                    value={dueDate}
                    onChange={(e) => setDueDate(e.target.value)}
                    style={{ width: '100%', fontSize: '13px' }}
                  />
                </div>
                <div style={{ fontSize: '12px', color: '#64748b', marginTop: '4px', display: 'flex', alignItems: 'center', gap: '4px' }}>
                  <Clock size={13} />
                  <span>
                    Hẹn trả vào ngày:{' '}
                    <strong>
                      {dueDate ? new Date(dueDate).toLocaleDateString('vi-VN') : '---'}
                    </strong>
                  </span>
                </div>
              </div>

              {/* 3. Danh Sách Sách Trong Phiếu */}
              <div>
                <div className="ticket-section-title">
                  <span>3. SÁCH ĐÃ CHỌN ({selectedCopies.length}) *</span>
                  {selectedCopies.length > 0 && (
                    <button
                      type="button"
                      style={{
                        background: 'none',
                        border: 'none',
                        color: '#ef4444',
                        fontSize: '12px',
                        cursor: 'pointer',
                        padding: 0,
                      }}
                      onClick={() => setSelectedCopies([])}
                    >
                      Xóa tất cả
                    </button>
                  )}
                </div>

                {selectedCopies.length === 0 ? (
                  <div
                    style={{
                      border: '2px dashed #cbd5e1',
                      borderRadius: '10px',
                      padding: '24px 16px',
                      textAlign: 'center',
                      color: '#94a3b8',
                      backgroundColor: '#f8fafc',
                    }}
                  >
                    <BookOpen size={32} style={{ marginBottom: '8px', opacity: 0.6 }} />
                    <div style={{ fontSize: '13px', fontWeight: 500 }}>Chưa có cuốn sách nào</div>
                    <div style={{ fontSize: '12px', marginTop: '4px' }}>
                      Chọn từ danh mục bên trái hoặc quét mã vạch ở trên
                    </div>
                  </div>
                ) : (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', maxHeight: '240px', overflowY: 'auto' }}>
                    {selectedCopies.map((item, idx) => (
                      <div key={item.copyId} className="ticket-book-item">
                        <div style={{ minWidth: 0, flex: 1 }}>
                          <div
                            style={{
                              fontWeight: 600,
                              fontSize: '13px',
                              color: '#0f172a',
                              whiteSpace: 'nowrap',
                              overflow: 'hidden',
                              textOverflow: 'ellipsis',
                            }}
                          >
                            {idx + 1}. {item.bookTitle}
                          </div>
                          <div style={{ fontSize: '11px', color: '#64748b', display: 'flex', gap: '8px', marginTop: '2px' }}>
                            <span>Mã vạch: <strong style={{ fontFamily: 'monospace' }}>{item.barcode}</strong></span>
                            {item.bookAuthor && <span>• {item.bookAuthor}</span>}
                          </div>
                        </div>
                        <button
                          type="button"
                          className="btn-icon"
                          style={{ color: '#ef4444', padding: '4px' }}
                          title="Xóa cuốn này"
                          onClick={() => handleRemoveCopy(item.copyId)}
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              {/* 4. Ghi Chú */}
              <div>
                <div className="ticket-section-title">
                  <span>4. GHI CHÚ PHIẾU MƯỢN</span>
                </div>
                <textarea
                  className="form-input"
                  rows={2}
                  placeholder="Ghi chú thêm (thẻ sinh viên, tình trạng sách lúc bàn giao...)"
                  value={note}
                  onChange={(e) => setNote(e.target.value)}
                  style={{ width: '100%', fontSize: '13px', resize: 'vertical' }}
                />
              </div>

              {/* 5. Nút Thao Tác Xác Nhận */}
              <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', marginTop: '8px' }}>
                <button
                  type="button"
                  className="btn btn-primary"
                  style={{
                    width: '100%',
                    padding: '12px',
                    fontSize: '14px',
                    fontWeight: 600,
                    display: 'flex',
                    justifyContent: 'center',
                    alignItems: 'center',
                    gap: '8px',
                  }}
                  disabled={submitting || !selectedUser || selectedCopies.length === 0}
                  onClick={handleConfirmCreate}
                >
                  {submitting ? (
                    <>
                      <RefreshCw size={18} className="spin" /> Đang tạo phiếu...
                    </>
                  ) : (
                    <>
                      <CheckCircle2 size={18} /> Xác Nhận Lập Phiếu Mượn
                    </>
                  )}
                </button>

                <button
                  type="button"
                  className="btn btn-secondary"
                  style={{ width: '100%', padding: '8px', fontSize: '13px' }}
                  onClick={handleResetTicket}
                  disabled={submitting}
                >
                  <RotateCcw size={14} /> Làm Mới / Đặt Lại Phiếu
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* ─── Modal Thông Báo Lập Phiếu Thành Công ─── */}
      {successResultModal && (
        <div className="modal-backdrop">
          <div className="modal-card" style={{ maxWidth: '480px', textAlign: 'center', padding: '28px' }}>
            <div
              style={{
                width: '64px',
                height: '64px',
                borderRadius: '50%',
                backgroundColor: '#dcfce7',
                color: '#16a34a',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                margin: '0 auto 16px auto',
              }}
            >
              <CheckCircle2 size={36} />
            </div>

            <h3 style={{ margin: '0 0 8px 0', fontSize: '20px', color: '#0f172a' }}>
              Lập Phiếu Mượn Thành Công!
            </h3>
            <p style={{ color: '#64748b', fontSize: '14px', margin: '0 0 20px 0' }}>
              Phiếu mượn <strong>#{successResultModal.recordId}</strong> đã được ghi nhận vào hệ thống thư viện.
            </p>

            <div
              style={{
                backgroundColor: '#f8fafc',
                border: '1px solid #e2e8f0',
                borderRadius: '10px',
                padding: '16px',
                textAlign: 'left',
                marginBottom: '24px',
                display: 'flex',
                flexDirection: 'column',
                gap: '8px',
                fontSize: '13px',
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: '#64748b' }}>Độc giả:</span>
                <strong style={{ color: '#0f172a' }}>{successResultModal.userName}</strong>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: '#64748b' }}>Số lượng sách:</span>
                <strong>{successResultModal.bookCount} cuốn</strong>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: '#64748b' }}>Hạn trả:</span>
                <strong style={{ color: '#0284c7' }}>{successResultModal.dueDate}</strong>
              </div>
            </div>

            <div style={{ display: 'flex', gap: '12px' }}>
              <button
                className="btn btn-secondary"
                style={{ flex: 1 }}
                onClick={() => {
                  setSuccessResultModal(null);
                  handleResetTicket();
                }}
              >
                Lập Phiếu Khác
              </button>
              <button
                className="btn btn-primary"
                style={{ flex: 1 }}
                onClick={() => {
                  setSuccessResultModal(null);
                  navigate('/staff/borrows');
                }}
              >
                Xem Danh Sách Phiếu
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
