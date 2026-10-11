import React, { useState } from 'react';
import type { User, BorrowRecord } from '../types';
import { usersApi } from '../api';
import {
  Search,
  History,
  Mail,
  Phone,
  MapPin,
  X,
  BookOpen,
  Shield,
  Briefcase,
  UserCheck,
  Check,
  RefreshCw,
  UserPlus,
  Eye,
  EyeOff,
  Lock,
  User as UserIcon
} from 'lucide-react';

interface UsersTabProps {
  users: User[];
  onNotify: (text: string, type?: 'success' | 'error') => void;
  onRefresh?: () => void;
}

export const UsersTab: React.FC<UsersTabProps> = ({ users, onNotify, onRefresh }) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [userBorrows, setUserBorrows] = useState<BorrowRecord[]>([]);
  const [loadingBorrows, setLoadingBorrows] = useState(false);
  const [isHistoryModalOpen, setIsHistoryModalOpen] = useState(false);

  // Role Management State
  const [isRoleModalOpen, setIsRoleModalOpen] = useState(false);
  const [roleUser, setRoleUser] = useState<User | null>(null);
  const [userRoles, setUserRoles] = useState<string[]>([]);
  const [submittingRoles, setSubmittingRoles] = useState(false);

  // Create User Modal State
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [createFullName, setCreateFullName] = useState('');
  const [createPhoneNumber, setCreatePhoneNumber] = useState('');
  const [createPassword, setCreatePassword] = useState('');
  const [createAddress, setCreateAddress] = useState('');
  const [createRoles, setCreateRoles] = useState<string[]>(['Member']);
  const [showPassword, setShowPassword] = useState(false);
  const [submittingCreate, setSubmittingCreate] = useState(false);

  // Password Reset Modal State
  const [isPasswordModalOpen, setIsPasswordModalOpen] = useState(false);
  const [passwordUser, setPasswordUser] = useState<User | null>(null);
  const [newPassword, setNewPassword] = useState('');
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [submittingPassword, setSubmittingPassword] = useState(false);

  const openPasswordModal = (user: User) => {
    setPasswordUser(user);
    setNewPassword('');
    setShowNewPassword(false);
    setIsPasswordModalOpen(true);
  };

  const handleUpdatePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!passwordUser) return;
    const pwd = newPassword.trim();
    if (pwd.length < 6) {
      onNotify('Mật khẩu mới phải có ít nhất 6 ký tự!', 'error');
      return;
    }
    setSubmittingPassword(true);
    try {
      await usersApi.updatePassword(passwordUser.id, pwd);
      onNotify(`Đã đổi mật khẩu thành công cho ${passwordUser.fullName}!`, 'success');
      setIsPasswordModalOpen(false);
    } catch (err: any) {
      onNotify(err.message || 'Cập nhật mật khẩu thất bại.', 'error');
    } finally {
      setSubmittingPassword(false);
    }
  };

  const resetCreateForm = () => {
    setCreateFullName('');
    setCreatePhoneNumber('');
    setCreatePassword('');
    setCreateAddress('');
    setCreateRoles(['Member']);
    setShowPassword(false);
  };

  const openCreateModal = () => {
    resetCreateForm();
    setIsCreateModalOpen(true);
  };

  const toggleCreateRole = (role: string) => {
    setCreateRoles((prev) => {
      if (prev.includes(role)) {
        if (prev.length === 1) {
          onNotify('Người dùng phải có ít nhất 1 vai trò!', 'error');
          return prev;
        }
        return prev.filter((r) => r !== role);
      } else {
        return [...prev, role];
      }
    });
  };

  const handleCreateUser = async (e: React.FormEvent) => {
    e.preventDefault();
    const fullName = createFullName.trim();
    const phoneNumber = createPhoneNumber.trim();
    const password = createPassword.trim();
    const address = createAddress.trim();

    if (!fullName) {
      onNotify('Vui lòng nhập họ và tên người dùng!', 'error');
      return;
    }

    if (!address) {
      onNotify('Vui lòng nhập địa chỉ!', 'error');
      return;
    }

    if (!phoneNumber) {
      onNotify('Vui lòng nhập số điện thoại!', 'error');
      return;
    }

    if (!/^\d{10}$/.test(phoneNumber)) {
      onNotify('Số điện thoại phải gồm đúng 10 chữ số!', 'error');
      return;
    }

    if (password.length < 8) {
      onNotify('Mật khẩu khởi tạo phải có ít nhất 8 ký tự!', 'error');
      return;
    }

    if (createRoles.length === 0) {
      onNotify('Vui lòng chọn ít nhất 1 vai trò cho người dùng!', 'error');
      return;
    }

    setSubmittingCreate(true);
    try {
      const newUser = await usersApi.create({
        fullName,
        phoneNumber,
        password,
        address,
        roles: createRoles,
      });

      onNotify(`Tạo tài khoản cho "${newUser.fullName}" (${phoneNumber}) thành công!`);
      setIsCreateModalOpen(false);
      resetCreateForm();
      if (onRefresh) onRefresh();
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi tạo người dùng mới', 'error');
    } finally {
      setSubmittingCreate(false);
    }
  };

  const viewUserHistory = async (user: User) => {
    setSelectedUser(user);
    setLoadingBorrows(true);
    setIsHistoryModalOpen(true);
    try {
      const records = await usersApi.getUserBorrowRecords(user.id);
      setUserBorrows(records);
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi tải lịch sử mượn trả của người dùng', 'error');
    } finally {
      setLoadingBorrows(false);
    }
  };

  const normalizeRoleForUI = (r: string): string => {
    const lower = r.toLowerCase();
    if (lower === 'librarian') return 'Staff';
    if (lower === 'user') return 'Member';
    return r;
  };

  const openRoleModal = (user: User) => {
    setRoleUser(user);
    setUserRoles(
      user.roles && user.roles.length > 0
        ? user.roles.map(normalizeRoleForUI)
        : ['Member']
    );
    setIsRoleModalOpen(true);
  };

  const toggleRole = (role: string) => {
    setUserRoles((prev) => {
      if (prev.includes(role)) {
        if (prev.length === 1) {
          onNotify('Người dùng phải có ít nhất 1 vai trò!', 'error');
          return prev;
        }
        return prev.filter((r) => r !== role);
      } else {
        return [...prev, role];
      }
    });
  };

  const handleSaveRoles = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!roleUser) return;
    if (userRoles.length === 0) {
      onNotify('Người dùng phải có ít nhất 1 vai trò!', 'error');
      return;
    }

    setSubmittingRoles(true);
    try {
      await usersApi.updateRoles(roleUser.id, userRoles);
      onNotify(`Đã cập nhật vai trò cho "${roleUser.fullName}" thành công!`);
      setIsRoleModalOpen(false);
      if (onRefresh) onRefresh();
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi cập nhật vai trò người dùng', 'error');
    } finally {
      setSubmittingRoles(false);
    }
  };

  const filteredUsers = users.filter(
    (u) =>
      u.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      (u.email && u.email.toLowerCase().includes(searchTerm.toLowerCase())) ||
      (u.phoneNumber && u.phoneNumber.includes(searchTerm))
  );

  const renderRoleBadges = (roles?: string[]) => {
    if (!roles || roles.length === 0) {
      return (
        <span
          className="badge"
          style={{ background: '#334155', color: '#94a3b8', fontSize: '11px', padding: '2px 8px' }}
        >
          Chưa gán
        </span>
      );
    }

    return (
      <div style={{ display: 'flex', gap: '4px', flexWrap: 'wrap' }}>
        {roles.map((rawRole) => {
          const role = rawRole.toLowerCase();
          if (role === 'admin') {
            return (
              <span
                key={rawRole}
                className="badge"
                style={{
                  background: 'rgba(239, 68, 68, 0.15)',
                  color: '#dc2626',
                  border: '1px solid rgba(239, 68, 68, 0.35)',
                  fontSize: '11px',
                  padding: '2px 8px',
                  borderRadius: '12px',
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: '4px',
                  fontWeight: 600,
                }}
              >
                <Shield size={10} /> Admin
              </span>
            );
          }
          if (role === 'staff' || role === 'librarian') {
            return (
              <span
                key={rawRole}
                className="badge"
                style={{
                  background: 'rgba(59, 130, 246, 0.15)',
                  color: '#2563eb',
                  border: '1px solid rgba(59, 130, 246, 0.35)',
                  fontSize: '11px',
                  padding: '2px 8px',
                  borderRadius: '12px',
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: '4px',
                  fontWeight: 600,
                }}
              >
                <Briefcase size={10} /> Staff
              </span>
            );
          }
          return (
            <span
              key={rawRole}
              className="badge"
              style={{
                background: 'rgba(16, 185, 129, 0.15)',
                color: '#059669',
                border: '1px solid rgba(16, 185, 129, 0.35)',
                fontSize: '11px',
                padding: '2px 8px',
                borderRadius: '12px',
                display: 'inline-flex',
                alignItems: 'center',
                gap: '4px',
                fontWeight: 600,
              }}
            >
              <UserCheck size={10} /> Member
            </span>
          );
        })}
      </div>
    );
  };

  return (
    <div className="tab-container">
      <div className="section-header">
        <div>
          <h2>👥 Quản Lý Người Dùng & Phân Quyền</h2>
          <p className="section-desc">
            Danh sách tài khoản độc giả, nhân viên, quản trị viên và cấu hình phân quyền hệ thống
          </p>
        </div>
        <button className="btn btn-primary" onClick={openCreateModal}>
          <UserPlus size={18} /> Thêm Người Dùng
        </button>
      </div>

      <div className="filter-bar">
        <div className="search-input-wrapper">
          <Search size={18} className="search-icon" />
          <input
            type="text"
            placeholder="Tìm theo tên người dùng, email, số điện thoại..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
      </div>

      <div className="table-card">
        {filteredUsers.length === 0 ? (
          <div className="empty-state">
            <p>Không tìm thấy người dùng nào phù hợp.</p>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="custom-table">
              <thead>
                <tr>
                  <th>Mã ID</th>
                  <th>Họ và tên</th>
                  <th>Email</th>
                  <th>Số điện thoại</th>
                  <th>Vai trò</th>
                  <th>Địa chỉ</th>
                  <th>Ngày tham gia</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {filteredUsers.map((u) => (
                  <tr key={u.id}>
                    <td>
                      <code>{String(u.id).slice(0, 8)}</code>
                    </td>
                    <td>
                      <strong>{u.fullName}</strong>
                    </td>
                    <td>
                      <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                        <Mail size={14} color="#64748b" /> {u.email || u.phoneNumber}
                      </span>
                    </td>
                    <td>
                      {u.phoneNumber ? (
                        <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                          <Phone size={14} color="#64748b" /> {u.phoneNumber}
                        </span>
                      ) : (
                        '—'
                      )}
                    </td>
                    <td>{renderRoleBadges(u.roles)}</td>
                    <td>
                      {u.address ? (
                        <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                          <MapPin size={14} color="#64748b" /> {u.address}
                        </span>
                      ) : (
                        '—'
                      )}
                    </td>
                    <td>{u.createdAt ? new Date(u.createdAt).toLocaleDateString('vi-VN') : '—'}</td>
                    <td>
                      <div className="action-buttons">
                        <button
                          className="btn-action"
                          style={{ color: '#f59e0b' }}
                          title="Phân quyền tài khoản"
                          onClick={() => openRoleModal(u)}
                        >
                          <Shield size={15} /> Quyền
                        </button>
                        <button
                          className="btn-action"
                          style={{ color: '#38bdf8' }}
                          title="Đặt lại mật khẩu cho tài khoản"
                          onClick={() => openPasswordModal(u)}
                        >
                          <Lock size={15} /> Mật khẩu
                        </button>
                        <button
                          className="btn-action btn-copy"
                          title="Xem lịch sử mượn sách"
                          onClick={() => viewUserHistory(u)}
                        >
                          <History size={15} /> Lịch sử
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modal Thêm Người Dùng Mới */}
      {isCreateModalOpen && (
        <div className="modal-backdrop" onClick={() => setIsCreateModalOpen(false)}>
          <div className="modal-box modal-lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <UserPlus size={20} color="#f59e0b" />
                <h3>Thêm Người Dùng Mới</h3>
              </div>
              <button className="btn-icon" onClick={() => setIsCreateModalOpen(false)}>
                <X size={18} />
              </button>
            </div>

            <form onSubmit={handleCreateUser} className="form-content">
              <div className="form-row">
                <div className="form-group">
                  <label>
                    <UserIcon size={15} /> Họ và tên *
                  </label>
                  <input
                    type="text"
                    required
                    maxLength={50}
                    placeholder="Ví dụ: Nguyễn Văn An"
                    value={createFullName}
                    onChange={(e) => setCreateFullName(e.target.value)}
                  />
                </div>

                <div className="form-group">
                  <label>
                    <Phone size={15} /> Số điện thoại (tài khoản đăng nhập) *
                  </label>
                  <input
                    type="tel"
                    required
                    maxLength={10}
                    placeholder="Ví dụ: 0912345678 (10 số)"
                    value={createPhoneNumber}
                    onChange={(e) => setCreatePhoneNumber(e.target.value.replace(/\D/g, ''))}
                  />
                  <span style={{ fontSize: '11px', color: '#64748b' }}>
                    Tối đa 10 chữ số, dùng làm tài khoản đăng nhập hệ thống.
                  </span>
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>
                    <Lock size={15} /> Mật khẩu khởi tạo *
                  </label>
                  <div style={{ position: 'relative' }}>
                    <input
                      type={showPassword ? 'text' : 'password'}
                      required
                      minLength={8}
                      placeholder="Tối thiểu 8 ký tự"
                      value={createPassword}
                      onChange={(e) => setCreatePassword(e.target.value)}
                      style={{ width: '100%', paddingRight: '40px' }}
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      style={{
                        position: 'absolute',
                        right: '10px',
                        top: '50%',
                        transform: 'translateY(-50%)',
                        background: 'transparent',
                        border: 'none',
                        color: '#64748b',
                        cursor: 'pointer',
                        display: 'flex',
                        alignItems: 'center',
                        padding: '4px',
                      }}
                      title={showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
                    >
                      {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
                    </button>
                  </div>
                  <span style={{ fontSize: '11px', color: '#64748b' }}>
                    Mật khẩu ban đầu để người dùng đăng nhập hệ thống (tối thiểu 8 ký tự).
                  </span>
                </div>

                <div className="form-group">
                  <label>
                    <MapPin size={15} /> Địa chỉ liên hệ *
                  </label>
                  <input
                    type="text"
                    maxLength={255}
                    placeholder="Ví dụ: 123 Lê Lợi, Phường Bến Nghé, Quận 1"
                    required
                    value={createAddress}
                    onChange={(e) => setCreateAddress(e.target.value)}
                  />
                  <span style={{ fontSize: '11px', color: '#64748b' }}>
                    Địa chỉ cư trú hoặc nơi làm việc của người dùng.
                  </span>
                </div>
              </div>

              <div style={{ marginTop: '8px' }}>
                <label style={{ display: 'block', marginBottom: '8px', fontSize: '13px', fontWeight: 600, color: '#334155' }}>
                  Phân quyền vai trò ban đầu *:
                </label>

                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '10px' }}>
                  {/* Role Member */}
                  <div
                    onClick={() => toggleCreateRole('Member')}
                    style={{
                      display: 'flex',
                      alignItems: 'flex-start',
                      gap: '10px',
                      padding: '12px 14px',
                      borderRadius: '8px',
                      background: createRoles.includes('Member') ? 'rgba(16, 185, 129, 0.08)' : '#f8fafc',
                      border: createRoles.includes('Member') ? '1.5px solid #10b981' : '1px solid #cbd5e1',
                      cursor: 'pointer',
                      userSelect: 'none',
                      transition: 'all 0.15s ease',
                    }}
                  >
                    <div
                      style={{
                        width: '18px',
                        height: '18px',
                        marginTop: '2px',
                        borderRadius: '4px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        background: createRoles.includes('Member') ? '#10b981' : '#fff',
                        border: '1.5px solid #10b981',
                        color: '#fff',
                        flexShrink: 0,
                      }}
                    >
                      {createRoles.includes('Member') && <Check size={12} />}
                    </div>
                    <div>
                      <strong style={{ color: '#059669', display: 'flex', alignItems: 'center', gap: '5px', fontSize: '13px' }}>
                        <UserCheck size={14} /> Member (Độc giả)
                      </strong>
                      <span style={{ fontSize: '11px', color: '#64748b', display: 'block', marginTop: '2px' }}>
                        Mượn sách, tra cứu và xem lịch sử cá nhân
                      </span>
                    </div>
                  </div>

                  {/* Role Staff */}
                  <div
                    onClick={() => toggleCreateRole('Staff')}
                    style={{
                      display: 'flex',
                      alignItems: 'flex-start',
                      gap: '10px',
                      padding: '12px 14px',
                      borderRadius: '8px',
                      background: createRoles.includes('Staff') ? 'rgba(59, 130, 246, 0.08)' : '#f8fafc',
                      border: createRoles.includes('Staff') ? '1.5px solid #3b82f6' : '1px solid #cbd5e1',
                      cursor: 'pointer',
                      userSelect: 'none',
                      transition: 'all 0.15s ease',
                    }}
                  >
                    <div
                      style={{
                        width: '18px',
                        height: '18px',
                        marginTop: '2px',
                        borderRadius: '4px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        background: createRoles.includes('Staff') ? '#3b82f6' : '#fff',
                        border: '1.5px solid #3b82f6',
                        color: '#fff',
                        flexShrink: 0,
                      }}
                    >
                      {createRoles.includes('Staff') && <Check size={12} />}
                    </div>
                    <div>
                      <strong style={{ color: '#2563eb', display: 'flex', alignItems: 'center', gap: '5px', fontSize: '13px' }}>
                        <Briefcase size={14} /> Staff (Thủ thư)
                      </strong>
                      <span style={{ fontSize: '11px', color: '#64748b', display: 'block', marginTop: '2px' }}>
                        Xử lý mượn/trả và quản lý kho sách
                      </span>
                    </div>
                  </div>

                  {/* Role Admin */}
                  <div
                    onClick={() => toggleCreateRole('Admin')}
                    style={{
                      display: 'flex',
                      alignItems: 'flex-start',
                      gap: '10px',
                      padding: '12px 14px',
                      borderRadius: '8px',
                      background: createRoles.includes('Admin') ? 'rgba(239, 68, 68, 0.08)' : '#f8fafc',
                      border: createRoles.includes('Admin') ? '1.5px solid #ef4444' : '1px solid #cbd5e1',
                      cursor: 'pointer',
                      userSelect: 'none',
                      transition: 'all 0.15s ease',
                    }}
                  >
                    <div
                      style={{
                        width: '18px',
                        height: '18px',
                        marginTop: '2px',
                        borderRadius: '4px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        background: createRoles.includes('Admin') ? '#ef4444' : '#fff',
                        border: '1.5px solid #ef4444',
                        color: '#fff',
                        flexShrink: 0,
                      }}
                    >
                      {createRoles.includes('Admin') && <Check size={12} />}
                    </div>
                    <div>
                      <strong style={{ color: '#dc2626', display: 'flex', alignItems: 'center', gap: '5px', fontSize: '13px' }}>
                        <Shield size={14} /> Admin (Quản trị)
                      </strong>
                      <span style={{ fontSize: '11px', color: '#64748b', display: 'block', marginTop: '2px' }}>
                        Toàn quyền cấu hình & quản trị hệ thống
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              <div className="modal-actions" style={{ marginTop: '16px' }}>
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
                  disabled={submittingCreate}
                >
                  {submittingCreate ? (
                    <>
                      <RefreshCw size={16} className="spin" /> Đang tạo...
                    </>
                  ) : (
                    <>
                      <UserPlus size={16} /> Tạo Người Dùng
                    </>
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Phân Quyền Vai Trò */}
      {isRoleModalOpen && roleUser && (
        <div className="modal-backdrop" onClick={() => setIsRoleModalOpen(false)}>
          <div className="modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Shield size={20} color="#f59e0b" />
                <h3>Phân Quyền: {roleUser.fullName}</h3>
              </div>
              <button className="btn-icon" onClick={() => setIsRoleModalOpen(false)}>
                <X size={18} />
              </button>
            </div>

            <form onSubmit={handleSaveRoles} className="form-content">
              <p style={{ margin: '0 0 16px 0', color: '#64748b', fontSize: '13px' }}>
                SĐT: <strong>{roleUser.phoneNumber || 'Chưa cập nhật'}</strong> | Mã ID: <code>{String(roleUser.id).slice(0, 8)}</code>
              </p>

              <div style={{ marginBottom: '20px' }}>
                <label style={{ display: 'block', marginBottom: '8px', fontSize: '13px', fontWeight: 600, color: '#334155' }}>
                  Chọn các vai trò kích hoạt:
                </label>

                <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                  {/* Role Admin */}
                  <div
                    onClick={() => toggleRole('Admin')}
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: '12px',
                      padding: '12px 14px',
                      borderRadius: '8px',
                      background: userRoles.includes('Admin') ? 'rgba(239, 68, 68, 0.08)' : '#f8fafc',
                      border: userRoles.includes('Admin') ? '1.5px solid #ef4444' : '1px solid #cbd5e1',
                      cursor: 'pointer',
                      userSelect: 'none',
                    }}
                  >
                    <div
                      style={{
                        width: '20px',
                        height: '20px',
                        borderRadius: '4px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        background: userRoles.includes('Admin') ? '#ef4444' : '#fff',
                        border: '1.5px solid #ef4444',
                        color: '#fff',
                      }}
                    >
                      {userRoles.includes('Admin') && <Check size={14} />}
                    </div>
                    <div>
                      <strong style={{ color: '#dc2626', display: 'flex', alignItems: 'center', gap: '6px' }}>
                        <Shield size={14} /> Admin (Quản trị viên)
                      </strong>
                      <span style={{ fontSize: '12px', color: '#64748b' }}>
                        Toàn quyền quản lý hệ thống, sách, người dùng và phân quyền
                      </span>
                    </div>
                  </div>

                  {/* Role Staff */}
                  <div
                    onClick={() => toggleRole('Staff')}
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: '12px',
                      padding: '12px 14px',
                      borderRadius: '8px',
                      background: userRoles.includes('Staff') ? 'rgba(59, 130, 246, 0.08)' : '#f8fafc',
                      border: userRoles.includes('Staff') ? '1.5px solid #3b82f6' : '1px solid #cbd5e1',
                      cursor: 'pointer',
                      userSelect: 'none',
                    }}
                  >
                    <div
                      style={{
                        width: '20px',
                        height: '20px',
                        borderRadius: '4px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        background: userRoles.includes('Staff') ? '#3b82f6' : '#fff',
                        border: '1.5px solid #3b82f6',
                        color: '#fff',
                      }}
                    >
                      {userRoles.includes('Staff') && <Check size={14} />}
                    </div>
                    <div>
                      <strong style={{ color: '#2563eb', display: 'flex', alignItems: 'center', gap: '6px' }}>
                        <Briefcase size={14} /> Staff (Nhân viên / Thủ thư)
                      </strong>
                      <span style={{ fontSize: '12px', color: '#64748b' }}>
                        Xử lý tạo phiếu mượn, xác nhận trả sách và tra cứu kho sách
                      </span>
                    </div>
                  </div>

                  {/* Role Member */}
                  <div
                    onClick={() => toggleRole('Member')}
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: '12px',
                      padding: '12px 14px',
                      borderRadius: '8px',
                      background: userRoles.includes('Member') ? 'rgba(16, 185, 129, 0.08)' : '#f8fafc',
                      border: userRoles.includes('Member') ? '1.5px solid #10b981' : '1px solid #cbd5e1',
                      cursor: 'pointer',
                      userSelect: 'none',
                    }}
                  >
                    <div
                      style={{
                        width: '20px',
                        height: '20px',
                        borderRadius: '4px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        background: userRoles.includes('Member') ? '#10b981' : '#fff',
                        border: '1.5px solid #10b981',
                        color: '#fff',
                      }}
                    >
                      {userRoles.includes('Member') && <Check size={14} />}
                    </div>
                    <div>
                      <strong style={{ color: '#059669', display: 'flex', alignItems: 'center', gap: '6px' }}>
                        <UserCheck size={14} /> Member (Độc giả / Khách hàng)
                      </strong>
                      <span style={{ fontSize: '12px', color: '#64748b' }}>
                        Xem sách đang mượn, lịch sử mượn sách và quản lý hồ sơ cá nhân
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setIsRoleModalOpen(false)}
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={submittingRoles}
                >
                  {submittingRoles ? (
                    <>
                      <RefreshCw size={16} className="spin" /> Đang lưu...
                    </>
                  ) : (
                    'Lưu phân quyền'
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

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
                  <p>Người dùng này chưa từng mượn cuốn sách nào.</p>
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

      {/* Modal Đặt lại Mật khẩu Người dùng */}
      {isPasswordModalOpen && passwordUser && (
        <div className="modal-backdrop" onClick={() => setIsPasswordModalOpen(false)}>
          <div className="modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Lock size={20} color="#f59e0b" />
                <h3>Đặt Lại Mật Khẩu</h3>
              </div>
              <button
                type="button"
                className="btn-icon"
                onClick={() => setIsPasswordModalOpen(false)}
                disabled={submittingPassword}
              >
                <X size={18} />
              </button>
            </div>

            <form onSubmit={handleUpdatePassword} className="form-content">
              <p style={{ margin: 0, fontSize: '13px', color: '#64748b' }}>
                Tài khoản: <strong>{passwordUser.fullName}</strong> ({passwordUser.phoneNumber || 'Không có SĐT'})
              </p>

              <div className="form-group">
                <label>
                  <Lock size={15} /> Mật khẩu mới * (Tối thiểu 6 ký tự)
                </label>
                <div style={{ position: 'relative', display: 'flex', alignItems: 'center' }}>
                  <input
                    type={showNewPassword ? 'text' : 'password'}
                    required
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    placeholder="Nhập mật khẩu mới"
                    style={{ width: '100%', paddingRight: '40px' }}
                  />
                  <button
                    type="button"
                    onClick={() => setShowNewPassword(!showNewPassword)}
                    style={{
                      position: 'absolute',
                      right: '10px',
                      top: '50%',
                      transform: 'translateY(-50%)',
                      background: 'none',
                      border: 'none',
                      cursor: 'pointer',
                      color: '#64748b',
                      display: 'flex',
                      alignItems: 'center',
                      padding: '4px',
                    }}
                    title={showNewPassword ? 'Ẩn' : 'Hiện'}
                  >
                    {showNewPassword ? <EyeOff size={16} /> : <Eye size={16} />}
                  </button>
                </div>
              </div>

              <div style={{ padding: '10px 12px', background: '#f8fafc', borderRadius: '8px', border: '1px solid #e2e8f0', fontSize: '12px', color: '#64748b' }}>
                💡 Quản trị viên có thể trực tiếp đặt lại mật khẩu cho tài khoản này mà không cần biết mật khẩu cũ.
              </div>

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setIsPasswordModalOpen(false)}
                  disabled={submittingPassword}
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={submittingPassword}
                  style={{ display: 'flex', alignItems: 'center', gap: '6px' }}
                >
                  {submittingPassword ? (
                    <>
                      <RefreshCw size={15} className="spin" /> Đang cập nhật...
                    </>
                  ) : (
                    <>
                      <Check size={15} /> Cập nhật mật khẩu
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
