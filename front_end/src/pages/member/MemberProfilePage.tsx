import React, { useState, useEffect } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { useToast } from '../../contexts/ToastContext';
import { authApi } from '../../api';
import type { User, MeResponse } from '../../types';
import {
  User as UserIcon,
  Mail,
  Phone,
  MapPin,
  Shield,
  Briefcase,
  UserCheck,
  Save,
  RefreshCw,
  Calendar,
  KeyRound,
  Lock,
  Eye,
  EyeOff,
} from 'lucide-react';

export const MemberProfilePage: React.FC<{ title?: string }> = ({ title = 'Hồ Sơ Bạn Đọc Cá Nhân' }) => {
  const { user, updateUserSession } = useAuth();
  const { showToast } = useToast();

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [profileData, setProfileData] = useState<User | null>(null);

  // Form states
  const [fullName, setFullName] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [address, setAddress] = useState('');

  // Password states
  const [currentPwd, setCurrentPwd] = useState('');
  const [newPwd, setNewPwd] = useState('');
  const [confirmPwd, setConfirmPwd] = useState('');
  const [showCurrentPwd, setShowCurrentPwd] = useState(false);
  const [showNewPwd, setShowNewPwd] = useState(false);
  const [showConfirmPwd, setShowConfirmPwd] = useState(false);
  const [submittingPwd, setSubmittingPwd] = useState(false);

  const loadProfile = async () => {
    if (!user) return;
    setLoading(true);
    try {
      const me: MeResponse = await authApi.getMe();
      setFullName(me.fullName);
      setPhoneNumber(me.phoneNumber || '');
      setAddress(me.address || '');
      setProfileData({
        id: me.userId,
        fullName: me.fullName,
        email: me.email,
        phoneNumber: me.phoneNumber,
        address: me.address || '',
        createdAt: me.createdAt,
        roles: me.roles,
      });
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi tải thông tin cá nhân', 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadProfile();
  }, [user]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) return;

    if (!address.trim()) {
      showToast('Địa chỉ không được để trống.', 'error');
      return;
    }

    setSubmitting(true);
    try {
      const updated = await authApi.updateProfile({
        address: address.trim(),
      });

      updateUserSession({
        fullName: updated.fullName,
        phoneNumber: updated.phoneNumber,
      });

      setProfileData(updated);
      showToast('Cập nhật thông tin cá nhân thành công!');
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi cập nhật thông tin', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  const handlePasswordChange = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newPwd !== confirmPwd) {
      showToast('Mật khẩu mới và xác nhận mật khẩu không khớp!', 'error');
      return;
    }
    if (newPwd === currentPwd) {
      showToast('Mật khẩu mới không được trùng với mật khẩu hiện tại!', 'error');
      return;
    }
    if (newPwd.length < 8) {
      showToast('Mật khẩu mới phải có tối thiểu 8 ký tự!', 'error');
      return;
    }

    setSubmittingPwd(true);
    try {
      await authApi.changePassword({
        currentPassword: currentPwd,
        newPassword: newPwd,
      });
      showToast('Đổi mật khẩu thành công!');
      setCurrentPwd('');
      setNewPwd('');
      setConfirmPwd('');
    } catch (err: any) {
      showToast(err.message || 'Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu hiện tại.', 'error');
    } finally {
      setSubmittingPwd(false);
    }
  };

  if (loading) {
    return (
      <div className="tab-container">
        <div className="loading-banner">
          <RefreshCw size={18} className="spin" />
          <span>Đang tải thông tin cá nhân...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="tab-container">
      <div className="section-header">
        <div>
          <h2>👤 {title}</h2>
          <p className="section-desc">
            Xem và cập nhật thông tin liên lạc phục vụ công tác mượn trả và liên hệ thông báo
          </p>
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 320px), 1fr))', gap: '24px' }}>
        {/* Profile Info Card */}
        <div className="table-card" style={{ padding: '24px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '16px', marginBottom: '20px' }}>
            <div
              style={{
                width: '64px',
                height: '64px',
                borderRadius: '50%',
                background: 'var(--primary)',
                color: '#fff',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <UserIcon size={32} />
            </div>
            <div>
              <h3 style={{ margin: '0 0 6px 0', fontSize: '18px', color: 'var(--text-main)' }}>
                {profileData?.fullName || user?.fullName}
              </h3>
              <div style={{ display: 'flex', gap: '6px', flexWrap: 'wrap' }}>
                {user?.roles?.map((role) => (
                  <span
                    key={role}
                    className="badge"
                    style={{
                      background:
                        role === 'Admin'
                          ? 'rgba(239, 68, 68, 0.2)'
                          : role === 'Staff'
                          ? 'rgba(59, 130, 246, 0.2)'
                          : 'rgba(16, 185, 129, 0.2)',
                      color:
                        role === 'Admin' ? '#ef4444' : role === 'Staff' ? '#3b82f6' : '#10b981',
                      border: '1px solid currentColor',
                      fontSize: '11px',
                    }}
                  >
                    {role === 'Admin' && <Shield size={11} style={{ marginRight: '4px' }} />}
                    {role === 'Staff' && <Briefcase size={11} style={{ marginRight: '4px' }} />}
                    {role === 'Member' && <UserCheck size={11} style={{ marginRight: '4px' }} />}
                    {role}
                  </span>
                ))}
              </div>
            </div>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '14px', fontSize: '13px', borderTop: '1px solid var(--border-color)', paddingTop: '16px' }}>
            <div>
              <span style={{ color: 'var(--text-muted)', display: 'block', marginBottom: '2px' }}>Mã tài khoản:</span>
              <code>{profileData?.id || user?.userId}</code>
            </div>
            <div>
              <span style={{ color: 'var(--text-muted)', display: 'block', marginBottom: '2px' }}>Email đăng ký:</span>
              <span style={{ display: 'flex', alignItems: 'center', gap: '6px', color: 'var(--text-main)' }}>
                <Mail size={14} color="#64748b" /> {profileData?.email || user?.email}
              </span>
            </div>
            {profileData?.createdAt && (
              <div>
                <span style={{ color: 'var(--text-muted)', display: 'block', marginBottom: '2px' }}>Ngày tạo tài khoản:</span>
                <span style={{ display: 'flex', alignItems: 'center', gap: '6px', color: 'var(--text-main)' }}>
                  <Calendar size={14} color="#64748b" />{' '}
                  {new Date(profileData.createdAt).toLocaleDateString('vi-VN')}
                </span>
              </div>
            )}
          </div>
        </div>

        {/* Edit Form Card */}
        <div className="table-card" style={{ padding: '24px' }}>
          <h3 style={{ margin: '0 0 16px 0', fontSize: '16px', color: 'var(--text-main)' }}>
            ✏️ Cập Nhật Thông Tin Cá Nhân
          </h3>

          <form onSubmit={handleSubmit}>
            <div className="form-group" style={{ marginBottom: '16px' }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: 'var(--text-main)', marginBottom: '6px' }}>
                <UserIcon size={14} /> Họ và tên (theo hồ sơ nhà trường)
              </label>
              <input
                type="text"
                value={fullName}
                readOnly
                style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: 'var(--surface-muted)', border: '1px solid var(--border-color)', color: 'var(--text-main)', fontSize: '14px' }}
              />
            </div>

            <div className="form-group" style={{ marginBottom: '16px' }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: 'var(--text-main)', marginBottom: '6px' }}>
                <Phone size={14} /> Số điện thoại liên hệ
              </label>
              <input
                type="tel"
                value={phoneNumber}
                readOnly
                placeholder="0912345678"
                style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: 'var(--surface-muted)', border: '1px solid var(--border-color)', color: 'var(--text-main)', fontSize: '14px' }}
              />
            </div>

            <div className="form-group" style={{ marginBottom: '24px' }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: 'var(--text-main)', marginBottom: '6px' }}>
                <MapPin size={14} /> Địa chỉ liên hệ *
              </label>
              <input
                type="text"
                required
                maxLength={255}
                value={address}
                onChange={(e) => setAddress(e.target.value)}
                placeholder="Số nhà, đường, quận/huyện..."
                style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: 'var(--surface-muted)', border: '1px solid var(--border-color)', color: 'var(--text-main)', fontSize: '14px' }}
              />
            </div>

            <button
              type="submit"
              className="btn btn-primary"
              disabled={submitting}
              style={{ display: 'flex', alignItems: 'center', gap: '8px' }}
            >
              {submitting ? (
                <>
                  <RefreshCw size={16} className="spin" /> Đang lưu...
                </>
              ) : (
                <>
                  <Save size={16} /> Lưu thay đổi
                </>
              )}
            </button>
          </form>
        </div>

        {/* Change Password Card */}
        <div className="table-card" style={{ padding: '24px' }}>
          <h3 style={{ margin: '0 0 16px 0', fontSize: '16px', color: 'var(--text-main)', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <KeyRound size={18} color="var(--primary)" /> Đổi Mật Khẩu
          </h3>

          <form onSubmit={handlePasswordChange}>
            <div className="form-group" style={{ marginBottom: '16px' }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: 'var(--text-main)', marginBottom: '6px' }}>
                <Lock size={14} /> Mật khẩu hiện tại *
              </label>
              <div style={{ position: 'relative', display: 'flex', alignItems: 'center' }}>
                <input
                  type={showCurrentPwd ? 'text' : 'password'}
                  required
                  value={currentPwd}
                  onChange={(e) => setCurrentPwd(e.target.value)}
                  placeholder="Nhập mật khẩu đang sử dụng"
                  style={{ width: '100%', padding: '10px 40px 10px 14px', borderRadius: '8px', background: 'var(--surface-muted)', border: '1px solid var(--border-color)', color: 'var(--text-main)', fontSize: '14px' }}
                />
                <button
                  type="button"
                  onClick={() => setShowCurrentPwd(!showCurrentPwd)}
                  style={{ position: 'absolute', right: '10px', background: 'none', border: 'none', cursor: 'pointer', color: '#64748b', display: 'flex', alignItems: 'center', padding: '4px' }}
                  title={showCurrentPwd ? 'Ẩn' : 'Hiện'}
                >
                  {showCurrentPwd ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>

            <div className="form-group" style={{ marginBottom: '16px' }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: 'var(--text-main)', marginBottom: '6px' }}>
                <Lock size={14} /> Mật khẩu mới * (Tối thiểu 8 ký tự)
              </label>
              <div style={{ position: 'relative', display: 'flex', alignItems: 'center' }}>
                <input
                  type={showNewPwd ? 'text' : 'password'}
                  required
                  value={newPwd}
                  onChange={(e) => setNewPwd(e.target.value)}
                  placeholder="Nhập mật khẩu mới an toàn"
                  style={{ width: '100%', padding: '10px 40px 10px 14px', borderRadius: '8px', background: 'var(--surface-muted)', border: '1px solid var(--border-color)', color: 'var(--text-main)', fontSize: '14px' }}
                />
                <button
                  type="button"
                  onClick={() => setShowNewPwd(!showNewPwd)}
                  style={{ position: 'absolute', right: '10px', background: 'none', border: 'none', cursor: 'pointer', color: '#64748b', display: 'flex', alignItems: 'center', padding: '4px' }}
                  title={showNewPwd ? 'Ẩn' : 'Hiện'}
                >
                  {showNewPwd ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>

            <div className="form-group" style={{ marginBottom: '24px' }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: 'var(--text-main)', marginBottom: '6px' }}>
                <Lock size={14} /> Xác nhận mật khẩu mới *
              </label>
              <div style={{ position: 'relative', display: 'flex', alignItems: 'center' }}>
                <input
                  type={showConfirmPwd ? 'text' : 'password'}
                  required
                  value={confirmPwd}
                  onChange={(e) => setConfirmPwd(e.target.value)}
                  placeholder="Nhập lại mật khẩu mới"
                  style={{ width: '100%', padding: '10px 40px 10px 14px', borderRadius: '8px', background: 'var(--surface-muted)', border: '1px solid var(--border-color)', color: 'var(--text-main)', fontSize: '14px' }}
                />
                <button
                  type="button"
                  onClick={() => setShowConfirmPwd(!showConfirmPwd)}
                  style={{ position: 'absolute', right: '10px', background: 'none', border: 'none', cursor: 'pointer', color: '#64748b', display: 'flex', alignItems: 'center', padding: '4px' }}
                  title={showConfirmPwd ? 'Ẩn' : 'Hiện'}
                >
                  {showConfirmPwd ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>

            <button
              type="submit"
              className="btn btn-primary"
              disabled={submittingPwd}
              style={{ display: 'flex', alignItems: 'center', gap: '8px' }}
            >
              {submittingPwd ? (
                <>
                  <RefreshCw size={16} className="spin" /> Đang cập nhật...
                </>
              ) : (
                <>
                  <KeyRound size={16} /> Cập nhật mật khẩu
                </>
              )}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};
