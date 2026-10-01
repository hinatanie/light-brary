import React from 'react';
import type { AuthResponse } from '../types';
import { BookOpen, User, LogOut, LogIn, Wifi, WifiOff } from 'lucide-react';

interface HeaderProps {
  currentUser: AuthResponse | null;
  onOpenAuth: () => void;
  onLogout: () => void;
  isConnected: boolean;
}

export const Header: React.FC<HeaderProps> = ({
  currentUser,
  onOpenAuth,
  onLogout,
  isConnected,
}) => {
  return (
    <header className="app-header">
      <div className="header-brand">
        <div className="brand-logo">
          <BookOpen size={28} color="#f59e0b" />
        </div>
        <div>
          <h1 className="brand-title">LightBrary</h1>
          <p className="brand-subtitle">Hệ Thống Quản Lý Thư Viện Hiện Đại</p>
        </div>
      </div>

      <div className="header-actions">
        <div className={`status-indicator ${isConnected ? 'online' : 'offline'}`}>
          {isConnected ? <Wifi size={14} /> : <WifiOff size={14} />}
          <span>{isConnected ? 'Backend: Kết nối tốt' : 'Mất kết nối Backend'}</span>
        </div>

        {currentUser ? (
          <div className="user-profile-badge">
            <div className="user-avatar">
              <User size={16} />
            </div>
            <div className="user-details">
              <span className="user-name">{currentUser.fullName}</span>
              <span className="user-email">{currentUser.email}</span>
            </div>
            <button className="btn-logout" onClick={onLogout} title="Đăng xuất">
              <LogOut size={16} />
            </button>
          </div>
        ) : (
          <button className="btn btn-auth" onClick={onOpenAuth}>
            <LogIn size={16} /> Đăng Nhập / Đăng Ký
          </button>
        )}
      </div>
    </header>
  );
};
