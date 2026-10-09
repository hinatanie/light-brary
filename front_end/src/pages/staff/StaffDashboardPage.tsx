import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import type { BookCopy, BorrowRecord } from '../../types';
import { booksApi, bookCopiesApi, borrowRecordsApi } from '../../api';
import { useToast } from '../../contexts/ToastContext';
import {
  RotateCcw,
  Library,
  AlertCircle,
  PlusCircle,
  ArrowUpRight,
  RefreshCw,
  Clock,
  CheckCircle2
} from 'lucide-react';

export const StaffDashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const { showToast } = useToast();

  const [loading, setLoading] = useState(true);
  const [copies, setCopies] = useState<BookCopy[]>([]);
  const [borrowRecords, setBorrowRecords] = useState<BorrowRecord[]>([]);

  const loadData = async () => {
    setLoading(true);
    try {
      const [bks, borrows] = await Promise.all([
        booksApi.getAll(),
        borrowRecordsApi.getAll(),
      ]);
      setBorrowRecords(borrows);

      const copiesArrays = await Promise.all(
        bks.map((b) => bookCopiesApi.getByBookId(b.id).catch(() => [] as BookCopy[]))
      );
      setCopies(copiesArrays.flat());
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi tải dữ liệu bàn làm việc', 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const activeBorrows = borrowRecords.filter((b) => b.status === 'Borrowing' || b.status === 'Overdue').length;
  const availableCopies = copies.filter((c) => c.status === 'Available').length;
  const overdueBorrows = borrowRecords.filter((b) => b.status === 'Overdue').length;

  return (
    <div className="tab-container">
      <div className="section-header">
        <div>
          <h2>🏢 Bàn Làm Việc Thủ Thư</h2>
          <p className="section-desc">
            Xử lý quy trình mượn - trả sách cho bạn đọc, kiểm tra sách quá hạn và tra cứu tình trạng kho
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
            onClick={() => navigate('/staff/borrows')}
          >
            <PlusCircle size={16} /> Tạo Phiếu Mượn Mới
          </button>
        </div>
      </div>

      {loading ? (
        <div className="loading-banner">
          <RefreshCw size={18} className="spin" />
          <span>Đang đồng bộ dữ liệu bàn làm việc...</span>
        </div>
      ) : (
        <>
          <div className="stats-grid">
            <div className="stat-card" onClick={() => navigate('/staff/borrows')}>
              <div className="stat-icon" style={{ backgroundColor: '#fee2e2', color: '#dc2626' }}>
                <RotateCcw size={24} />
              </div>
              <div className="stat-info">
                <span className="stat-value">{activeBorrows}</span>
                <span className="stat-label">Phiếu đang mượn cần theo dõi</span>
              </div>
              <ArrowUpRight size={18} className="stat-arrow" />
            </div>

            <div className="stat-card" onClick={() => navigate('/staff/borrows')} style={{ borderLeft: '4px solid #ef4444' }}>
              <div className="stat-icon" style={{ backgroundColor: 'rgba(239, 68, 68, 0.15)', color: '#ef4444' }}>
                <AlertCircle size={24} />
              </div>
              <div className="stat-info">
                <span className="stat-value" style={{ color: overdueBorrows > 0 ? '#ef4444' : '#f8fafc' }}>
                  {overdueBorrows}
                </span>
                <span className="stat-label">Phiếu quá hạn trả</span>
              </div>
              <ArrowUpRight size={18} className="stat-arrow" />
            </div>

            <div className="stat-card" onClick={() => navigate('/staff/book-copies')}>
              <div className="stat-icon" style={{ backgroundColor: '#dcfce7', color: '#16a34a' }}>
                <Library size={24} />
              </div>
              <div className="stat-info">
                <span className="stat-value">{availableCopies}</span>
                <span className="stat-label">Cuốn sách sẵn sàng cho mượn</span>
              </div>
              <ArrowUpRight size={18} className="stat-arrow" />
            </div>
          </div>

          {/* Quick Actions Panel */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '16px', marginTop: '20px' }}>
            <div
              className="table-card"
              style={{
                padding: '20px',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                gap: '16px',
                border: '1px solid #334155',
                transition: 'border-color 0.2s',
              }}
              onClick={() => navigate('/staff/borrows')}
            >
              <div style={{ background: 'rgba(245, 158, 11, 0.15)', color: '#f59e0b', padding: '14px', borderRadius: '12px' }}>
                <PlusCircle size={28} />
              </div>
              <div>
                <h4 style={{ margin: '0 0 4px 0', fontSize: '16px', color: '#f8fafc' }}>Tạo Phiếu Mượn Cho Bạn Đọc</h4>
                <p style={{ margin: 0, fontSize: '13px', color: '#94a3b8' }}>
                  Chọn độc giả, hẹn ngày trả và quét mã vạch sách
                </p>
              </div>
            </div>

            <div
              className="table-card"
              style={{
                padding: '20px',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                gap: '16px',
                border: '1px solid #334155',
                transition: 'border-color 0.2s',
              }}
              onClick={() => navigate('/staff/borrows')}
            >
              <div style={{ background: 'rgba(16, 185, 129, 0.15)', color: '#10b981', padding: '14px', borderRadius: '12px' }}>
                <CheckCircle2 size={28} />
              </div>
              <div>
                <h4 style={{ margin: '0 0 4px 0', fontSize: '16px', color: '#f8fafc' }}>Xác Nhận Trả Sách</h4>
                <p style={{ margin: 0, fontSize: '13px', color: '#94a3b8' }}>
                  Kiểm tra tình trạng sách, nhận sách hoàn trả và cập nhật trạng thái phiếu
                </p>
              </div>
            </div>
          </div>

          {/* Recent Borrows */}
          <div className="table-card" style={{ marginTop: '24px' }}>
            <div style={{ padding: '16px 20px', borderBottom: '1px solid #1e293b', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <h3 style={{ margin: 0, fontSize: '16px', display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Clock size={18} color="#f59e0b" /> Phiếu Mượn Cần Xử Lý Gần Đây
              </h3>
              <button
                className="btn-action btn-copy"
                onClick={() => navigate('/staff/borrows')}
              >
                Xem chi tiết
              </button>
            </div>
            <div className="table-responsive">
              <table className="custom-table">
                <thead>
                  <tr>
                    <th>Mã phiếu</th>
                    <th>Độc giả</th>
                    <th>Ngày mượn</th>
                    <th>Hạn trả</th>
                    <th>Số sách</th>
                    <th>Trạng thái</th>
                  </tr>
                </thead>
                <tbody>
                  {borrowRecords.slice(0, 6).map((b) => {
                    const isOverdue = b.status === 'Overdue' || (b.status === 'Borrowing' && b.details.some(d => d.isOverdue));
                    return (
                      <tr key={b.id}>
                        <td><strong>#{b.id}</strong></td>
                        <td>{b.userFullName}</td>
                        <td>{new Date(b.borrowDate).toLocaleDateString('vi-VN')}</td>
                        <td>
                          <span style={{ color: isOverdue ? '#ef4444' : 'inherit', fontWeight: isOverdue ? 600 : 'normal' }}>
                            {new Date(b.dueDate).toLocaleDateString('vi-VN')}
                            {isOverdue && ' (Quá hạn)'}
                          </span>
                        </td>
                        <td>{b.totalBooks ?? b.details?.length ?? 0} cuốn</td>
                        <td>
                          <span className={`badge badge-${b.status.toLowerCase()}`}>
                            {b.status === 'Borrowing'
                              ? 'Đang mượn'
                              : b.status === 'Returned'
                              ? 'Đã trả đủ'
                              : 'Quá hạn'}
                          </span>
                        </td>
                      </tr>
                    );
                  })}
                  {borrowRecords.length === 0 && (
                    <tr>
                      <td colSpan={6} style={{ textAlign: 'center', padding: '32px', color: '#94a3b8' }}>
                        Hiện không có phiếu mượn nào cần xử lý.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </div>
  );
};
