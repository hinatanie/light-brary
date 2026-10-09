import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import type { BorrowRecord } from '../../types';
import { borrowRecordsApi } from '../../api';
import { useAuth } from '../../contexts/AuthContext';
import { useToast } from '../../contexts/ToastContext';
import {
  BookOpen,
  Calendar,
  AlertCircle,
  CheckCircle2,
  Clock,
  ArrowRight,
  RefreshCw,
  Library
} from 'lucide-react';

export const MemberDashboardPage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const { showToast } = useToast();

  const [loading, setLoading] = useState(true);
  const [myBorrows, setMyBorrows] = useState<BorrowRecord[]>([]);

  const loadData = async () => {
    setLoading(true);
    try {
      const records = await borrowRecordsApi.getMyBorrowRecords();
      setMyBorrows(records);
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi tải thông tin mượn sách của bạn', 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  // Currently borrowed details
  const activeRecords = myBorrows.filter((b) => b.status === 'Borrowing' || b.status === 'Overdue');
  const borrowingDetails: {
    recordId: string;
    bookTitle: string;
    barcode: string;
    dueDate: string;
    isOverdue: boolean;
  }[] = [];

  activeRecords.forEach((rec) => {
    (rec.details || []).forEach((d) => {
      if (d.status !== 'Returned') {
        const isOverdue = Boolean(d.isOverdue ?? rec.status === 'Overdue');
        borrowingDetails.push({
          recordId: String(rec.id),
          bookTitle: d.bookTitle,
          barcode: d.barcode,
          dueDate: rec.dueDate,
          isOverdue,
        });
      }
    });
  });

  const returnedRecords = myBorrows.filter((b) => b.status === 'Returned');
  const overdueCount = borrowingDetails.filter((b) => b.isOverdue).length;

  return (
    <div className="tab-container">
      <div className="section-header">
        <div>
          <h2>👋 Xin Chào, {user?.fullName || 'Bạn Đọc'}!</h2>
          <p className="section-desc">
            Theo dõi sách đang giữ, thời hạn trả sách và trạng thái các phiếu mượn của bạn
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

      {loading ? (
        <div className="loading-banner">
          <RefreshCw size={18} className="spin" />
          <span>Đang tải thông tin sách của bạn...</span>
        </div>
      ) : (
        <>
          <div className="stats-grid">
            <div className="stat-card" style={{ cursor: 'default' }}>
              <div className="stat-icon" style={{ backgroundColor: '#e0f2fe', color: '#0284c7' }}>
                <BookOpen size={24} />
              </div>
              <div className="stat-info">
                <span className="stat-value">{borrowingDetails.length}</span>
                <span className="stat-label">Cuốn sách đang mượn</span>
              </div>
            </div>

            <div
              className="stat-card"
              style={{
                cursor: 'default',
                borderLeft: overdueCount > 0 ? '4px solid #ef4444' : undefined,
              }}
            >
              <div
                className="stat-icon"
                style={{
                  backgroundColor: overdueCount > 0 ? 'rgba(239, 68, 68, 0.15)' : '#fef3c7',
                  color: overdueCount > 0 ? '#ef4444' : '#d97706',
                }}
              >
                {overdueCount > 0 ? <AlertCircle size={24} /> : <Clock size={24} />}
              </div>
              <div className="stat-info">
                <span className="stat-value" style={{ color: overdueCount > 0 ? '#ef4444' : 'inherit' }}>
                  {overdueCount}
                </span>
                <span className="stat-label">Cuốn sách quá hạn</span>
              </div>
            </div>

            <div className="stat-card" style={{ cursor: 'default' }}>
              <div className="stat-icon" style={{ backgroundColor: '#dcfce7', color: '#16a34a' }}>
                <CheckCircle2 size={24} />
              </div>
              <div className="stat-info">
                <span className="stat-value">{returnedRecords.length}</span>
                <span className="stat-label">Phiếu mượn đã hoàn tất</span>
              </div>
            </div>
          </div>

          {/* Sách đang mượn list */}
          <div className="table-card" style={{ marginTop: '24px' }}>
            <div
              style={{
                padding: '16px 20px',
                borderBottom: '1px solid #1e293b',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
              }}
            >
              <h3 style={{ margin: 0, fontSize: '16px', display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Library size={18} color="#f59e0b" /> Danh Sách Sách Bạn Đang Giữ
              </h3>
              <button
                className="btn-action btn-copy"
                onClick={() => navigate('/member/borrows')}
              >
                Xem tất cả phiếu mượn <ArrowRight size={14} />
              </button>
            </div>

            {borrowingDetails.length === 0 ? (
              <div className="empty-state" style={{ padding: '40px 20px' }}>
                <CheckCircle2 size={44} color="#10b981" />
                <h4 style={{ margin: '12px 0 6px 0', color: '#0f172a' }}>
                  Bạn hiện không giữ cuốn sách nào
                </h4>
                <p style={{ margin: '0 0 16px 0', color: '#64748b', fontSize: '14px' }}>
                  Hãy khám phá kho sách thư viện và đăng ký mượn những cuốn sách bạn yêu thích ngay hôm nay!
                </p>
                <button
                  className="btn btn-primary"
                  onClick={() => navigate('/member/catalog')}
                >
                  <BookOpen size={16} /> Tra cứu kho sách & Mượn ngay
                </button>
              </div>
            ) : (
              <div className="table-responsive">
                <table className="custom-table">
                  <thead>
                    <tr>
                      <th>Tựa sách</th>
                      <th>Mã bản sao (Barcode)</th>
                      <th>Thuộc phiếu</th>
                      <th>Hạn trả sách</th>
                      <th>Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    {borrowingDetails.map((item, idx) => (
                      <tr key={idx}>
                        <td>
                          <strong>{item.bookTitle}</strong>
                        </td>
                        <td>
                          <code>{item.barcode}</code>
                        </td>
                        <td>#{item.recordId}</td>
                        <td>
                          <span
                            style={{
                              color: item.isOverdue ? '#ef4444' : '#f8fafc',
                              fontWeight: item.isOverdue ? 600 : 'normal',
                              display: 'inline-flex',
                              alignItems: 'center',
                              gap: '6px',
                            }}
                          >
                            <Calendar size={14} />
                            {new Date(item.dueDate).toLocaleDateString('vi-VN')}
                          </span>
                        </td>
                        <td>
                          {item.isOverdue ? (
                            <span
                              className="badge"
                              style={{
                                background: 'rgba(239, 68, 68, 0.2)',
                                color: '#ef4444',
                                border: '1px solid rgba(239, 68, 68, 0.4)',
                              }}
                            >
                              ⚠️ Quá hạn trả
                            </span>
                          ) : (
                            <span
                              className="badge"
                              style={{
                                background: 'rgba(59, 130, 246, 0.2)',
                                color: '#3b82f6',
                                border: '1px solid rgba(59, 130, 246, 0.4)',
                              }}
                            >
                              Đang mượn
                            </span>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
};
