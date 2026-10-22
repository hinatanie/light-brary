import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { ToastProvider } from './contexts/ToastContext';
import { ProtectedRoute } from './components/ProtectedRoute';

// Layouts
import { AuthLayout } from './layouts/AuthLayout';
import { InternalAuthLayout } from './layouts/InternalAuthLayout';
import { AdminLayout } from './layouts/AdminLayout';
import { StaffLayout } from './layouts/StaffLayout';
import { MemberLayout } from './layouts/MemberLayout';

// Auth & Status Pages
import { LoginPage } from './pages/LoginPage';
import { ChangePasswordPage } from './pages/ChangePasswordPage';
import { UnauthorizedPage } from './pages/UnauthorizedPage';
import { NotFoundPage } from './pages/NotFoundPage';
import { BookDetailsPage } from './pages/BookDetailsPage';

// Admin Pages
import { AdminDashboardPage } from './pages/admin/AdminDashboardPage';
import { AdminBooksPage } from './pages/admin/AdminBooksPage';
import { AdminCategoriesPage } from './pages/admin/AdminCategoriesPage';
import { AdminBookCopiesPage } from './pages/admin/AdminBookCopiesPage';
import { AdminBorrowsPage } from './pages/admin/AdminBorrowsPage';
import { AdminUsersPage } from './pages/admin/AdminUsersPage';

// Staff Pages
import { StaffDashboardPage } from './pages/staff/StaffDashboardPage';
import { StaffBorrowsPage } from './pages/staff/StaffBorrowsPage';
import { StaffCreateBorrowPage } from './pages/staff/StaffCreateBorrowPage';
import { StaffBooksPage } from './pages/staff/StaffBooksPage';
import { StaffBookCopiesPage } from './pages/staff/StaffBookCopiesPage';

// Member Pages
import { MemberDashboardPage } from './pages/member/MemberDashboardPage';
import { MemberCatalogPage } from './pages/member/MemberCatalogPage';
import { MemberBorrowsPage } from './pages/member/MemberBorrowsPage';
import { MemberProfilePage } from './pages/member/MemberProfilePage';

import { RefreshCw } from 'lucide-react';
import './App.css';

// Root Redirect Component
const RootRedirect: React.FC = () => {
  const { user, isAuthenticated, loading } = useAuth();

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh', gap: '12px', color: '#64748b' }}>
        <RefreshCw size={24} className="spin" />
        <span>Đang khởi động hệ thống thư viện...</span>
      </div>
    );
  }

  if (!isAuthenticated || !user) {
    return <Navigate to="/login" replace />;
  }

  if (user.mustChangePassword) {
    return <Navigate to="/change-password" replace />;
  }

  if (user.roles && user.roles.some((r) => r.toLowerCase() === 'admin')) {
    return <Navigate to="/admin" replace />;
  }

  if (user.roles && user.roles.some((r) => r.toLowerCase() === 'staff' || r.toLowerCase() === 'librarian')) {
    return <Navigate to="/staff" replace />;
  }

  return <Navigate to="/member" replace />;
};

export function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <ToastProvider>
          <Routes>
            {/* Root Redirect */}
            <Route path="/" element={<RootRedirect />} />

            {/* Auth Routes */}
            <Route element={<AuthLayout />}>
              <Route path="/login" element={<LoginPage roleMode="member" />} />
            </Route>
            <Route element={<InternalAuthLayout portalTitle="Cổng quản trị thư viện" />}>
              <Route path="/login/admin" element={<LoginPage roleMode="admin" />} />
            </Route>
            <Route element={<InternalAuthLayout portalTitle="Cổng thủ thư" />}>
              <Route path="/login/librarian" element={<LoginPage roleMode="librarian" />} />
            </Route>

            {/* 1. Admin Portal */}
            <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
              <Route path="/admin" element={<AdminLayout />}>
                <Route index element={<AdminDashboardPage />} />
                <Route path="books" element={<AdminBooksPage />} />
                <Route path="books/:bookId" element={<BookDetailsPage portalPath="/admin" />} />
                <Route path="categories" element={<AdminCategoriesPage />} />
                <Route path="book-copies" element={<AdminBookCopiesPage />} />
                <Route path="borrows" element={<AdminBorrowsPage />} />
                <Route path="users" element={<AdminUsersPage />} />
                <Route path="profile" element={<MemberProfilePage title="Hồ Sơ Cá Nhân" />} />
              </Route>
            </Route>

            {/* 2. Staff Portal */}
            <Route element={<ProtectedRoute allowedRoles={['Staff', 'Librarian', 'Admin']} />}>
              <Route path="/staff" element={<StaffLayout />}>
                <Route index element={<StaffDashboardPage />} />
                <Route path="borrows" element={<StaffBorrowsPage />} />
                <Route path="create-borrow" element={<StaffCreateBorrowPage />} />
                <Route path="books" element={<StaffBooksPage />} />
                <Route path="books/:bookId" element={<BookDetailsPage portalPath="/staff" />} />
                <Route path="book-copies" element={<StaffBookCopiesPage />} />
              </Route>
            </Route>

            {/* 3. Member Portal */}
            <Route element={<ProtectedRoute allowedRoles={['Member', 'User', 'Staff', 'Librarian', 'Admin']} />}>
              <Route path="/member" element={<MemberLayout />}>
                <Route index element={<MemberDashboardPage />} />
                <Route path="catalog" element={<MemberCatalogPage />} />
                <Route path="borrows" element={<MemberBorrowsPage />} />
                <Route path="profile" element={<MemberProfilePage />} />
              </Route>
            </Route>

            {/* Change Password (bất kỳ user đăng nhập nào) */}
            <Route element={<ProtectedRoute />}>
              <Route path="/change-password" element={<ChangePasswordPage />} />
            </Route>

            {/* Common / Error Pages */}
            <Route path="/unauthorized" element={<UnauthorizedPage />} />
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </ToastProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
