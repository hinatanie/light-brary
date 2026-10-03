import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { authApi } from '../api';
import { KeyRound, Lock, AlertCircle, CheckCircle2, RefreshCw } from 'lucide-react';

export const ChangePasswordPage: React.FC = () => {
  const { user, updateUserSession } = useAuth();
  const navigate = useNavigate();

  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

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

    if (newPassword.length < 10) {
      setError('Mật khẩu mới phải có tối thiểu 10 ký tự!');
      return;
    }

    if (!/[A-Z]/.test(newPassword)) {
      setError('Mật khẩu mới phải chứa ít nhất một chữ in hoa (A-Z)!');
      return;
    }

    if (!/[a-z]/.test(newPassword)) {
      setError('Mật khẩu mới phải chứa ít nhất một chữ in thường (a-z)!');
      return;
    }

    if (!/[0-9]/.test(newPassword)) {
      setError('Mật khẩu mới phải chứa ít nhất một chữ số (0-9)!');
      return;
    }

    setLoading(true);

    try {
      await authApi.changePassword({
        currentPassword,
        newPassword,
      });

      setSuccess(true);
      updateUserSession({ mustChangePassword: false });

      setTimeout(() => {
        if (user?.roles.some((r) => r.toLowerCase() === 'admin')) {
          navigate('/admin', { replace: true });
        } else if (user?.roles.some((r) => r.toLowerCase() === 'staff')) {
          navigate('/staff', { replace: true });
        } else {
          navigate('/member', { replace: true });
        }
      }, 1500);
    } catch (err: any) {
      setError(err.message || 'Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu hiện tại.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="table-card" style={{ maxWidth: '480px', margin: '40px auto', padding: '32px 28px', borderRadius: '16px', background: '#1e293b', border: '1px solid #334155' }}>
      <div style={{ textAlign: 'center', marginBottom: '24px' }}>
        <div style={{ display: 'inline-flex', padding: '12px', borderRadius: '50%', background: 'rgba(245, 158, 11, 0.15)', color: '#f59e0b', marginBottom: '12px' }}>
          <KeyRound size={28} />
        </div>
        <h2 style={{ fontSize: '22px', fontWeight: 700, color: '#f8fafc', margin: '0 0 8px 0' }}>
          Đổi Mật Khẩu
        </h2>
        {user?.mustChangePassword ? (
          <p style={{ fontSize: '13px', color: '#f59e0b', margin: 0, fontWeight: 500 }}>
            Đây là lần đăng nhập đầu tiên hoặc tài khoản cần cập nhật mật khẩu mới trước khi tiếp tục sử dụng hệ thống.
          </p>
        ) : (
          <p style={{ fontSize: '13px', color: '#94a3b8', margin: 0 }}>
            Cập nhật mật khẩu định kỳ để bảo vệ an toàn cho tài khoản của bạn.
          </p>
        )}
      </div>

      {error && (
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', padding: '12px 14px', borderRadius: '8px', background: 'rgba(239, 68, 68, 0.15)', border: '1px solid #ef4444', color: '#fca5a5', marginBottom: '20px', fontSize: '13px' }}>
          <AlertCircle size={16} style={{ flexShrink: 0 }} />
          <span>{error}</span>
        </div>
      )}

      {success && (
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', padding: '12px 14px', borderRadius: '8px', background: 'rgba(16, 185, 129, 0.15)', border: '1px solid #10b981', color: '#6ee7b7', marginBottom: '20px', fontSize: '13px' }}>
          <CheckCircle2 size={16} style={{ flexShrink: 0 }} />
          <span>Đổi mật khẩu thành công! Đang chuyển hướng...</span>
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="form-group" style={{ marginBottom: '16px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: '#cbd5e1', marginBottom: '6px' }}>
            <Lock size={14} /> Mật khẩu hiện tại *
          </label>
          <input
            type="password"
            required
            value={currentPassword}
            onChange={(e) => setCurrentPassword(e.target.value)}
            placeholder="Nhập mật khẩu đang sử dụng"
            style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: '#0f172a', border: '1px solid #334155', color: '#f8fafc', fontSize: '14px' }}
          />
        </div>

        <div className="form-group" style={{ marginBottom: '16px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: '#cbd5e1', marginBottom: '6px' }}>
            <Lock size={14} /> Mật khẩu mới * (Tối thiểu 10 ký tự, có chữ hoa, thường, số)
          </label>
          <input
            type="password"
            required
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            placeholder="Nhập mật khẩu mới an toàn"
            style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: '#0f172a', border: '1px solid #334155', color: '#f8fafc', fontSize: '14px' }}
          />
        </div>

        <div className="form-group" style={{ marginBottom: '24px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: '#cbd5e1', marginBottom: '6px' }}>
            <Lock size={14} /> Xác nhận mật khẩu mới *
          </label>
          <input
            type="password"
            required
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            placeholder="Nhập lại mật khẩu mới"
            style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: '#0f172a', border: '1px solid #334155', color: '#f8fafc', fontSize: '14px' }}
          />
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
