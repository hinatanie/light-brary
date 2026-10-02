import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { authApi } from '../api';
import { UserPlus, Lock, Mail, User, Phone, MapPin, AlertCircle, RefreshCw } from 'lucide-react';

export const RegisterPage: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [address, setAddress] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (password !== confirmPassword) {
      setError('Mật khẩu xác nhận không khớp!');
      return;
    }

    if (password.length < 6) {
      setError('Mật khẩu phải có ít nhất 6 ký tự!');
      return;
    }

    setLoading(true);

    try {
      const res = await authApi.register({
        fullName: fullName.trim(),
        email: email.trim(),
        password,
        phoneNumber: phoneNumber.trim() || undefined,
        address: address.trim() || undefined,
      });

      login(res);
      navigate('/member', { replace: true });
    } catch (err: any) {
      setError(err.message || 'Đăng ký không thành công. Vui lòng kiểm tra lại thông tin.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="table-card" style={{ padding: '32px 28px', borderRadius: '16px', background: '#1e293b', border: '1px solid #334155' }}>
      <div style={{ textAlign: 'center', marginBottom: '24px' }}>
        <h2 style={{ fontSize: '22px', fontWeight: 700, color: '#f8fafc', margin: '0 0 8px 0' }}>
          Đăng Ký Tài Khoản Độc Giả
        </h2>
        <p style={{ fontSize: '13px', color: '#94a3b8', margin: 0 }}>
          Trở thành thành viên để mượn sách và theo dõi phiếu mượn
        </p>
      </div>

      {error && (
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', padding: '12px 14px', borderRadius: '8px', background: 'rgba(239, 68, 68, 0.15)', border: '1px solid #ef4444', color: '#fca5a5', marginBottom: '20px', fontSize: '13px' }}>
          <AlertCircle size={16} style={{ flexShrink: 0 }} />
          <span>{error}</span>
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="form-group" style={{ marginBottom: '14px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: '#cbd5e1', marginBottom: '6px' }}>
            <User size={14} /> Họ và tên *
          </label>
          <input
            type="text"
            required
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            placeholder="Nguyễn Văn A"
            style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: '#0f172a', border: '1px solid #334155', color: '#f8fafc', fontSize: '14px' }}
          />
        </div>

        <div className="form-group" style={{ marginBottom: '14px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: '#cbd5e1', marginBottom: '6px' }}>
            <Phone size={14} /> Số điện thoại (dùng để đăng nhập) *
          </label>
          <input
            type="tel"
            required
            value={phoneNumber}
            onChange={(e) => setPhoneNumber(e.target.value)}
            placeholder="0912345678"
            style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: '#0f172a', border: '1px solid #334155', color: '#f8fafc', fontSize: '14px' }}
          />
        </div>

        <div className="form-group" style={{ marginBottom: '14px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: '#cbd5e1', marginBottom: '6px' }}>
            <Mail size={14} /> Địa chỉ Email *
          </label>
          <input
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="docgia@example.com"
            style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: '#0f172a', border: '1px solid #334155', color: '#f8fafc', fontSize: '14px' }}
          />
        </div>

        <div className="form-group" style={{ marginBottom: '14px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: '#cbd5e1', marginBottom: '6px' }}>
            <MapPin size={14} /> Địa chỉ
          </label>
          <input
            type="text"
            value={address}
            onChange={(e) => setAddress(e.target.value)}
            placeholder="Số nhà, đường, quận/huyện..."
            style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: '#0f172a', border: '1px solid #334155', color: '#f8fafc', fontSize: '14px' }}
          />
        </div>

        <div className="form-group" style={{ marginBottom: '14px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: '#cbd5e1', marginBottom: '6px' }}>
            <Lock size={14} /> Mật khẩu *
          </label>
          <input
            type="password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Ít nhất 6 ký tự"
            style={{ width: '100%', padding: '10px 14px', borderRadius: '8px', background: '#0f172a', border: '1px solid #334155', color: '#f8fafc', fontSize: '14px' }}
          />
        </div>

        <div className="form-group" style={{ marginBottom: '24px' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '13px', color: '#cbd5e1', marginBottom: '6px' }}>
            <Lock size={14} /> Xác nhận mật khẩu *
          </label>
          <input
            type="password"
            required
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            placeholder="Nhập lại mật khẩu"
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
              <RefreshCw size={16} className="spin" /> Đang đăng ký...
            </>
          ) : (
            <>
              <UserPlus size={16} /> Đăng ký tài khoản
            </>
          )}
        </button>
      </form>

      <div style={{ marginTop: '24px', textAlign: 'center', fontSize: '13px', color: '#94a3b8' }}>
        Đã có tài khoản?{' '}
        <Link to="/login" style={{ color: '#f59e0b', textDecoration: 'none', fontWeight: 600 }}>
          Đăng nhập ngay
        </Link>
      </div>
    </div>
  );
};
