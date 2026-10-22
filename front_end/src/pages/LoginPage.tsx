import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { authApi } from '../api';
import { LogIn, AlertCircle, RefreshCw, Eye, EyeOff } from 'lucide-react';

export type LoginRoleMode = 'admin' | 'librarian' | 'member';

interface LoginPageProps {
  roleMode?: LoginRoleMode;
}

export const LoginPage: React.FC<LoginPageProps> = ({ roleMode }) => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const currentMode: LoginRoleMode = roleMode || (location.pathname.startsWith('/login/admin')
    ? 'admin' : location.pathname.startsWith('/login/librarian') ? 'librarian' : 'member');
  const [userName, setUserName] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const modeConfig = {
    member: { title: 'Đăng nhập sinh viên', portalTarget: '/member' },
    librarian: { title: 'Đăng nhập thủ thư', portalTarget: '/staff' },
    admin: { title: 'Đăng nhập quản trị viên', portalTarget: '/admin' },
  }[currentMode];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const res = await authApi.login({
        userName: userName.trim(),
        password,
      });

      // Successful login for this portal
      login(res);

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
        setError(err.message || 'Đăng nhập không thành công. Vui lòng kiểm tra lại tên đăng nhập hoặc mật khẩu.');
      }
    } finally {
      setLoading(false);
    }
  };


  return (
    <section className="login-panel">
      <h2 className="login-heading"><LogIn size={18} />{modeConfig.title}</h2>
      <p className="login-description">Sử dụng tài khoản được nhà trường cấp.</p>

      {error && <div className="login-error" role="alert"><AlertCircle size={17} /><span>{error}</span></div>}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="login-username">{currentMode === 'member' ? 'Mã số sinh viên' : 'Tên đăng nhập'}</label>
          <input id="login-username" type="text" required maxLength={256} autoComplete="username"
            value={userName} onChange={(e) => setUserName(e.target.value)}
            placeholder={currentMode === 'member' ? 'Nhập mã số sinh viên' : 'Nhập tên đăng nhập'} />
        </div>
        <div className="form-group">
          <label htmlFor="login-password">Mật khẩu</label>
          <div className="login-password-field">
            <input id="login-password" type={showPassword ? 'text' : 'password'} required autoComplete="current-password"
              value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Nhập mật khẩu" />
            <button type="button" className="password-visibility" onClick={() => setShowPassword(!showPassword)}
              aria-label={showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'} aria-pressed={showPassword}>
              {showPassword ? <EyeOff size={17} /> : <Eye size={17} />}
            </button>
          </div>
        </div>
        <button type="submit" className="btn btn-primary login-submit" disabled={loading}>
          {loading ? <><RefreshCw size={16} className="spin" /> Đang xác thực...</> : <><LogIn size={16} /> Đăng nhập</>}
        </button>
      </form>

      <p className="login-help">Quên mật khẩu? Vui lòng liên hệ cán bộ thư viện để được hỗ trợ.</p>
    </section>
  );
};
