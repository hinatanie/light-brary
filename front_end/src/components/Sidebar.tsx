import React from 'react';
import { NavLink } from 'react-router-dom';
import {
  LayoutDashboard,
  BookOpen,
  Bookmark,
  Layers,
  FileText,
  Users,
  RotateCcw,
  Library,
  User,
  FilePlus2
} from 'lucide-react';

interface SidebarProps {
  role: 'admin' | 'staff' | 'member';
}

export const Sidebar: React.FC<SidebarProps> = ({ role }) => {
  return (
    <nav className="app-sidebar" aria-label="Điều hướng thư viện">
      {role === 'admin' && (
        <>
          <div className="sidebar-group-title">QUẢN TRỊ VIÊN</div>
          <NavLink
            to="/admin"
            end
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <LayoutDashboard size={18} />
            <span>Tổng quan</span>
          </NavLink>
          <NavLink
            to="/admin/books"
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <BookOpen size={18} />
            <span>Đầu sách</span>
          </NavLink>
          <NavLink
            to="/admin/categories"
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <Bookmark size={18} />
            <span>Thể loại</span>
          </NavLink>
          <NavLink
            to="/admin/book-copies"
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <Layers size={18} />
            <span>Bản sao trên kệ</span>
          </NavLink>
          <NavLink
            to="/admin/borrows"
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <FileText size={18} />
            <span>Mượn - Trả sách</span>
          </NavLink>

          <div className="sidebar-group-title" style={{ marginTop: '20px' }}>
            HỆ THỐNG NGƯỜI DÙNG
          </div>
          <NavLink
            to="/admin/users"
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <Users size={18} />
            <span>Người dùng & Quyền</span>
          </NavLink>
          <NavLink
            to="/admin/profile"
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <User size={18} />
            <span>Thông tin cá nhân</span>
          </NavLink>
        </>
      )}

      {role === 'staff' && (
        <>
          <div className="sidebar-group-title">QUẦY THỦ THƯ</div>
          <NavLink
            to="/staff"
            end
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <LayoutDashboard size={18} />
            <span>Bàn làm việc</span>
          </NavLink>
          <NavLink
            to="/staff/create-borrow"
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <FilePlus2 size={18} />
            <span>Lập phiếu mượn</span>
          </NavLink>
          <NavLink
            to="/staff/borrows"
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <RotateCcw size={18} />
            <span>Xử lý mượn - trả</span>
          </NavLink>

          <div className="sidebar-group-title" style={{ marginTop: '20px' }}>
            KHO SÁCH THƯ VIỆN
          </div>
          <NavLink
            to="/staff/books"
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <BookOpen size={18} />
            <span>Tra cứu đầu sách</span>
          </NavLink>
          <NavLink
            to="/staff/book-copies"
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <Library size={18} />
            <span>Kiểm tra bản sao</span>
          </NavLink>
        </>
      )}

      {role === 'member' && (
        <>
          <div className="sidebar-group-title">BẠN ĐỌC</div>
          <NavLink
            to="/member/catalog"
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <Library size={18} />
            <span>Tra cứu danh mục sách</span>
          </NavLink>
          <NavLink
            to="/member"
            end
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <BookOpen size={18} />
            <span>Sách đang mượn</span>
          </NavLink>
          <NavLink
            to="/member/borrows"
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <FileText size={18} />
            <span>Lịch sử phiếu mượn</span>
          </NavLink>
          <NavLink
            to="/member/profile"
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <User size={18} />
            <span>Thông tin cá nhân</span>
          </NavLink>
        </>
      )}
      <div className="sidebar-school">
        <img src="/images/sgu-logo.webp" alt="Logo Đại học Sài Gòn" width="138" height="138" />
        <strong>THƯ VIỆN ĐẠI HỌC SÀI GÒN</strong>
        <span>Cổng thông tin bạn đọc</span>
        <span className="sidebar-version">LightBrary · SGU</span>
      </div>
    </nav>
  );
};
