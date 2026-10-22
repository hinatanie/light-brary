import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { authApi } from '../api';
import { KeyRound, Lock, AlertCircle, CheckCircle2, RefreshCw, Eye, EyeOff, ArrowLeft } from 'lucide-react';

export const ChangePasswordPage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();

  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showCurrent, setShowCurrent] = useState(false);
  const [showNew, setShowNew] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const getDashboardPath = () => {
    if (user?.roles.some((r) => r.toLowerCase() === 'admin')) return '/admin';
    if (user?.roles.some((r) => r.toLowerCase() === 'staff' || r.toLowerCase() === 'librarian')) return '/staff';
    return '/member';
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (newPassword !== confirmPassword) {
      setError('Mật khẩu mới và xác nhận mật khẩu không khớp!');
      return;
    }

    if (newPassword === currentPassword) {
      setError('Mật khẩu mới không được trùng với mật khẩu hiện tại!');
      return;
    }

    if (newPassword.length < 6) {
      setError('Mật khẩu mới phải có tối thiểu 6 ký tự!');
      return;
    }

    setLoading(true);

    try {
      await authApi.changePassword({
        currentPassword,
        newPassword,
      });

      setSuccess(true);

      setTimeout(() => {
        navigate(getDashboardPath(), { replace: true });
      }, 1500);
    } catch (err: any) {
      setError(err.message || 'Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu hiện tại.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="table-card" style={{ maxWidth: '480px', margin: '40px auto', padding: '32px 28px', borderRadius: '16px', background: 'var(--card-bg)', border: '1px solid var(--border-color)' }}>
      <div style={{ marginBottom: '16px' }}>
        <Link
          to={getDashboardPath()}
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: '6px',
            fontSize: '13px',
            color: 'var(--text-muted)',
            textDecoration: 'none',
            transition: 'color 0.15s',
          }}
        >
          <ArrowLeft size={16} /> Quay lại trang chính
        </Link>
      </div>

      <div style={{ textAlign: 'center', marginBottom: '24px' }}>
        <div style={{ display: 'inline-flex', padding: '12px', borderRadius: '50%', background: 'var(--primary-soft)', color: 'var(--primary)', marginBottom: '12px' }}>
          <KeyRound size={28} />
        </div>
        <h2 style={{ fontSize: '22px', fontWeight: 700, color: 'var(--text-main)', margin: '0 0 8px 0' }}>
          Đổi Mật Khẩu
        </h2>
        <p style={{ fontSize: '13px', color: 'var(--text-muted)', margin: 0 }}>
          Cập nhật mật khẩu định kỳ để bảo vệ an toàn cho tài khoản của bạn.
        </p>
      </div>

      {error && (
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', padding: '12px 14px', borderRadius: '8px', background: 'rgba(239, 68, 68, 0.15)', border: '1px solid #ef4444', color: '#b91c1c', marginBottom: '20px', fontSize: '13px' }}>
          <AlertCircle size={16} style={{ flexShrink: 0 }} />
          <span>{error}</span>
        </div>
      )}

      {success && (
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', padding: '12px 14px', borderRadius: '8px', background: 'rgba(16, 185, 129, 0.15)', border: '1px solid #10b981', color: '#047857', marginBottom: '20px', fontSize: '13px' }}>
          <CheckCircle2 size={16} style={{ flexShrink: 0 }} />
          <span>Đổi mật khẩu thành công! Đang chuyển hướng...</span>
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="form-group" style={{ marginBottom: '16px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: 'var(--text-main)', marginBottom: '6px' }}>
            <Lock size={14} /> Mật khẩu hiện tại *
          </label>
          <div style={{ position: 'relative', display: 'flex', alignItems: 'center' }}>
            <input
              type={showCurrent ? 'text' : 'password'}
              required
              value={currentPassword}
              onChange={(e) => setCurrentPassword(e.target.value)}
              placeholder="Nhập mật khẩu đang sử dụng"
              style={{ width: '100%', padding: '10px 40px 10px 14px', borderRadius: '8px', background: 'var(--surface-muted)', border: '1px solid var(--border-color)', color: 'var(--text-main)', fontSize: '14px' }}
            />
            <button
              type="button"
              onClick={() => setShowCurrent(!showCurrent)}
              style={{ position: 'absolute', right: '10px', background: 'none', border: 'none', cursor: 'pointer', color: '#64748b', display: 'flex', alignItems: 'center', padding: '4px' }}
              title={showCurrent ? 'Ẩn' : 'Hiện'}
            >
              {showCurrent ? <EyeOff size={16} /> : <Eye size={16} />}
            </button>
          </div>
        </div>

        <div className="form-group" style={{ marginBottom: '16px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: 'var(--text-main)', marginBottom: '6px' }}>
            <Lock size={14} /> Mật khẩu mới * (Tối thiểu 6 ký tự)
          </label>
          <div style={{ position: 'relative', display: 'flex', alignItems: 'center' }}>
            <input
              type={showNew ? 'text' : 'password'}
              required
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              placeholder="Nhập mật khẩu mới an toàn"
              style={{ width: '100%', padding: '10px 40px 10px 14px', borderRadius: '8px', background: 'var(--surface-muted)', border: '1px solid var(--border-color)', color: 'var(--text-main)', fontSize: '14px' }}
            />
            <button
              type="button"
              onClick={() => setShowNew(!showNew)}
              style={{ position: 'absolute', right: '10px', background: 'none', border: 'none', cursor: 'pointer', color: '#64748b', display: 'flex', alignItems: 'center', padding: '4px' }}
              title={showNew ? 'Ẩn' : 'Hiện'}
            >
              {showNew ? <EyeOff size={16} /> : <Eye size={16} />}
            </button>
          </div>
        </div>

        <div className="form-group" style={{ marginBottom: '24px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: 'var(--text-main)', marginBottom: '6px' }}>
            <Lock size={14} /> Xác nhận mật khẩu mới *
          </label>
          <div style={{ position: 'relative', display: 'flex', alignItems: 'center' }}>
            <input
              type={showConfirm ? 'text' : 'password'}
              required
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              placeholder="Nhập lại mật khẩu mới"
              style={{ width: '100%', padding: '10px 40px 10px 14px', borderRadius: '8px', background: 'var(--surface-muted)', border: '1px solid var(--border-color)', color: 'var(--text-main)', fontSize: '14px' }}
            />
            <button
              type="button"
              onClick={() => setShowConfirm(!showConfirm)}
              style={{ position: 'absolute', right: '10px', background: 'none', border: 'none', cursor: 'pointer', color: '#64748b', display: 'flex', alignItems: 'center', padding: '4px' }}
              title={showConfirm ? 'Ẩn' : 'Hiện'}
            >
              {showConfirm ? <EyeOff size={16} /> : <Eye size={16} />}
            </button>
          </div>
        </div>

        <button
          type="submit"
          className="btn btn-primary"
          disabled={loading || success}
          style={{ width: '100%', padding: '12px', justifyContent: 'center', fontSize: '14px', fontWeight: 600 }}
        >
          {loading ? (
            <>
              <RefreshCw size={16} className="spin" /> Đang cập nhật mật khẩu...
            </>
          ) : (
            'Xác Nhận Đổi Mật Khẩu'
          )}
        </button>
      </form>
    </div>
  );
};
