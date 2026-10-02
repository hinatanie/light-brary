import React, { useState } from 'react';
import { authApi } from '../api';
import type { AuthResponse } from '../types';
import { X, LogIn, UserPlus, Lock, Mail, User, Phone, MapPin, Info } from 'lucide-react';

interface AuthModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (auth: AuthResponse) => void;
  onNotify: (text: string, type?: 'success' | 'error') => void;
}

export const AuthModal: React.FC<AuthModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  onNotify,
}) => {
  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [loading, setLoading] = useState(false);

  // Form states
  const [loginPhone, setLoginPhone] = useState('0916114536');
  const [password, setPassword] = useState('Asdf1234!');
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [registerPhone, setRegisterPhone] = useState('');
  const [address, setAddress] = useState('');

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    try {
      if (mode === 'login') {
        const res = await authApi.login({
          phoneNumber: loginPhone.trim(),
          password,
        });
        localStorage.setItem('lightbrary_token', res.token);
        localStorage.setItem('lightbrary_user', JSON.stringify(res));
        onSuccess(res);
        onNotify(`Chào mừng ${res.fullName}! Đăng nhập thành công bằng SĐT ${loginPhone}.`);
        onClose();
      } else {
        const res = await authApi.register({
          fullName: fullName.trim(),
          email: email.trim(),
          password,
          phoneNumber: registerPhone.trim(),
          address: address.trim() || undefined,
        });
        localStorage.setItem('lightbrary_token', res.token);
        localStorage.setItem('lightbrary_user', JSON.stringify(res));
        onSuccess(res);
        onNotify(`Tạo tài khoản thành công cho ${res.fullName}! Bạn có thể đăng nhập bằng SĐT này.`);
        onClose();
      }
    } catch (err: any) {
      onNotify(err.message || 'Thao tác không thành công', 'error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-box" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            {mode === 'login' ? <LogIn size={20} color="#f59e0b" /> : <UserPlus size={20} color="#f59e0b" />}
            <h3>{mode === 'login' ? 'Đăng Nhập Bằng Số Điện Thoại' : 'Đăng Ký Tài Khoản Thư Viện'}</h3>
          </div>
          <button className="btn-icon" onClick={onClose}><X size={18} /></button>
        </div>

        <div className="auth-tab-buttons">
          <button
            type="button"
            className={`tab-btn ${mode === 'login' ? 'active' : ''}`}
            onClick={() => setMode('login')}
          >
            Đăng nhập bằng SĐT
          </button>
          <button
            type="button"
            className={`tab-btn ${mode === 'register' ? 'active' : ''}`}
            onClick={() => setMode('register')}
          >
            Đăng ký độc giả mới
          </button>
        </div>

        <form onSubmit={handleSubmit} className="form-content">
          {mode === 'login' ? (
            <>
              <div className="login-tip-banner">
                <Info size={16} style={{ flexShrink: 0 }} />
                <span>
                  Admin mặc định: SĐT <strong>0916114536</strong> | Mật khẩu: <strong>Asdf1234!</strong>
                </span>
              </div>

              <div className="form-group">
                <label><Phone size={14} /> Số điện thoại đăng nhập *</label>
                <input
                  type="tel"
                  required
                  value={loginPhone}
                  onChange={(e) => setLoginPhone(e.target.value)}
                  placeholder="0916114536"
                />
              </div>

              <div className="form-group">
                <label><Lock size={14} /> Mật khẩu *</label>
                <input
                  type="password"
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Nhập mật khẩu..."
                />
              </div>
            </>
          ) : (
            <>
              <div className="form-group">
                <label><User size={14} /> Họ và tên *</label>
                <input
                  type="text"
                  required
                  value={fullName}
                  onChange={(e) => setFullName(e.target.value)}
                  placeholder="Nguyễn Văn A"
                />
              </div>

              <div className="form-group">
                <label><Phone size={14} /> Số điện thoại (Dùng để đăng nhập) *</label>
                <input
                  type="tel"
                  required
                  value={registerPhone}
                  onChange={(e) => setRegisterPhone(e.target.value)}
                  placeholder="0912345678"
                />
              </div>

              <div className="form-group">
                <label><Mail size={14} /> Địa chỉ Email *</label>
                <input
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="docgia@lightbrary.vn"
                />
              </div>

              <div className="form-group">
                <label><Lock size={14} /> Mật khẩu *</label>
                <input
                  type="password"
                  required
                  minLength={6}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Tối thiểu 6 ký tự"
                />
              </div>

              <div className="form-group">
                <label><MapPin size={14} /> Địa chỉ</label>
                <input
                  type="text"
                  value={address}
                  onChange={(e) => setAddress(e.target.value)}
                  placeholder="Hà Nội, Việt Nam"
                />
              </div>
            </>
          )}

          <div className="modal-actions" style={{ marginTop: '20px' }}>
            <button type="button" className="btn btn-secondary" onClick={onClose}>
              Hủy
            </button>
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? 'Đang xử lý...' : mode === 'login' ? 'Đăng Nhập' : 'Tạo Tài Khoản'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
