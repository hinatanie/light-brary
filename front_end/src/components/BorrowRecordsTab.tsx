import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { BorrowRecord, User, BookCopy } from '../types';
import { borrowRecordsApi } from '../api';
import {
  Plus,
  Search,
  CheckSquare,
  Square,
  RotateCcw,
  Calendar,
  X,
  FileText,
  AlertCircle,
  AlertTriangle
} from 'lucide-react';

interface BorrowRecordsTabProps {
  borrowRecords: BorrowRecord[];
  users: User[];
  availableCopies: BookCopy[];
  onRefresh: () => void;
  onNotify: (text: string, type?: 'success' | 'error') => void;
}

export const BorrowRecordsTab: React.FC<BorrowRecordsTabProps> = ({
  borrowRecords,
  users,
  availableCopies,
  onRefresh,
  onNotify,
}) => {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isReturnModalOpen, setIsReturnModalOpen] = useState(false);
  const [selectedRecord, setSelectedRecord] = useState<BorrowRecord | null>(null);

  // Mark lost modal states
  const [isLostModalOpen, setIsLostModalOpen] = useState(false);
  const [lostTargetDetail, setLostTargetDetail] = useState<{ detailId: string; title: string; barcode: string } | null>(null);
  const [lostNote, setLostNote] = useState<string>('');
  const [submittingLost, setSubmittingLost] = useState(false);

  // Create form states
  const [userId, setUserId] = useState<string>(String(users[0]?.id || ''));
  const [dueDate, setDueDate] = useState<string>(() => {
    const d = new Date();
    d.setDate(d.getDate() + 14); // Mặc định hẹn trả sau 14 ngày
    return d.toISOString().split('T')[0];
  });
  const [selectedCopyIds, setSelectedCopyIds] = useState<string[]>([]);
  const [note, setNote] = useState('');
  const [submitting, setSubmitting] = useState(false);

  // Return form states: map detailId -> boolean
  const [returnItemsState, setReturnItemsState] = useState<Record<string, boolean>>({});

  const openCreateModal = () => {
    setUserId(String(users[0]?.id || ''));
    const d = new Date();
    d.setDate(d.getDate() + 14);
    setDueDate(d.toISOString().split('T')[0]);
    setSelectedCopyIds([]);
    setNote('');
    setIsCreateModalOpen(true);
  };

  const toggleCopySelection = (copyId: string) => {
    setSelectedCopyIds((prev) =>
      prev.includes(copyId) ? prev.filter((id) => id !== copyId) : [...prev, copyId]
    );
  };

  const handleCreateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedCopyIds.length === 0) {
      onNotify('Vui lòng chọn ít nhất một cuốn sách để mượn!', 'error');
      return;
    }
    setSubmitting(true);
    try {
      const formattedDueDate = new Date(dueDate).toISOString();
      await borrowRecordsApi.create({
        userId,
        dueDate: formattedDueDate,
        bookCopyIds: selectedCopyIds,
        note: note.trim() || undefined,
      });
      onNotify('Tạo phiếu mượn sách thành công!');
      setIsCreateModalOpen(false);
      onRefresh();
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi tạo phiếu mượn', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  const openReturnModal = async (record: BorrowRecord) => {
    let target = record;
    if (!target.details || target.details.length === 0) {
      try {
        target = await borrowRecordsApi.getById(record.id);
      } catch {
        // use record as fallback
      }
    }
    setSelectedRecord(target);
    const initial: Record<string, boolean> = {};
    (target.details || []).forEach((d) => {
      // Chỉ chọn mặc định những cuốn đang mượn (chưa trả và chưa báo mất)
      if (d.status === 'Borrowing') {
        initial[d.id] = true;
      }
    });
    setReturnItemsState(initial);
    setIsReturnModalOpen(true);
  };

  const handleMarkLost = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedRecord || !lostTargetDetail) return;
    setSubmittingLost(true);
    try {
      await borrowRecordsApi.markLost(selectedRecord.id, lostTargetDetail.detailId, {
        note: lostNote.trim() || undefined,
      });
      onNotify(`Đã ghi nhận báo mất cuốn sách "${lostTargetDetail.title}"!`);
      setIsLostModalOpen(false);
      setIsReturnModalOpen(false);
      onRefresh();
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi báo mất sách', 'error');
    } finally {
      setSubmittingLost(false);
    }
  };

  const handleReturnSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedRecord) return;

    const itemsToReturn = Object.entries(returnItemsState)
      .filter(([_, isSelected]) => isSelected)
      .map(([detailId]) => ({
        borrowDetailId: detailId,
      }));

    if (itemsToReturn.length === 0) {
      onNotify('Vui lòng chọn ít nhất một cuốn sách để trả!', 'error');
      return;
    }

    setSubmitting(true);
    try {
      await borrowRecordsApi.returnBooks(selectedRecord.id, { items: itemsToReturn });
      onNotify(`Đã xử lý trả ${itemsToReturn.length} cuốn sách thành công!`);
      setIsReturnModalOpen(false);
      onRefresh();
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi xử lý trả sách', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  const filteredRecords = borrowRecords.filter((br) => {
    const name = br.userFullName || br.user?.fullName || '';
    const details = br.details || [];
    const matchesSearch =
      name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      String(br.id).includes(searchTerm) ||
      details.some(
        (d) =>
          d.bookTitle?.toLowerCase().includes(searchTerm.toLowerCase()) ||
          d.barcode?.toLowerCase().includes(searchTerm.toLowerCase())
      );
    const matchesStatus = statusFilter === 'all' || br.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  return (
    <div className="tab-container">
      <div className="section-header">
        <div>
          <h2>📋 Quản Lý Mượn & Trả Sách</h2>
          <p className="section-desc">
            Lập phiếu mượn nhiều cuốn cùng lúc, theo dõi hạn trả và xử lý hoàn trả sách
          </p>
        </div>
        <div style={{ display: 'flex', gap: '10px' }}>
          <button className="btn btn-primary" onClick={() => navigate('/staff/create-borrow')}>
            <Plus size={18} /> Lập Phiếu Mượn Tại Quầy
          </button>
          <button className="btn btn-secondary" onClick={openCreateModal} title="Mở biểu mẫu nhanh dạng hộp thoại">
            Lập Nhanh
          </button>
        </div>
      </div>

      <div className="filter-bar">
        <div className="search-input-wrapper">
          <Search size={18} className="search-icon" />
          <input
            type="text"
            placeholder="Tìm theo mã phiếu, tên độc giả, tên sách, barcode..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>

        <select
          className="filter-select"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
        >
          <option value="all">Tất cả trạng thái phiếu</option>
          <option value="Borrowing">Đang mượn (Borrowing)</option>
          <option value="Returned">Đã trả đủ (Returned)</option>
          <option value="Overdue">Quá hạn (Overdue)</option>
        </select>
      </div>

      <div className="table-card">
        {filteredRecords.length === 0 ? (
          <div className="empty-state">
            <FileText size={48} color="#94a3b8" />
            <p>Không có phiếu mượn nào phù hợp.</p>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="custom-table">
              <thead>
                <tr>
                  <th>Mã phiếu</th>
                  <th>Độc giả</th>
                  <th>Ngày mượn</th>
                  <th>Hạn trả</th>
                  <th>Chi tiết sách</th>
                  <th>Trạng thái</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {filteredRecords.map((br) => {
                  const finishedCount = br.details.filter((d) => d.status === 'Returned' || d.status === 'Lost').length;
                  return (
                    <tr key={br.id}>
                      <td><strong>#{br.id}</strong></td>
                      <td>
                        <div style={{ fontWeight: 600 }}>{br.userFullName}</div>
                        <div style={{ fontSize: '12px', color: '#64748b' }}>Mã: {String(br.userId).slice(0, 8)}...</div>
                      </td>
                      <td>{new Date(br.borrowDate).toLocaleDateString('vi-VN')}</td>
                      <td>
                        <span style={{ fontWeight: 500, color: (br.status === 'Overdue' || br.details.some(d => d.isOverdue)) ? '#ef4444' : 'inherit' }}>
                          {new Date(br.dueDate).toLocaleDateString('vi-VN')}
                          {(br.status === 'Overdue' || br.details.some(d => d.isOverdue)) && ' (Quá hạn)'}
                        </span>
                      </td>
                      <td>
                        <div className="borrow-details-summary">
                          {br.details.map((d) => (
                            <span
                              key={d.id}
                              className={`pill-item ${d.status === 'Returned' ? 'pill-returned' : ''}`}
                              style={d.status === 'Lost' ? { background: 'rgba(239, 68, 68, 0.2)', color: '#ef4444', border: '1px solid rgba(239, 68, 68, 0.4)' } : undefined}
                            >
                              {d.bookTitle} (<code>{d.barcode}</code>)
                              {d.status === 'Returned' && ' ✓'}
                              {d.status === 'Lost' && ' ✕ (Mất)'}
                            </span>
                          ))}
                        </div>
                        <div style={{ fontSize: '12px', color: '#64748b', marginTop: '4px' }}>
                          Tiến độ: {finishedCount}/{br.details.length} cuốn đã hoàn tất
                        </div>
                      </td>
                      <td>
                        <span className={`badge badge-${br.status.toLowerCase()}`}>
                          {br.status === 'Borrowing'
                            ? 'Đang mượn'
                            : br.status === 'Returned'
                            ? 'Đã trả đủ'
                            : 'Quá hạn'}
                        </span>
                      </td>
                      <td>
                        <button
                          className="btn-action btn-return"
                          onClick={() => openReturnModal(br)}
                        >
                          <RotateCcw size={16} /> Chi tiết / Trả
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modal Lập Phiếu Mượn */}
      {isCreateModalOpen && (
        <div className="modal-backdrop" onClick={() => setIsCreateModalOpen(false)}>
          <div className="modal-box modal-lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>📝 Lập Phiếu Mượn Sách Mới</h3>
              <button className="btn-icon" onClick={() => setIsCreateModalOpen(false)}>
                <X size={18} />
              </button>
            </div>
            <form onSubmit={handleCreateSubmit} className="form-content">
              <div className="form-row">
                <div className="form-group">
                  <label>Chọn độc giả mượn *</label>
                  <select
                    required
                    value={userId}
                    onChange={(e) => setUserId(e.target.value)}
                  >
                    {users.map((u) => (
                      <option key={u.id} value={u.id}>
                        {u.fullName} ({u.email})
                      </option>
                    ))}
                  </select>
                </div>

                <div className="form-group">
                  <label><Calendar size={14} /> Hạn trả sách *</label>
                  <input
                    type="date"
                    required
                    value={dueDate}
                    min={new Date().toISOString().split('T')[0]}
                    onChange={(e) => setDueDate(e.target.value)}
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Ghi chú phiếu mượn</label>
                <input
                  type="text"
                  placeholder="Ví dụ: Độc giả mượn phục vụ nghiên cứu luận văn"
                  value={note}
                  onChange={(e) => setNote(e.target.value)}
                />
              </div>

              <div className="form-group">
                <label style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span>Chọn các cuốn sách mượn (Chỉ hiển thị sách đang sẵn sàng trên kệ) *</span>
                  <span style={{ color: '#0284c7', fontWeight: 600 }}>
                    Đã chọn: {selectedCopyIds.length} cuốn
                  </span>
                </label>

                {availableCopies.length === 0 ? (
                  <div className="empty-inline-box">
                    <AlertCircle size={18} color="#f59e0b" />
                    <span>Hiện không còn cuốn sách nào ở trạng thái "Sẵn sàng" (Available) trên kệ!</span>
                  </div>
                ) : (
                  <div className="copy-selection-list">
                    {availableCopies.map((c) => {
                      const isSelected = selectedCopyIds.includes(String(c.id));
                      return (
                        <div
                          key={c.id}
                          className={`copy-select-item ${isSelected ? 'selected' : ''}`}
                          onClick={() => toggleCopySelection(String(c.id))}
                        >
                          <div className="checkbox-icon">
                            {isSelected ? <CheckSquare size={18} color="#0284c7" /> : <Square size={18} color="#94a3b8" />}
                          </div>
                          <div className="copy-info">
                            <div className="copy-title">{c.bookTitle}</div>
                            <div className="copy-meta">
                              Tác giả: {c.bookAuthor} | Mã vạch: <code>{c.barcode}</code>
                            </div>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setIsCreateModalOpen(false)}
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={submitting || selectedCopyIds.length === 0}
                >
                  {submitting ? 'Đang tạo phiếu...' : `Xác Nhận Mượn (${selectedCopyIds.length} cuốn)`}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Xử Lý Trả Sách */}
      {isReturnModalOpen && selectedRecord && (
        <div className="modal-backdrop" onClick={() => setIsReturnModalOpen(false)}>
          <div className="modal-box modal-lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>🔄 Xử Lý Hoàn Trả Sách (Phiếu #{selectedRecord.id})</h3>
              <button className="btn-icon" onClick={() => setIsReturnModalOpen(false)}>
                <X size={18} />
              </button>
            </div>

            <div className="record-summary-banner">
              <div><strong>Độc giả:</strong> {selectedRecord.userFullName}</div>
              <div><strong>Ngày mượn:</strong> {new Date(selectedRecord.borrowDate).toLocaleDateString('vi-VN')}</div>
              <div><strong>Hạn trả:</strong> {new Date(selectedRecord.dueDate).toLocaleDateString('vi-VN')}</div>
            </div>

            <form onSubmit={handleReturnSubmit} className="form-content">
              <label style={{ margin: '12px 0 8px 0', display: 'block', fontWeight: 600 }}>
                Chọn các cuốn sách độc giả hoàn trả:
              </label>

              <div className="return-items-list">
                {selectedRecord.details.map((d) => {
                  const isSelected = Boolean(returnItemsState[d.id]);
                  const isAlreadyReturned = d.status === 'Returned';
                  const isAlreadyLost = d.status === 'Lost';
                  const isDone = isAlreadyReturned || isAlreadyLost;

                  return (
                    <div
                      key={d.id}
                      className={`return-item-row ${isDone ? 'already-returned' : ''}`}
                    >
                      <div className="return-check">
                        <input
                          type="checkbox"
                          disabled={isDone}
                          checked={isSelected}
                          onChange={(e) => {
                            setReturnItemsState((prev) => ({
                              ...prev,
                              [d.id]: e.target.checked,
                            }));
                          }}
                        />
                      </div>

                      <div className="return-book-info">
                        <strong>{d.bookTitle}</strong>
                        <div style={{ fontSize: '13px', color: '#64748b' }}>
                          Mã vạch: <code>{d.barcode}</code> — {d.bookAuthor}
                        </div>
                        {isAlreadyReturned && (
                          <span className="badge badge-returned" style={{ marginTop: '4px' }}>
                            Đã trả vào ngày {new Date(d.returnDate!).toLocaleDateString('vi-VN')}
                          </span>
                        )}
                        {isAlreadyLost && (
                          <span className="badge" style={{ marginTop: '4px', background: 'rgba(239, 68, 68, 0.2)', color: '#ef4444' }}>
                            Đã báo mất
                          </span>
                        )}
                      </div>

                      {!isDone && (
                        <div>
                          <button
                            type="button"
                            className="btn-action"
                            style={{
                              color: '#ef4444',
                              borderColor: 'rgba(239, 68, 68, 0.3)',
                              padding: '8px 12px',
                              borderRadius: '6px',
                              fontSize: '12px',
                              fontWeight: 600,
                              whiteSpace: 'nowrap',
                              display: 'flex',
                              alignItems: 'center',
                              gap: '6px',
                            }}
                            title="Báo mất cuốn sách này"
                            onClick={() => {
                              setLostTargetDetail({ detailId: String(d.id), title: d.bookTitle, barcode: d.barcode });
                              setLostNote('');
                              setIsLostModalOpen(true);
                            }}
                          >
                            <AlertTriangle size={14} /> Báo mất
                          </button>
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>

              <div className="modal-actions" style={{ marginTop: '20px' }}>
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setIsReturnModalOpen(false)}
                >
                  Đóng
                </button>
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={
                    submitting ||
                    !Object.values(returnItemsState).some(Boolean)
                  }
                >
                  {submitting ? 'Đang xử lý...' : 'Xác Nhận Trả Sách Vào Kệ'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Báo Mất Sách */}
      {isLostModalOpen && lostTargetDetail && selectedRecord && (
        <div className="modal-backdrop" onClick={() => setIsLostModalOpen(false)}>
          <div className="modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px', color: '#ef4444' }}>
                <AlertTriangle size={20} />
                <h3>Báo Mất Sách</h3>
              </div>
              <button className="btn-icon" onClick={() => setIsLostModalOpen(false)}>
                <X size={18} />
              </button>
            </div>

            <form onSubmit={handleMarkLost} className="form-content">
              <p style={{ margin: '0 0 16px 0', fontSize: '13px', color: '#94a3b8' }}>
                Độc giả làm mất cuốn sách: <strong style={{ color: '#f8fafc' }}>{lostTargetDetail.title}</strong> (Mã vạch: <code>{lostTargetDetail.barcode}</code>). Cuốn sách này sẽ được đánh dấu trạng thái "Lost" và không thể mượn lại cho đến khi được thay thế.
              </p>

              <div className="form-group" style={{ marginBottom: '24px' }}>
                <label style={{ fontSize: '13px', fontWeight: 600, color: '#f8fafc', marginBottom: '6px' }}>
                  Ghi chú lý do / tình trạng
                </label>
                <input
                  type="text"
                  placeholder="Ví dụ: Độc giả làm rơi mất trong quá trình đi tàu..."
                  value={lostNote}
                  onChange={(e) => setLostNote(e.target.value)}
                  style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: '#0f172a', border: '1px solid #334155', color: '#f8fafc', fontSize: '14px' }}
                />
              </div>

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setIsLostModalOpen(false)}
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="btn btn-primary"
                  style={{ background: '#ef4444', borderColor: '#ef4444' }}
                  disabled={submittingLost}
                >
                  {submittingLost ? 'Đang ghi nhận...' : 'Xác Nhận Báo Mất'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
