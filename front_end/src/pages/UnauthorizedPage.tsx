import React from 'react';
import { Link } from 'react-router-dom';
import { ShieldAlert, ArrowLeft } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';

export const UnauthorizedPage: React.FC = () => {
  const { user } = useAuth();

  const getBackLink = () => {
    if (!user || !user.roles) return '/login';
    if (user.roles.includes('Admin')) return '/admin';
    if (user.roles.includes('Staff')) return '/staff';
    return '/member';
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '70vh', textAlign: 'center', padding: '24px' }}>
      <div style={{ background: 'rgba(239, 68, 68, 0.15)', padding: '20px', borderRadius: '50%', marginBottom: '20px', color: '#ef4444' }}>
        <ShieldAlert size={56} />
      </div>
      <h1 style={{ fontSize: '28px', fontWeight: 800, color: '#f8fafc', marginBottom: '8px' }}>
        403 - Không Có Quyền Truy Cập
      </h1>
      <p style={{ color: '#94a3b8', maxWidth: '460px', marginBottom: '24px', fontSize: '15px' }}>
        Bạn không có quyền hạn cần thiết để truy cập vào phân hệ này. Vui lòng liên hệ Quản trị viên thư viện nếu bạn cho rằng đây là sự nhầm lẫn.
      </p>
      <Link to={getBackLink()} className="btn btn-primary" style={{ textDecoration: 'none', display: 'inline-flex', alignItems: 'center', gap: '8px' }}>
        <ArrowLeft size={16} /> Quay lại trang chính của bạn
      </Link>
    </div>
  );
};
