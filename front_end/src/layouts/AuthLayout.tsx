import React from 'react';
import { Outlet, Link } from 'react-router-dom';
import { BookOpen } from 'lucide-react';

export const AuthLayout: React.FC = () => {
  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column', backgroundColor: '#0f172a', alignItems: 'center', justifyContent: 'center', padding: '24px 16px' }}>
      <div style={{ marginBottom: '24px', textAlign: 'center' }}>
        <Link to="/" style={{ textDecoration: 'none', display: 'inline-flex', alignItems: 'center', gap: '12px' }}>
          <div style={{ background: 'rgba(245, 158, 11, 0.15)', padding: '10px', borderRadius: '12px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <BookOpen size={36} color="#f59e0b" />
          </div>
          <div style={{ textAlign: 'left' }}>
            <h1 style={{ fontSize: '26px', fontWeight: 800, color: '#f8fafc', margin: 0 }}>LightBrary</h1>
            <p style={{ fontSize: '13px', color: '#94a3b8', margin: 0 }}>Hệ Thống Quản Lý Thư Viện</p>
          </div>
        </Link>
      </div>

      <div style={{ width: '100%', maxWidth: '440px' }}>
        <Outlet />
      </div>

      <footer style={{ marginTop: '32px', color: '#64748b', fontSize: '12px', textAlign: 'center' }}>
        © 2026 LightBrary • Thiết kế Clean Architecture & React Router
      </footer>
    </div>
  );
};
