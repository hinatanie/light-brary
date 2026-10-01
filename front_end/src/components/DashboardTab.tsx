import React from 'react';
import type { Book, Category, BookCopy, BorrowRecord, User } from '../types';
import { BookOpen, Bookmark, Library, FileText, Users, ArrowUpRight } from 'lucide-react';

interface DashboardTabProps {
  books: Book[];
  categories: Category[];
  copies: BookCopy[];
  borrowRecords: BorrowRecord[];
  users: User[];
  onNavigate: (tab: string) => void;
}

export const DashboardTab: React.FC<DashboardTabProps> = ({
  books,
  categories,
  copies,
  borrowRecords,
  users,
  onNavigate,
}) => {
  const activeBorrows = borrowRecords.filter((b) => b.status === 'Borrowing').length;
  const availableCopies = copies.filter((c) => c.status === 'Available').length;
  const borrowedCopies = copies.filter((c) => c.status === 'Borrowed').length;

  return (
    <div className="tab-container">
      <div className="section-header">
        <div>
          <h2>📊 Tổng Quan Hoạt Động Thư Viện</h2>
          <p className="section-desc">Theo dõi nhanh số lượng đầu sách, bản sao vật lý và phiếu mượn thời gian thực</p>
        </div>
      </div>

      <div className="stats-grid">
        <div className="stat-card" onClick={() => onNavigate('books')}>
          <div className="stat-icon" style={{ backgroundColor: '#e0f2fe', color: '#0284c7' }}>
            <BookOpen size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-value">{books.length}</span>
            <span className="stat-label">Tổng đầu sách</span>
          </div>
          <ArrowUpRight size={18} className="stat-arrow" />
        </div>

        <div className="stat-card" onClick={() => onNavigate('categories')}>
          <div className="stat-icon" style={{ backgroundColor: '#fef3c7', color: '#d97706' }}>
            <Bookmark size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-value">{categories.length}</span>
            <span className="stat-label">Thể loại sách</span>
          </div>
          <ArrowUpRight size={18} className="stat-arrow" />
        </div>

        <div className="stat-card" onClick={() => onNavigate('copies')}>
          <div className="stat-icon" style={{ backgroundColor: '#dcfce7', color: '#16a34a' }}>
            <Library size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-value">{availableCopies} / {copies.length}</span>
            <span className="stat-label">Sẵn sàng ({borrowedCopies} đang mượn)</span>
          </div>
          <ArrowUpRight size={18} className="stat-arrow" />
        </div>

        <div className="stat-card" onClick={() => onNavigate('borrows')}>
          <div className="stat-icon" style={{ backgroundColor: '#fee2e2', color: '#dc2626' }}>
            <FileText size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-value">{activeBorrows}</span>
            <span className="stat-label">Phiếu đang mượn</span>
          </div>
          <ArrowUpRight size={18} className="stat-arrow" />
        </div>

        <div className="stat-card" onClick={() => onNavigate('users')}>
          <div className="stat-icon" style={{ backgroundColor: '#f3e8ff', color: '#9333ea' }}>
            <Users size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-value">{users.length}</span>
            <span className="stat-label">Độc giả đăng ký</span>
          </div>
          <ArrowUpRight size={18} className="stat-arrow" />
        </div>
      </div>

      <div className="dashboard-sections">
        {/* Phiếu mượn gần đây */}
        <div className="card-box">
          <div className="card-header-line">
            <h3>Phiếu Mượn Gần Đây</h3>
            <button className="btn-link" onClick={() => onNavigate('borrows')}>Xem tất cả</button>
          </div>
          {borrowRecords.length === 0 ? (
            <p className="empty-text">Chưa có phiếu mượn nào được tạo.</p>
          ) : (
            <div className="table-responsive">
              <table className="custom-table">
                <thead>
                  <tr>
                    <th>Mã phiếu</th>
                    <th>Độc giả</th>
                    <th>Ngày mượn</th>
                    <th>Hạn trả</th>
                    <th>Số cuốn</th>
                    <th>Trạng thái</th>
                  </tr>
                </thead>
                <tbody>
                  {borrowRecords.slice(0, 5).map((br) => (
                    <tr key={br.id}>
                      <td><strong>#{br.id}</strong></td>
                      <td>{br.userFullName}</td>
                      <td>{new Date(br.borrowDate).toLocaleDateString('vi-VN')}</td>
                      <td>{new Date(br.dueDate).toLocaleDateString('vi-VN')}</td>
                      <td>{br.details.length} cuốn</td>
                      <td>
                        <span className={`badge badge-${br.status.toLowerCase()}`}>
                          {br.status === 'Borrowing' ? 'Đang mượn' : br.status === 'Returned' ? 'Đã trả' : 'Quá hạn'}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        {/* Đầu sách mới cập nhật */}
        <div className="card-box">
          <div className="card-header-line">
            <h3>Đầu Sách Trong Thư Viện</h3>
            <button className="btn-link" onClick={() => onNavigate('books')}>Quản lý sách</button>
          </div>
          {books.length === 0 ? (
            <p className="empty-text">Chưa có đầu sách nào trong kho.</p>
          ) : (
            <div className="table-responsive">
              <table className="custom-table">
                <thead>
                  <tr>
                    <th>Tên sách</th>
                    <th>Tác giả</th>
                    <th>Thể loại</th>
                    <th>Năm XB</th>
                  </tr>
                </thead>
                <tbody>
                  {books.slice(0, 5).map((b) => (
                    <tr key={b.id}>
                      <td><strong>{b.title}</strong></td>
                      <td>{b.author}</td>
                      <td><span className="category-tag">{b.categoryName || 'Chưa phân loại'}</span></td>
                      <td>{b.publishYear || '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
