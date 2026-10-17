import React from 'react';
import { Outlet, Link } from 'react-router-dom';
import { BookOpen, CalendarDays, ClipboardList, ExternalLink, GraduationCap, Library, LogIn } from 'lucide-react';

const currentYear = new Date().getFullYear();

export const AuthLayout: React.FC = () => {
  return (
    <div className="auth-layout">
      <header className="app-header auth-header">
        <Link to="/" className="header-brand">
          <img className="brand-logo" src="/images/sgu-logo.webp" alt="Logo Trường Đại học Sài Gòn" width="42" height="42" />
          <div>
            <p className="brand-title">TRƯỜNG ĐẠI HỌC SÀI GÒN</p>
            <p className="brand-subtitle">Cổng thông tin thư viện</p>
          </div>
        </Link>
        <a className="header-school-link" href="https://sgu.edu.vn/" target="_blank" rel="noreferrer">
          Website nhà trường <ExternalLink size={14} />
        </a>
      </header>

      <div className="auth-content">
        <aside className="auth-sidebar" aria-label="Đăng nhập thư viện">
          <Outlet />
          <div className="sidebar-school">
            <img src="/images/sgu-logo.webp" alt="Logo Đại học Sài Gòn" width="160" height="160" />
            <strong>THƯ VIỆN ĐẠI HỌC SÀI GÒN</strong>
            <span>Đồng hành cùng học tập và nghiên cứu</span>
            <span className="sidebar-version">LightBrary · SGU</span>
          </div>
        </aside>

        <main className="auth-main">
          <section className="portal-panel">
            <div className="portal-panel-heading"><Library size={19} /><h1>Cổng thông tin thư viện</h1></div>
            <div className="portal-introduction">
              <span className="portal-eyebrow">TRƯỜNG ĐẠI HỌC SÀI GÒN</span>
              <h2>Chào mừng bạn đến với thư viện</h2>
              <p>Tra cứu tài liệu, theo dõi tình trạng mượn trả và quản lý thông tin bạn đọc trong cùng một hệ thống.</p>
            </div>
            <div className="portal-services">
              <div><BookOpen size={27} /><h3>Tra cứu tài liệu</h3><p>Tìm sách theo tên, tác giả, thể loại và kiểm tra số bản còn trên kệ.</p></div>
              <div><CalendarDays size={27} /><h3>Theo dõi mượn trả</h3><p>Xem sách đang giữ, hạn trả và lịch sử các phiếu mượn của bạn.</p></div>
              <div><GraduationCap size={27} /><h3>Thông tin bạn đọc</h3><p>Xem hồ sơ, cập nhật địa chỉ liên hệ và thay đổi mật khẩu.</p></div>
            </div>
          </section>

          <section className="portal-panel">
            <div className="portal-panel-heading"><ClipboardList size={19} /><h2>Hướng dẫn sử dụng</h2></div>
            <ol className="portal-instructions">
              <li><span>01</span><div><h3>Đăng nhập tài khoản</h3><p>Sinh viên sử dụng mã số sinh viên và mật khẩu được nhà trường cấp.</p></div></li>
              <li><span>02</span><div><h3>Tìm sách trong thư viện</h3><p>Vào mục “Tra cứu danh mục sách” để xem thông tin đầu sách và bản sao.</p></div></li>
              <li><span>03</span><div><h3>Theo dõi hạn trả sách</h3><p>Kiểm tra mục “Sách đang mượn” và “Lịch sử phiếu mượn” sau khi mượn tại quầy.</p></div></li>
            </ol>
          </section>

          <section className="portal-panel portal-account-note">
            <LogIn size={22} />
            <div><h2>Tài khoản do nhà trường cấp</h2><p>Nếu cần hỗ trợ đăng nhập hoặc thông tin mượn trả, vui lòng liên hệ cán bộ thư viện.</p></div>
          </section>

          <footer className="app-footer">
            <span>© {currentYear} Thư viện Trường Đại học Sài Gòn</span>
            <a href="https://thongtindaotao.sgu.edu.vn/#/home" target="_blank" rel="noreferrer">Cổng thông tin đào tạo <ExternalLink size={13} /></a>
          </footer>
        </main>
      </div>
    </div>
  );
};
