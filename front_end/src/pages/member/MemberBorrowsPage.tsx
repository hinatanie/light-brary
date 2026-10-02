import React, { useState, useEffect } from 'react';
import type { BorrowRecord } from '../../types';
import { borrowRecordsApi } from '../../api';
import { useToast } from '../../contexts/ToastContext';
import {
  FileText,
  Search,
  X,
  BookOpen,
  RefreshCw,
  Eye
} from 'lucide-react';

export const MemberBorrowsPage: React.FC = () => {
  const { showToast } = useToast();
  const [loading, setLoading] = useState(true);
  const [borrows, setBorrows] = useState<BorrowRecord[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [selectedRecord, setSelectedRecord] = useState<BorrowRecord | null>(null);

  const loadData = async () => {
    setLoading(true);
    try {
      const records = await borrowRecordsApi.getMyBorrowRecords();
      setBorrows(records);
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi tải lịch sử phiếu mượn', 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const filteredBorrows = borrows.filter((b) => {
    const matchesSearch =
      b.id.toString().includes(searchTerm) ||
      b.details.some(
        (d) =>
          d.bookTitle.toLowerCase().includes(searchTerm.toLowerCase()) ||
          d.barcode.toLowerCase().includes(searchTerm.toLowerCase())
      );
    const matchesStatus = statusFilter === 'all' || b.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  return (
    <div className="tab-container">
      <div className="section-header">
        <div>
          <h2>📜 Lịch Sử & Danh Sách Phiếu Mượn Sách</h2>
          <p className="section-desc">
            Xem toàn bộ các phiếu mượn bạn đã tạo, hạn trả sách và chi tiết từng cuốn sách
          </p>
        </div>
        <button
          className="btn btn-secondary"
          onClick={loadData}
          disabled={loading}
        >
          <RefreshCw size={16} className={loading ? 'spin' : ''} /> Làm mới
        </button>
      </div>

      <div className="filter-bar">
        <div className="search-input-wrapper">
          <Search size={18} className="search-icon" />
          <input
            type="text"
            placeholder="Tìm theo mã phiếu, tên sách, mã vạch..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>

        <select
          className="filter-select"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
        >
          <option value="all">Tất cả trạng thái ({borrows.length})</option>
          <option value="Borrowing">Đang mượn</option>
          <option value="Returned">Đã trả đủ</option>
        </select>
      </div>

      <div className="table-card">
        {loading ? (
          <div className="loading-banner">
            <RefreshCw size={18} className="spin" />
            <span>Đang tải dữ liệu phiếu mượn...</span>
          </div>
        ) : filteredBorrows.length === 0 ? (
          <div className="empty-state">
            <BookOpen size={48} color="#94a3b8" />
            <p>Không tìm thấy phiếu mượn nào phù hợp.</p>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="custom-table">
              <thead>
                <tr>
                  <th>Mã phiếu</th>
                  <th>Ngày mượn</th>
                  <th>Hạn trả</th>
                  <th>Số cuốn sách</th>
                  <th>Trạng thái</th>
                  <th>Ghi chú</th>
                  <th>Chi tiết</th>
                </tr>
              </thead>
              <tbody>
                {filteredBorrows.map((b) => {
                  const isOverdue = b.status === 'Borrowing' && new Date(b.dueDate) < new Date();
                  return (
                    <tr key={b.id}>
                      <td>
                        <strong>#{b.id}</strong>
                      </td>
                      <td>{new Date(b.borrowDate).toLocaleDateString('vi-VN')}</td>
                      <td>
                        <span style={{ color: isOverdue ? '#ef4444' : 'inherit', fontWeight: isOverdue ? 600 : 'normal' }}>
                          {new Date(b.dueDate).toLocaleDateString('vi-VN')}
                          {isOverdue && ' (Quá hạn)'}
                        </span>
                      </td>
                      <td>{b.details.length} cuốn</td>
                      <td>
                        <span className={`badge badge-${b.status.toLowerCase()}`}>
                          {b.status === 'Borrowing' ? 'Đang mượn' : 'Đã trả đủ'}
                        </span>
                      </td>
                      <td>{b.note || '—'}</td>
                      <td>
                        <button
                          className="btn-action btn-copy"
                          title="Xem chi tiết phiếu mượn"
                          onClick={() => setSelectedRecord(b)}
                        >
                          <Eye size={15} /> Xem chi tiết
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

      {/* Modal Chi Tiết Phiếu Mượn */}
      {selectedRecord && (
        <div className="modal-backdrop" onClick={() => setSelectedRecord(null)}>
          <div className="modal-box modal-lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <FileText size={20} color="#f59e0b" />
                <h3>Chi Tiết Phiếu Mượn #{selectedRecord.id}</h3>
              </div>
              <button className="btn-icon" onClick={() => setSelectedRecord(null)}>
                <X size={18} />
              </button>
            </div>

            <div style={{ padding: '20px' }}>
              <div
                style={{
                  display: 'grid',
                  gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
                  gap: '12px',
                  background: '#0f172a',
                  padding: '16px',
                  borderRadius: '10px',
                  border: '1px solid #1e293b',
                  marginBottom: '20px',
                  fontSize: '13px',
                }}
              >
                <div>
                  <span style={{ color: '#94a3b8' }}>Ngày mượn: </span>
                  <strong>{new Date(selectedRecord.borrowDate).toLocaleDateString('vi-VN')}</strong>
                </div>
                <div>
                  <span style={{ color: '#94a3b8' }}>Hạn trả: </span>
                  <strong style={{ color: new Date(selectedRecord.dueDate) < new Date() && selectedRecord.status === 'Borrowing' ? '#ef4444' : '#f8fafc' }}>
                    {new Date(selectedRecord.dueDate).toLocaleDateString('vi-VN')}
                  </strong>
                </div>
                <div>
                  <span style={{ color: '#94a3b8' }}>Trạng thái phiếu: </span>
                  <span className={`badge badge-${selectedRecord.status.toLowerCase()}`}>
                    {selectedRecord.status === 'Borrowing' ? 'Đang mượn' : 'Đã hoàn tất'}
                  </span>
                </div>
                {selectedRecord.note && (
                  <div style={{ gridColumn: '1 / -1' }}>
                    <span style={{ color: '#94a3b8' }}>Ghi chú: </span>
                    <span>{selectedRecord.note}</span>
                  </div>
                )}
              </div>

              <h4 style={{ margin: '0 0 12px 0', fontSize: '15px', color: '#f8fafc' }}>
                Danh sách sách trong phiếu:
              </h4>

              <div className="table-responsive">
                <table className="custom-table">
                  <thead>
                    <tr>
                      <th>Tựa sách</th>
                      <th>Mã vạch (Barcode)</th>
                      <th>Tác giả</th>
                      <th>Ngày trả thực tế</th>
                      <th>Trạng thái</th>
                      <th>Tiền phạt</th>
                    </tr>
                  </thead>
                  <tbody>
                    {selectedRecord.details.map((d) => (
                      <tr key={d.id}>
                        <td>
                          <strong>{d.bookTitle}</strong>
                        </td>
                        <td>
                          <code>{d.barcode}</code>
                        </td>
                        <td>{d.bookAuthor}</td>
                        <td>
                          {d.returnDate
                            ? new Date(d.returnDate).toLocaleDateString('vi-VN')
                            : 'Chưa trả'}
                        </td>
                        <td>
                          <span
                            style={{
                              color: d.status === 'Returned' ? '#10b981' : '#f59e0b',
                              fontWeight: 600,
                            }}
                          >
                            {d.status === 'Returned' ? 'Đã trả' : 'Đang mượn'}
                          </span>
                        </td>
                        <td>
                          {d.fineAmount > 0 ? (
                            <strong style={{ color: '#ef4444' }}>
                              {d.fineAmount.toLocaleString('vi-VN')} VNĐ
                            </strong>
                          ) : (
                            '0 VNĐ'
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="modal-actions" style={{ padding: '16px 20px' }}>
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => setSelectedRecord(null)}
              >
                Đóng
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
