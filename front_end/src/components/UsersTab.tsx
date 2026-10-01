import React, { useState } from 'react';
import type { User, BorrowRecord } from '../types';
import { usersApi } from '../api';
import { Search, History, Mail, Phone, MapPin, X, BookOpen } from 'lucide-react';

interface UsersTabProps {
  users: User[];
  onNotify: (text: string, type?: 'success' | 'error') => void;
}

export const UsersTab: React.FC<UsersTabProps> = ({ users, onNotify }) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [userBorrows, setUserBorrows] = useState<BorrowRecord[]>([]);
  const [loadingBorrows, setLoadingBorrows] = useState(false);
  const [isHistoryModalOpen, setIsHistoryModalOpen] = useState(false);

  const viewUserHistory = async (user: User) => {
    setSelectedUser(user);
    setLoadingBorrows(true);
    setIsHistoryModalOpen(true);
    try {
      const records = await usersApi.getUserBorrowRecords(user.id);
      setUserBorrows(records);
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi tải lịch sử mượn trả của độc giả', 'error');
    } finally {
      setLoadingBorrows(false);
    }
  };

  const filteredUsers = users.filter(
    (u) =>
      u.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      u.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
      (u.phoneNumber && u.phoneNumber.includes(searchTerm))
  );

  return (
    <div className="tab-container">
      <div className="section-header">
        <div>
          <h2>👥 Danh Sách Độc Giả Thư Viện</h2>
          <p className="section-desc">Quản lý tài khoản độc giả và tra cứu lịch sử mượn trả riêng biệt</p>
        </div>
      </div>

      <div className="filter-bar">
        <div className="search-input-wrapper">
          <Search size={18} className="search-icon" />
          <input
            type="text"
            placeholder="Tìm theo tên độc giả, email, số điện thoại..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
      </div>

      <div className="table-card">
        {filteredUsers.length === 0 ? (
          <div className="empty-state">
            <p>Không tìm thấy độc giả nào phù hợp.</p>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="custom-table">
              <thead>
                <tr>
                  <th>Mã độc giả</th>
                  <th>Họ và tên</th>
                  <th>Email</th>
                  <th>Số điện thoại</th>
                  <th>Địa chỉ</th>
                  <th>Ngày tham gia</th>
                  <th>Lịch sử mượn</th>
                </tr>
              </thead>
              <tbody>
                {filteredUsers.map((u) => (
                  <tr key={u.id}>
                    <td><code>{u.id.slice(0, 8)}...</code></td>
                    <td><strong>{u.fullName}</strong></td>
                    <td>
                      <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                        <Mail size={14} color="#64748b" /> {u.email}
                      </span>
                    </td>
                    <td>
                      {u.phoneNumber ? (
                        <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                          <Phone size={14} color="#64748b" /> {u.phoneNumber}
                        </span>
                      ) : '—'}
                    </td>
                    <td>
                      {u.address ? (
                        <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                          <MapPin size={14} color="#64748b" /> {u.address}
                        </span>
                      ) : '—'}
                    </td>
                    <td>{new Date(u.createdAt).toLocaleDateString('vi-VN')}</td>
                    <td>
                      <button
                        className="btn-action btn-copy"
                        onClick={() => viewUserHistory(u)}
                      >
                        <History size={16} /> Lịch sử mượn
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modal Lịch Sử Mượn Trả Của Độc Giả */}
      {isHistoryModalOpen && selectedUser && (
        <div className="modal-backdrop" onClick={() => setIsHistoryModalOpen(false)}>
          <div className="modal-box modal-lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>📜 Lịch Sử Mượn Trả: {selectedUser.fullName}</h3>
              <button className="btn-icon" onClick={() => setIsHistoryModalOpen(false)}>
                <X size={18} />
              </button>
            </div>

            <div style={{ padding: '16px 20px' }}>
              <p style={{ margin: '0 0 16px 0', color: '#64748b' }}>
                Email: <strong>{selectedUser.email}</strong> | Mã: <code>{selectedUser.id}</code>
              </p>

              {loadingBorrows ? (
                <p>Đang tải dữ liệu lịch sử...</p>
              ) : userBorrows.length === 0 ? (
                <div className="empty-state">
                  <BookOpen size={40} color="#94a3b8" />
                  <p>Độc giả này chưa từng mượn cuốn sách nào.</p>
                </div>
              ) : (
                <div className="table-responsive">
                  <table className="custom-table">
                    <thead>
                      <tr>
                        <th>Mã phiếu</th>
                        <th>Ngày mượn</th>
                        <th>Hạn trả</th>
                        <th>Danh sách sách</th>
                        <th>Trạng thái</th>
                      </tr>
                    </thead>
                    <tbody>
                      {userBorrows.map((b) => (
                        <tr key={b.id}>
                          <td><strong>#{b.id}</strong></td>
                          <td>{new Date(b.borrowDate).toLocaleDateString('vi-VN')}</td>
                          <td>{new Date(b.dueDate).toLocaleDateString('vi-VN')}</td>
                          <td>
                            {b.details.map((d) => (
                              <div key={d.id} style={{ fontSize: '13px', margin: '2px 0' }}>
                                • <strong>{d.bookTitle}</strong> (<code>{d.barcode}</code>) —{' '}
                                <span style={{ color: d.status === 'Returned' ? '#16a34a' : '#ea580c' }}>
                                  {d.status === 'Returned' ? 'Đã trả' : 'Đang mượn'}
                                </span>
                              </div>
                            ))}
                          </td>
                          <td>
                            <span className={`badge badge-${b.status.toLowerCase()}`}>
                              {b.status === 'Borrowing' ? 'Đang mượn' : 'Đã trả đủ'}
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>

            <div className="modal-actions" style={{ padding: '16px 20px' }}>
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => setIsHistoryModalOpen(false)}
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
