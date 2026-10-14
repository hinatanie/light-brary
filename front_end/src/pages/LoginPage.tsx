import React, { useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { authApi } from '../api';
import {
  LogIn,
  Lock,
  Phone,
  AlertCircle,
  RefreshCw,
  Shield,
  BookOpen,
  ArrowRight,
  ArrowLeft,
} from 'lucide-react';

export type LoginRoleMode = 'admin' | 'librarian' | 'member';

interface LoginPageProps {
  roleMode?: LoginRoleMode;
}

export const LoginPage: React.FC<LoginPageProps> = ({ roleMode }) => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  // Determine role mode from prop or pathname
  const currentMode: LoginRoleMode = React.useMemo(() => {
    if (roleMode) return roleMode;
    const path = location.pathname.toLowerCase();
    if (path.startsWith('/login/admin')) return 'admin';
    if (path.startsWith('/login/librarian') || path.startsWith('/login/librian')) return 'librarian';
    return 'member';
  }, [roleMode, location.pathname]);

  const [phoneNumber, setPhoneNumber] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Configuration according to role mode
  const modeConfig = React.useMemo(() => {
    switch (currentMode) {
      case 'admin':
        return {
          badgeText: 'QUẢN TRỊ VIÊN',
          badgeColor: '#ef4444',
          badgeBg: 'rgba(239, 68, 68, 0.15)',
          badgeBorder: 'rgba(239, 68, 68, 0.3)',
          icon: <Shield size={28} color="#ef4444" />,
          title: 'Cổng Đăng Nhập Quản Trị Viên',
          subtitle: 'Khu vực quản trị cấp cao dành riêng cho Quản trị viên (Admin)',
          buttonColor: '#ef4444',
          portalTarget: '/admin',
        };
      case 'librarian':
        return {
          badgeText: 'THỦ THƯ & NHÂN VIÊN',
          badgeColor: '#0ea5e9',
          badgeBg: 'rgba(14, 165, 233, 0.15)',
          badgeBorder: 'rgba(14, 165, 233, 0.3)',
          icon: <BookOpen size={28} color="#0ea5e9" />,
          title: 'Cổng Đăng Nhập Thủ Thư',
          subtitle: 'Hệ thống nghiệp vụ dành cho Thủ thư & Nhân viên thư viện',
          buttonColor: '#0284c7',
          portalTarget: '/staff',
        };
      case 'member':
      default:
        return {
          badgeText: 'ĐỘC GIẢ',
          badgeColor: '#f59e0b',
          badgeBg: 'rgba(245, 158, 11, 0.15)',
          badgeBorder: 'rgba(245, 158, 11, 0.3)',
          icon: <LogIn size={28} color="#f59e0b" />,
          title: 'Đăng Nhập Độc Giả',
          subtitle: 'Tra cứu danh mục, theo dõi phiếu mượn và đặt mượn sách',
          buttonColor: '#f59e0b',
          portalTarget: '/member',
        };
    }
  }, [currentMode]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const res = await authApi.login({
        userName: phoneNumber.trim(),
        password,
      });

      // Successful login for this portal
      login(res);

      if (res.mustChangePassword) {
        navigate('/change-password', { replace: true });
        return;
      }

      // Điều hướng dựa vào đường dẫn đăng nhập (URL path), không phụ thuộc vào role của user
      const from = (location.state as any)?.from?.pathname;
      if (
        from &&
        !from.startsWith('/login') &&
        from !== '/unauthorized' &&
        from !== '/change-password' &&
        from.startsWith(modeConfig.portalTarget)
      ) {
        navigate(from, { replace: true });
        return;
      }

      // /login -> /member (trang người dùng), /login/admin -> /admin, /login/librarian -> /staff (web thủ thư)
      navigate(modeConfig.portalTarget, { replace: true });
    } catch (err: any) {
      if (err.status === 423 || err.code === 'ACCOUNT_LOCKED') {
        setError('Tài khoản đã bị tạm khóa do nhập sai mật khẩu quá 5 lần. Vui lòng thử lại sau 15 phút.');
      } else {
        setError(err.message || 'Đăng nhập không thành công. Vui lòng kiểm tra lại số điện thoại hoặc mật khẩu.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      className="table-card"
      style={{
        padding: '32px 28px',
        borderRadius: '16px',
        background: '#1e293b',
        border: '1px solid #334155',
        boxShadow: '0 10px 25px -5px rgba(0, 0, 0, 0.4)',
      }}
    >
      {/* Header with Role Badge */}
      <div style={{ textAlign: 'center', marginBottom: '24px' }}>
        <div
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            justifyContent: 'center',
            width: '56px',
            height: '56px',
            borderRadius: '14px',
            background: modeConfig.badgeBg,
            border: `1px solid ${modeConfig.badgeBorder}`,
            marginBottom: '14px',
          }}
        >
          {modeConfig.icon}
        </div>

        <div>
          <span
            style={{
              display: 'inline-block',
              fontSize: '11px',
              fontWeight: 700,
              letterSpacing: '0.08em',
              color: modeConfig.badgeColor,
              background: modeConfig.badgeBg,
              border: `1px solid ${modeConfig.badgeBorder}`,
              padding: '3px 10px',
              borderRadius: '20px',
              marginBottom: '10px',
            }}
          >
            {modeConfig.badgeText}
          </span>
        </div>

        <h2 style={{ fontSize: '22px', fontWeight: 700, color: '#f8fafc', margin: '0 0 8px 0' }}>
          {modeConfig.title}
        </h2>
        <p style={{ fontSize: '13px', color: '#94a3b8', margin: 0, lineHeight: 1.5 }}>
          {modeConfig.subtitle}
        </p>
      </div>

      {error && (
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '10px',
            padding: '12px 14px',
            borderRadius: '8px',
            background: 'rgba(239, 68, 68, 0.15)',
            border: '1px solid #ef4444',
            color: '#fca5a5',
            marginBottom: '20px',
            fontSize: '13px',
            lineHeight: 1.4,
          }}
        >
          <AlertCircle size={18} style={{ flexShrink: 0, color: '#ef4444' }} />
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
            placeholder="Số điện thoại"
            style={{
              width: '100%',
              padding: '10px 14px',
              borderRadius: '8px',
              background: '#0f172a',
              border: '1px solid #334155',
              color: '#f8fafc',
              fontSize: '14px',
              outline: 'none',
              transition: 'border-color 0.2s',
            }}
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
            style={{
              width: '100%',
              padding: '10px 14px',
              borderRadius: '8px',
              background: '#0f172a',
              border: '1px solid #334155',
              color: '#f8fafc',
              fontSize: '14px',
              outline: 'none',
              transition: 'border-color 0.2s',
            }}
          />
        </div>

        <button
          type="submit"
          className="btn"
          disabled={loading}
          style={{
            width: '100%',
            padding: '12px',
            justifyContent: 'center',
            fontSize: '14px',
            fontWeight: 600,
            background: modeConfig.buttonColor,
            color: '#ffffff',
            border: 'none',
            borderRadius: '8px',
            cursor: loading ? 'not-allowed' : 'pointer',
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
            transition: 'opacity 0.2s',
          }}
        >
          {loading ? (
            <>
              <RefreshCw size={16} className="spin" /> Đang xác thực...
            </>
          ) : (
            <>
              <LogIn size={16} /> Đăng nhập
            </>
          )}
        </button>
      </form>

      {/* Footer Navigation Links */}
      <div style={{ marginTop: '24px', paddingTop: '16px', borderTop: '1px solid #334155', fontSize: '13px' }}>
        {currentMode === 'member' ? (
          <div>
            <div style={{ textAlign: 'center', marginBottom: '16px', color: '#94a3b8' }}>
              Chưa có tài khoản độc giả?{' '}
              <Link to="/register" style={{ color: '#f59e0b', textDecoration: 'none', fontWeight: 600 }}>
                Đăng ký độc giả mới
              </Link>
            </div>

            {/* Portal Switcher for Staff/Admin */}
            <div
              style={{
                background: '#0f172a',
                padding: '10px 14px',
                borderRadius: '8px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                color: '#64748b',
                fontSize: '12px',
              }}
            >
              <span>Cổng cán bộ thư viện:</span>
              <div style={{ display: 'flex', gap: '10px' }}>
                <Link
                  to="/login/librarian"
                  style={{ color: '#38bdf8', textDecoration: 'none', fontWeight: 600, display: 'inline-flex', alignItems: 'center', gap: '3px' }}
                >
                  Thủ thư <ArrowRight size={12} />
                </Link>
                <span style={{ color: '#334155' }}>|</span>
                <Link
                  to="/login/admin"
                  style={{ color: '#f87171', textDecoration: 'none', fontWeight: 600, display: 'inline-flex', alignItems: 'center', gap: '3px' }}
                >
                  Admin <ArrowRight size={12} />
                </Link>
              </div>
            </div>
          </div>
        ) : currentMode === 'admin' ? (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', textAlign: 'center' }}>
            <Link
              to="/login/librarian"
              style={{
                color: '#38bdf8',
                textDecoration: 'none',
                display: 'inline-flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '6px',
                padding: '8px',
                borderRadius: '6px',
                background: 'rgba(14, 165, 233, 0.08)',
              }}
            >
              <BookOpen size={14} /> Chuyển sang Cổng Đăng Nhập Thủ Thư
            </Link>

            <Link
              to="/login"
              style={{
                color: '#94a3b8',
                textDecoration: 'none',
                display: 'inline-flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '6px',
              }}
            >
              <ArrowLeft size={14} /> Quay lại cổng Độc Giả
            </Link>
          </div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', textAlign: 'center' }}>
            <Link
              to="/login/admin"
              style={{
                color: '#f87171',
                textDecoration: 'none',
                display: 'inline-flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '6px',
                padding: '8px',
                borderRadius: '6px',
                background: 'rgba(239, 68, 68, 0.08)',
              }}
            >
              <Shield size={14} /> Chuyển sang Cổng Đăng Nhập Quản Trị Viên
            </Link>

            <Link
              to="/login"
              style={{
                color: '#94a3b8',
                textDecoration: 'none',
                display: 'inline-flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '6px',
              }}
            >
              <ArrowLeft size={14} /> Quay lại cổng Độc Giả
            </Link>
          </div>
        )}
      </div>
    </div>
  );
};
