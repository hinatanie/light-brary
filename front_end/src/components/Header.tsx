import React from 'react';
import { useAuth } from '../contexts/AuthContext';
import { BookOpen, User as UserIcon, LogOut, LogIn, Shield, Briefcase, UserCheck } from 'lucide-react';
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
        <span className="badge badge-admin" style={{ display: 'inline-flex', alignItems: 'center', gap: '4px', background: '#dc2626', color: '#fff', padding: '2px 8px', borderRadius: '12px', fontSize: '11px', fontWeight: 600 }}>
          <Shield size={12} /> Admin
        </span>
      );
    }
    if (user.roles.includes('Staff')) {
      return (
        <span className="badge badge-staff" style={{ display: 'inline-flex', alignItems: 'center', gap: '4px', background: '#2563eb', color: '#fff', padding: '2px 8px', borderRadius: '12px', fontSize: '11px', fontWeight: 600 }}>
          <Briefcase size={12} /> Nhân Viên
        </span>
      );
    }
    return (
      <span className="badge badge-member" style={{ display: 'inline-flex', alignItems: 'center', gap: '4px', background: '#059669', color: '#fff', padding: '2px 8px', borderRadius: '12px', fontSize: '11px', fontWeight: 600 }}>
        <UserCheck size={12} /> Bạn Đọc
      </span>
    );
  };

  const getHomeLink = () => {
    if (!user || !user.roles) return '/login';
    if (user.roles.includes('Admin')) return '/admin';
    if (user.roles.includes('Staff')) return '/staff';
    return '/member';
  };

  return (
    <header className="app-header">
      <Link to={getHomeLink()} style={{ textDecoration: 'none' }} className="header-brand">
        <div className="brand-logo">
          <BookOpen size={28} color="#f59e0b" />
        </div>
        <div>
          <h1 className="brand-title">LightBrary</h1>
          <p className="brand-subtitle">
            {portalTitle ? portalTitle : 'Hệ Thống Quản Lý Thư Viện Hiện Đại'}
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
              <span className="user-email">{user.email}</span>
            </div>
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
