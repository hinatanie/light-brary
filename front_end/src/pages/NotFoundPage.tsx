import React from 'react';
import { Link } from 'react-router-dom';
import { HelpCircle, ArrowLeft } from 'lucide-react';

export const NotFoundPage: React.FC = () => {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '70vh', textAlign: 'center', padding: '24px' }}>
      <div style={{ background: 'rgba(245, 158, 11, 0.15)', padding: '20px', borderRadius: '50%', marginBottom: '20px', color: '#f59e0b' }}>
        <HelpCircle size={56} />
      </div>
      <h1 style={{ fontSize: '28px', fontWeight: 800, color: '#f8fafc', marginBottom: '8px' }}>
        404 - Không Tìm Thấy Trang
      </h1>
      <p style={{ color: '#94a3b8', maxWidth: '460px', marginBottom: '24px', fontSize: '15px' }}>
        Đường dẫn bạn yêu cầu không tồn tại hoặc đã được di chuyển trong hệ thống LightBrary.
      </p>
      <Link to="/" className="btn btn-primary" style={{ textDecoration: 'none', display: 'inline-flex', alignItems: 'center', gap: '8px' }}>
        <ArrowLeft size={16} /> Về trang chủ
      </Link>
    </div>
  );
};
