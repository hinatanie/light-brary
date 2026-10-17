import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Book, Category, BookCopy, BorrowRecord, User } from '../../types';
import { categoriesApi, booksApi, bookCopiesApi, borrowRecordsApi, usersApi } from '../../api';
import { useToast } from '../../contexts/ToastContext';
import {
  BookOpen,
  Bookmark,
  Library,
  FileText,
  Users,
  ArrowUpRight,
  RefreshCw,
  Clock,
  AlertCircle
} from 'lucide-react';

export const AdminDashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const { showToast } = useToast();

  const [loading, setLoading] = useState(true);
  const [books, setBooks] = useState<Book[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [copies, setCopies] = useState<BookCopy[]>([]);
  const [borrowRecords, setBorrowRecords] = useState<BorrowRecord[]>([]);
  const [users, setUsers] = useState<User[]>([]);

  const loadData = async () => {
    setLoading(true);
    try {
      const [bks, cats, borrows, usrs] = await Promise.all([
        booksApi.getAll(),
        categoriesApi.getAll(),
        borrowRecordsApi.getAll(),
        usersApi.getAll(),
      ]);
      setBooks(bks);
      setCategories(cats);
      setBorrowRecords(borrows);
      setUsers(usrs);

      // Load copies
      const copiesArrays = await Promise.all(
        bks.map((b) => bookCopiesApi.getByBookId(b.id).catch(() => [] as BookCopy[]))
      );
      setCopies(copiesArrays.flat());
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi tải dữ liệu tổng quan', 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const activeBorrows = borrowRecords.filter((b) => b.status === 'Borrowing' || b.status === 'Overdue').length;
  const availableCopies = copies.filter((c) => c.status === 'Available').length;
  const borrowedCopies = copies.filter((c) => c.status === 'Borrowed').length;
  const overdueBorrows = borrowRecords.filter((b) => b.status === 'Overdue').length;

  return (
    <div className="tab-container">
      <div className="section-header">
        <div>
          <h2>📊 Tổng Quan Hệ Thống Quản Trị Thư Viện</h2>
          <p className="section-desc">
            Toàn quyền quản lý tài nguyên sách, kho vật lý, quy trình mượn trả và phân quyền người dùng
          </p>
        </div>
        <button
          className="btn btn-secondary"
          onClick={loadData}
          disabled={loading}
          style={{ display: 'flex', alignItems: 'center', gap: '6px' }}
        >
          <RefreshCw size={16} className={loading ? 'spin' : ''} />
          <span>Làm mới</span>
        </button>
      </div>

      {loading ? (
        <div className="loading-banner">
          <RefreshCw size={18} className="spin" />
          <span>Đang đồng bộ dữ liệu hệ thống...</span>
        </div>
      ) : (
        <>
          <div className="stats-grid">
            <div className="stat-card" onClick={() => navigate('/admin/books')}>
              <div className="stat-icon" style={{ backgroundColor: '#e0f2fe', color: '#0284c7' }}>
                <BookOpen size={24} />
              </div>
              <div className="stat-info">
                <span className="stat-value">{books.length}</span>
                <span className="stat-label">Tổng đầu sách</span>
              </div>
              <ArrowUpRight size={18} className="stat-arrow" />
            </div>

            <div className="stat-card" onClick={() => navigate('/admin/categories')}>
              <div className="stat-icon" style={{ backgroundColor: '#fef3c7', color: '#d97706' }}>
                <Bookmark size={24} />
              </div>
              <div className="stat-info">
                <span className="stat-value">{categories.length}</span>
                <span className="stat-label">Thể loại sách</span>
              </div>
              <ArrowUpRight size={18} className="stat-arrow" />
            </div>

            <div className="stat-card" onClick={() => navigate('/admin/book-copies')}>
              <div className="stat-icon" style={{ backgroundColor: '#dcfce7', color: '#16a34a' }}>
                <Library size={24} />
              </div>
              <div className="stat-info">
                <span className="stat-value">{availableCopies} / {copies.length}</span>
                <span className="stat-label">Sẵn sàng ({borrowedCopies} đang mượn)</span>
              </div>
              <ArrowUpRight size={18} className="stat-arrow" />
            </div>

            <div className="stat-card" onClick={() => navigate('/admin/borrows')}>
              <div className="stat-icon" style={{ backgroundColor: '#fee2e2', color: '#dc2626' }}>
                <FileText size={24} />
              </div>
              <div className="stat-info">
                <span className="stat-value">{activeBorrows} / {borrowRecords.length}</span>
                <span className="stat-label">Phiếu đang mượn</span>
              </div>
              <ArrowUpRight size={18} className="stat-arrow" />
            </div>

            <div className="stat-card" onClick={() => navigate('/admin/users')}>
              <div className="stat-icon" style={{ backgroundColor: '#f3e8ff', color: '#9333ea' }}>
                <Users size={24} />
              </div>
              <div className="stat-info">
                <span className="stat-value">{users.length}</span>
                <span className="stat-label">Tài khoản người dùng</span>
              </div>
              <ArrowUpRight size={18} className="stat-arrow" />
            </div>

            <div className="stat-card" onClick={() => navigate('/admin/borrows')} style={{ borderLeft: '4px solid #ef4444' }}>
              <div className="stat-icon" style={{ backgroundColor: 'rgba(239, 68, 68, 0.15)', color: '#ef4444' }}>
                <AlertCircle size={24} />
              </div>
              <div className="stat-info">
                <span className="stat-value" style={{ color: overdueBorrows > 0 ? '#ef4444' : 'var(--text-main)' }}>
                  {overdueBorrows}
                </span>
                <span className="stat-label">Phiếu quá hạn</span>
              </div>
              <ArrowUpRight size={18} className="stat-arrow" />
            </div>
          </div>

          {/* Quick Recent Activity */}
          <div className="table-card" style={{ marginTop: '24px' }}>
            <div style={{ padding: '16px 20px', borderBottom: '1px solid var(--border-color)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <h3 style={{ margin: 0, fontSize: '16px', display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Clock size={18} color="var(--primary)" /> Phiếu Mượn Gần Đây Nhất
              </h3>
              <button
                className="btn-action btn-copy"
                onClick={() => navigate('/admin/borrows')}
              >
                Xem tất cả
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
                  {borrowRecords.slice(0, 5).map((b) => (
                    <tr key={b.id}>
                      <td><strong>#{b.id}</strong></td>
                      <td>{b.userFullName}</td>
                      <td>{new Date(b.borrowDate).toLocaleDateString('vi-VN')}</td>
                      <td>{new Date(b.dueDate).toLocaleDateString('vi-VN')}</td>
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
                  ))}
                  {borrowRecords.length === 0 && (
                    <tr>
                      <td colSpan={6} style={{ textAlign: 'center', padding: '32px', color: 'var(--text-muted)' }}>
                        Chưa có phiếu mượn nào trong hệ thống.
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
