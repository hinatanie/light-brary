import React, { useState, useEffect } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { useToast } from '../../contexts/ToastContext';
import { usersApi, authApi } from '../../api';
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
  Calendar
} from 'lucide-react';

export const MemberProfilePage: React.FC = () => {
  const { user, updateUserSession } = useAuth();
  const { showToast } = useToast();

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [profileData, setProfileData] = useState<User | null>(null);

  // Form states
  const [fullName, setFullName] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [address, setAddress] = useState('');

  const loadProfile = async () => {
    if (!user) return;
    setLoading(true);
    try {
      // First try to get full details from usersApi, or authApi.getMe()
      try {
        const fullUser = await usersApi.getById(user.userId);
        setProfileData(fullUser);
        setFullName(fullUser.fullName);
        setPhoneNumber(fullUser.phoneNumber || '');
        setAddress(fullUser.address || '');
      } catch {
        const me: MeResponse = await authApi.getMe();
        setFullName(me.fullName);
        setPhoneNumber(me.phoneNumber || '');
        setProfileData({
          id: me.userId,
          fullName: me.fullName,
          email: me.email,
          phoneNumber: me.phoneNumber,
          address: '',
          createdAt: new Date().toISOString(),
          roles: me.roles,
        });
      }
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

    const cleanPhone = phoneNumber.trim();
    if (!cleanPhone) {
      showToast('Số điện thoại là bắt buộc!', 'error');
      return;
    }

    if (!/^(0|\+84)[0-9]{9}$/.test(cleanPhone)) {
      showToast('Số điện thoại không hợp lệ! Vui lòng nhập số điện thoại Việt Nam gồm 10 chữ số.', 'error');
      return;
    }

    setSubmitting(true);
    try {
      const updated = await usersApi.update(user.userId, {
        fullName: fullName.trim(),
        phoneNumber: cleanPhone,
        address: address.trim() || undefined,
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
          <h2>👤 Hồ Sơ Bạn Đọc Cá Nhân</h2>
          <p className="section-desc">
            Xem và cập nhật thông tin liên lạc phục vụ công tác mượn trả và liên hệ thông báo
          </p>
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '24px' }}>
        {/* Profile Info Card */}
        <div className="table-card" style={{ padding: '24px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '16px', marginBottom: '20px' }}>
            <div
              style={{
                width: '64px',
                height: '64px',
                borderRadius: '50%',
                background: '#f59e0b',
                color: '#0f172a',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <UserIcon size={32} />
            </div>
            <div>
              <h3 style={{ margin: '0 0 6px 0', fontSize: '18px', color: '#f8fafc' }}>
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

          <div style={{ display: 'flex', flexDirection: 'column', gap: '14px', fontSize: '13px', borderTop: '1px solid #334155', paddingTop: '16px' }}>
            <div>
              <span style={{ color: '#94a3b8', display: 'block', marginBottom: '2px' }}>Mã tài khoản:</span>
              <code>{profileData?.id || user?.userId}</code>
            </div>
            <div>
              <span style={{ color: '#94a3b8', display: 'block', marginBottom: '2px' }}>Email đăng ký:</span>
              <span style={{ display: 'flex', alignItems: 'center', gap: '6px', color: '#f8fafc' }}>
                <Mail size={14} color="#64748b" /> {profileData?.email || user?.email}
              </span>
            </div>
            {profileData?.createdAt && (
              <div>
                <span style={{ color: '#94a3b8', display: 'block', marginBottom: '2px' }}>Ngày tạo tài khoản:</span>
                <span style={{ display: 'flex', alignItems: 'center', gap: '6px', color: '#f8fafc' }}>
                  <Calendar size={14} color="#64748b" />{' '}
                  {new Date(profileData.createdAt).toLocaleDateString('vi-VN')}
                </span>
              </div>
            )}
          </div>
        </div>

        {/* Edit Form Card */}
        <div className="table-card" style={{ padding: '24px' }}>
          <h3 style={{ margin: '0 0 16px 0', fontSize: '16px', color: '#f8fafc' }}>
            ✏️ Cập Nhật Thông Tin Cá Nhân
          </h3>

          <form onSubmit={handleSubmit}>
            <div className="form-group" style={{ marginBottom: '16px' }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: '#cbd5e1', marginBottom: '6px' }}>
                <UserIcon size={14} /> Họ và tên *
              </label>
              <input
                type="text"
                required
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: '#0f172a', border: '1px solid #334155', color: '#f8fafc', fontSize: '14px' }}
              />
            </div>

            <div className="form-group" style={{ marginBottom: '16px' }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: '#cbd5e1', marginBottom: '6px' }}>
                <Phone size={14} /> Số điện thoại (dùng để đăng nhập)
              </label>
              <input
                type="tel"
                value={phoneNumber}
                onChange={(e) => setPhoneNumber(e.target.value)}
                placeholder="0912345678"
                style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: '#0f172a', border: '1px solid #334155', color: '#f8fafc', fontSize: '14px' }}
              />
            </div>

            <div className="form-group" style={{ marginBottom: '24px' }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: '#cbd5e1', marginBottom: '6px' }}>
                <MapPin size={14} /> Địa chỉ liên hệ
              </label>
              <input
                type="text"
                value={address}
                onChange={(e) => setAddress(e.target.value)}
                placeholder="Số nhà, đường, quận/huyện..."
                style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: '#0f172a', border: '1px solid #334155', color: '#f8fafc', fontSize: '14px' }}
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
      </div>
    </div>
  );
};
