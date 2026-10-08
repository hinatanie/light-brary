import React, { useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { authApi } from '../api';
import { LogIn, Lock, Phone, AlertCircle, RefreshCw } from 'lucide-react';

export const LoginPage: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [phoneNumber, setPhoneNumber] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const res = await authApi.login({
        phoneNumber: phoneNumber.trim(),
        password,
      });

      login(res);

      if (res.mustChangePassword) {
        navigate('/change-password', { replace: true });
        return;
      }

      // Redirect based on role or state.from
      const from = (location.state as any)?.from?.pathname;
      if (from && from !== '/login' && from !== '/unauthorized' && from !== '/change-password') {
        navigate(from, { replace: true });
        return;
      }

      if (res.roles && res.roles.some((r) => r.toLowerCase() === 'admin')) {
        navigate('/admin', { replace: true });
      } else if (res.roles && res.roles.some((r) => r.toLowerCase() === 'staff' || r.toLowerCase() === 'librarian')) {
        navigate('/staff', { replace: true });
      } else {
        navigate('/member', { replace: true });
      }
    } catch (err: any) {
      if (err.status === 423 || err.code === 'ACCOUNT_LOCKED') {
        setError('Tài khoản đã bị tạm khóa do nhập sai mật khẩu quá nhiều lần (5 lần). Vui lòng thử lại sau 15 phút.');
      } else {
        setError(err.message || 'Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="table-card" style={{ padding: '32px 28px', borderRadius: '16px', background: '#1e293b', border: '1px solid #334155' }}>
      <div style={{ textAlign: 'center', marginBottom: '24px' }}>
        <h2 style={{ fontSize: '22px', fontWeight: 700, color: '#f8fafc', margin: '0 0 8px 0' }}>
          Đăng Nhập Hệ Thống
        </h2>
        <p style={{ fontSize: '13px', color: '#94a3b8', margin: 0 }}>
          Sử dụng số điện thoại đã đăng ký để truy cập
        </p>
      </div>

      {error && (
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', padding: '12px 14px', borderRadius: '8px', background: 'rgba(239, 68, 68, 0.15)', border: '1px solid #ef4444', color: '#fca5a5', marginBottom: '20px', fontSize: '13px' }}>
          <AlertCircle size={16} style={{ flexShrink: 0 }} />
          <span>{error}</span>
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="form-group" style={{ marginBottom: '16px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: '#cbd5e1', marginBottom: '6px' }}>
            <Phone size={14} /> Số điện thoại *
          </label>
          <input
            type="tel"
            required
            value={phoneNumber}
            onChange={(e) => setPhoneNumber(e.target.value)}
            placeholder="0916114536"
            style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: '#0f172a', border: '1px solid #334155', color: '#f8fafc', fontSize: '14px' }}
          />
        </div>

        <div className="form-group" style={{ marginBottom: '24px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: '#cbd5e1', marginBottom: '6px' }}>
            <Lock size={14} /> Mật khẩu *
          </label>
          <input
            type="password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="••••••••"
            style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: '#0f172a', border: '1px solid #334155', color: '#f8fafc', fontSize: '14px' }}
          />
        </div>

        <button
          type="submit"
          className="btn btn-primary"
          disabled={loading}
          style={{ width: '100%', padding: '12px', justifyContent: 'center', fontSize: '14px', fontWeight: 600 }}
        >
          {loading ? (
            <>
              <RefreshCw size={16} className="spin" /> Đang đăng nhập...
            </>
          ) : (
            <>
              <LogIn size={16} /> Đăng nhập
            </>
          )}
        </button>
      </form>

      <div style={{ marginTop: '24px', textAlign: 'center', fontSize: '13px', color: '#94a3b8' }}>
        Chưa có tài khoản?{' '}
        <Link to="/register" style={{ color: '#f59e0b', textDecoration: 'none', fontWeight: 600 }}>
          Đăng ký độc giả mới
        </Link>
      </div>
    </div>
  );
};
