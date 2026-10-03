import React, { useState } from 'react';
import type { Book, BookCopy } from '../types';
import { bookCopiesApi } from '../api';
import { Plus, Search, Edit3, Trash2, Barcode, X, RefreshCw } from 'lucide-react';

interface BookCopiesTabProps {
  copies: BookCopy[];
  books: Book[];
  selectedBookId: string | null;
  onClearSelectedBook: () => void;
  onRefresh: () => void;
  onNotify: (text: string, type?: 'success' | 'error') => void;
  readOnly?: boolean;
}

export const BookCopiesTab: React.FC<BookCopiesTabProps> = ({
  copies,
  books,
  selectedBookId,
  onClearSelectedBook,
  onRefresh,
  onNotify,
  readOnly = false,
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [isConditionModalOpen, setIsConditionModalOpen] = useState(false);
  const [targetCopy, setTargetCopy] = useState<BookCopy | null>(null);

  // Form states for Add Copy
  const [bookId, setBookId] = useState<string>(selectedBookId || books[0]?.id || '');
  const [barcode, setBarcode] = useState('');
  const [conditionNote, setConditionNote] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const generateBarcode = () => {
    const random = Math.floor(100000 + Math.random() * 900000);
    setBarcode(`BC-${random}`);
  };

  const openAddModal = () => {
    setBookId(selectedBookId || books[0]?.id || '');
    generateBarcode();
    setConditionNote('Sách mới nhập kho');
    setIsAddModalOpen(true);
  };

  const handleAddSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await bookCopiesApi.create({
        bookId: bookId,
        barcode: barcode.trim(),
        conditionNote: conditionNote.trim() || undefined,
      });
      onNotify(`Đã thêm bản sao [${barcode}] vào kệ thư viện!`);
      setIsAddModalOpen(false);
      onRefresh();
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi thêm bản sao sách', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  const openConditionModal = (copy: BookCopy) => {
    setTargetCopy(copy);
    setConditionNote(copy.conditionNote || '');
    setIsConditionModalOpen(true);
  };

  const handleUpdateConditionSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!targetCopy) return;
    setSubmitting(true);
    try {
      await bookCopiesApi.updateCondition(targetCopy.id, {
        conditionNote: conditionNote.trim() || undefined,
      });
      onNotify(`Đã cập nhật tình trạng cho bản sao [${targetCopy.barcode}]!`);
      setIsConditionModalOpen(false);
      onRefresh();
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi cập nhật tình trạng', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (copy: BookCopy) => {
    if (copy.status === 'Borrowed') {
      onNotify(`Bản sao [${copy.barcode}] đang được mượn, không thể xóa!`, 'error');
      return;
    }
    if (!window.confirm(`Bạn có chắc chắn muốn xóa bản sao [${copy.barcode}]?`)) return;

    try {
      await bookCopiesApi.delete(copy.id);
      onNotify(`Đã xóa bản sao [${copy.barcode}]`);
      onRefresh();
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi xóa bản sao', 'error');
    }
  };

  const selectedBook = books.find((b) => b.id === selectedBookId);

  const filteredCopies = copies.filter((c) => {
    const matchesBook = selectedBookId === null || c.bookId === selectedBookId;
    const matchesSearch =
      c.barcode.toLowerCase().includes(searchTerm.toLowerCase()) ||
      c.bookTitle.toLowerCase().includes(searchTerm.toLowerCase()) ||
      c.bookAuthor.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = statusFilter === 'all' || c.status === statusFilter;
    return matchesBook && matchesSearch && matchesStatus;
  });

  return (
    <div className="tab-container">
      <div className="section-header">
        <div>
          <h2>📦 {readOnly ? 'Tra Cứu Bản Sao Sách Vật Lý' : 'Quản Lý Bản Sao Sách Vật Lý (Book Copies)'}</h2>
          <p className="section-desc">
            {readOnly
              ? 'Kiểm tra tình trạng từng cuốn sách trên kệ thông qua mã vạch (Barcode)'
              : 'Theo dõi từng cuốn sách thực tế trên kệ thông qua mã vạch (Barcode) và tình trạng bảo quản'}
          </p>
        </div>
        {!readOnly && (
          <button className="btn btn-primary" onClick={openAddModal}>
            <Plus size={18} /> Thêm Bản Sao Vào Kệ
          </button>
        )}
      </div>

      {selectedBookId !== null && selectedBook && (
        <div className="filter-badge-banner">
          <span>
            Đang lọc bản sao của đầu sách: <strong>{selectedBook.title}</strong>
          </span>
          <button className="btn-link-dark" onClick={onClearSelectedBook}>
            <X size={14} /> Xóa bộ lọc (Xem tất cả)
          </button>
        </div>
      )}

      <div className="filter-bar">
        <div className="search-input-wrapper">
          <Search size={18} className="search-icon" />
          <input
            type="text"
            placeholder="Tìm theo Barcode, tên sách, tác giả..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>

        <select
          className="filter-select"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
        >
          <option value="all">Tất cả tình trạng</option>
          <option value="Available">Sẵn sàng mượn (Available)</option>
          <option value="Borrowed">Đang được mượn (Borrowed)</option>
          <option value="Lost">Thất lạc (Lost)</option>
        </select>
      </div>

      <div className="table-card">
        {filteredCopies.length === 0 ? (
          <div className="empty-state">
            <Barcode size={48} color="#94a3b8" />
            <p>Không có bản sao sách nào phù hợp.</p>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="custom-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Mã vạch (Barcode)</th>
                  <th>Tựa sách</th>
                  <th>Tác giả</th>
                  <th>Trạng thái</th>
                  <th>Ghi chú tình trạng</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {filteredCopies.map((c) => (
                  <tr key={c.id}>
                    <td>#{c.id}</td>
                    <td>
                      <code className="barcode-badge">
                        <Barcode size={14} /> {c.barcode}
                      </code>
                    </td>
                    <td>
                      <strong>{c.bookTitle}</strong>
                    </td>
                    <td>{c.bookAuthor}</td>
                    <td>
                      <span className={`badge badge-${c.status.toLowerCase()}`}>
                        {c.status === 'Available'
                          ? 'Sẵn sàng'
                          : c.status === 'Borrowed'
                          ? 'Đang mượn'
                          : 'Thất lạc'}
                      </span>
                    </td>
                    <td>
                      <span className="condition-text">{c.conditionNote || 'Bình thường'}</span>
                    </td>
                    <td>
                      {!readOnly ? (
                        <div className="action-buttons">
                          <button
                            className="btn-action"
                            title="Cập nhật tình trạng sách"
                            onClick={() => openConditionModal(c)}
                          >
                            <Edit3 size={16} /> Ghi chú
                          </button>
                          <button
                            className="btn-action btn-danger"
                            title="Xóa bản sao"
                            onClick={() => handleDelete(c)}
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      ) : (
                        <span style={{ color: '#94a3b8', fontSize: '13px' }}>—</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modal Thêm Bản Sao */}
      {isAddModalOpen && (
        <div className="modal-backdrop" onClick={() => setIsAddModalOpen(false)}>
          <div className="modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>📦 Thêm Bản Sao Sách Vào Kệ</h3>
              <button className="btn-icon" onClick={() => setIsAddModalOpen(false)}>
                <X size={18} />
              </button>
            </div>
            <form onSubmit={handleAddSubmit} className="form-content">
              <div className="form-group">
                <label>Chọn đầu sách *</label>
                <select
                  required
                  value={bookId}
                  onChange={(e) => setBookId(e.target.value)}
                >
                  {books.map((b) => (
                    <option key={b.id} value={b.id}>
                      {b.title} ({b.author})
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label>Mã vạch (Barcode) *</label>
                <div style={{ display: 'flex', gap: '8px' }}>
                  <input
                    type="text"
                    required
                    placeholder="BC-123456"
                    value={barcode}
                    onChange={(e) => setBarcode(e.target.value)}
                  />
                  <button
                    type="button"
                    className="btn btn-secondary"
                    title="Tự sinh mã vạch"
                    onClick={generateBarcode}
                  >
                    <RefreshCw size={16} />
                  </button>
                </div>
              </div>

              <div className="form-group">
                <label>Tình trạng ban đầu</label>
                <input
                  type="text"
                  placeholder="Ví dụ: Mới 100%, có dập dấu thư viện"
                  value={conditionNote}
                  onChange={(e) => setConditionNote(e.target.value)}
                />
              </div>

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setIsAddModalOpen(false)}
                >
                  Hủy
                </button>
                <button type="submit" className="btn btn-primary" disabled={submitting}>
                  {submitting ? 'Đang thêm...' : 'Lưu Bản Sao'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Cập nhật tình trạng sách */}
      {isConditionModalOpen && targetCopy && (
        <div className="modal-backdrop" onClick={() => setIsConditionModalOpen(false)}>
          <div className="modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>✏️ Cập Nhật Tình Trạng ({targetCopy.barcode})</h3>
              <button className="btn-icon" onClick={() => setIsConditionModalOpen(false)}>
                <X size={18} />
              </button>
            </div>
            <form onSubmit={handleUpdateConditionSubmit} className="form-content">
              <p style={{ margin: '0 0 12px 0', fontSize: '14px', color: '#64748b' }}>
                Đầu sách: <strong>{targetCopy.bookTitle}</strong>
              </p>

              <div className="form-group">
                <label>Ghi chú tình trạng sách hiện tại *</label>
                <textarea
                  rows={3}
                  required
                  placeholder="Ví dụ: Rách góc trang bìa, gáy sách hơi sờn..."
                  value={conditionNote}
                  onChange={(e) => setConditionNote(e.target.value)}
                />
              </div>

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setIsConditionModalOpen(false)}
                >
                  Hủy
                </button>
                <button type="submit" className="btn btn-primary" disabled={submitting}>
                  {submitting ? 'Đang lưu...' : 'Cập Nhật Ghi Chú'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
