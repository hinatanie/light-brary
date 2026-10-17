import React from 'react';
import { useAuth } from '../contexts/AuthContext';
import { User as UserIcon, LogOut, LogIn, Shield, Briefcase, UserCheck, KeyRound } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';

interface HeaderProps {
  portalTitle?: string;
}

export const Header: React.FC<HeaderProps> = ({ portalTitle }) => {
  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    const roles = (user?.roles || []).map((r) => r.toLowerCase());
    logout();
    if (roles.includes('admin')) {
      navigate('/login/admin');
    } else if (roles.includes('staff') || roles.includes('librarian')) {
      navigate('/login/librarian');
    } else {
      navigate('/login');
    }
  };

  const getRoleBadge = () => {
    if (!user || !user.roles || user.roles.length === 0) return null;
    if (user.roles.includes('Admin')) {
      return (
        <span className="header-role">
          <Shield size={12} /> Quản trị viên
        </span>
      );
    }
    if (user.roles.includes('Staff') || user.roles.includes('Librarian')) {
      return (
        <span className="header-role">
          <Briefcase size={12} /> Thủ thư
        </span>
      );
    }
    return (
      <span className="header-role">
        <UserCheck size={12} /> Bạn đọc
      </span>
    );
  };

  const getHomeLink = () => {
    if (!user || !user.roles) return '/login';
    if (user.roles.includes('Admin')) return '/admin';
    if (user.roles.includes('Staff') || user.roles.includes('Librarian')) return '/staff';
    return '/member';
  };

  return (
    <header className="app-header">
      <Link to={getHomeLink()} style={{ textDecoration: 'none' }} className="header-brand">
        <img className="brand-logo" src="/images/sgu-logo.webp" alt="Logo Trường Đại học Sài Gòn" width="42" height="42" />
        <div>
          <h1 className="brand-title">TRƯỜNG ĐẠI HỌC SÀI GÒN</h1>
          <p className="brand-subtitle">
            {portalTitle || 'Cổng thông tin thư viện'}
          </p>
        </div>
      </Link>

      <div className="header-actions">
        {isAuthenticated && user ? (
          <div className="user-profile-badge">
            <div className="user-avatar">
              <UserIcon size={16} />
            </div>
            <div className="user-details">
              <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                <span className="user-name">{user.fullName}</span>
                {getRoleBadge()}
              </div>
              <span className="user-email">{user.email || 'Cổng thông tin thư viện'}</span>
            </div>
            <Link
              to="/change-password"
              className="btn-logout"
              title="Đổi mật khẩu"
              style={{ display: 'flex', alignItems: 'center', textDecoration: 'none' }}
            >
              <KeyRound size={16} />
            </Link>
            <button className="btn-logout" onClick={handleLogout} title="Đăng xuất">
              <LogOut size={16} />
            </button>
          </div>
        ) : (
          <Link to="/login" className="btn btn-auth" style={{ textDecoration: 'none' }}>
            <LogIn size={16} /> Đăng Nhập
          </Link>
        )}
      </div>
    </header>
  );
};
