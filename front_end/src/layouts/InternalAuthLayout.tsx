import React from 'react';
import { Outlet } from 'react-router-dom';

interface InternalAuthLayoutProps {
  portalTitle: string;
}

export const InternalAuthLayout: React.FC<InternalAuthLayoutProps> = ({ portalTitle }) => {
  return (
    <div className="auth-layout">
      <header className="app-header">
        <div className="header-brand">
          <img className="brand-logo" src="/images/sgu-logo.webp" alt="Logo Trường Đại học Sài Gòn" width="42" height="42" />
          <div>
            <p className="brand-title">TRƯỜNG ĐẠI HỌC SÀI GÒN</p>
            <p className="brand-subtitle">{portalTitle}</p>
          </div>
        </div>
      </header>

      <main className="internal-auth-main">
        <div className="internal-auth-card">
          <div className="internal-auth-brand">
            <img src="/images/sgu-logo.webp" alt="Logo Đại học Sài Gòn" width="90" height="90" />
            <h1>{portalTitle}</h1>
            <p>Dành cho cán bộ được cấp quyền truy cập.</p>
          </div>
          <Outlet />
        </div>
      </main>
    </div>
  );
};
