import React from 'react';
import { Navigate, useLocation, Outlet } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { RefreshCw } from 'lucide-react';

interface ProtectedRouteProps {
  allowedRoles?: string[];
  children?: React.ReactNode;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ allowedRoles, children }) => {
  const { isAuthenticated, user, loading } = useAuth();
  const location = useLocation();

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh', gap: '12px', color: '#64748b' }}>
        <RefreshCw size={24} className="spin" />
        <span>Đang kiểm tra phiên đăng nhập...</span>
      </div>
    );
  }

  if (!isAuthenticated || !user) {
    let loginRedirect = '/login';
    if (location.pathname.startsWith('/admin')) {
      loginRedirect = '/login/admin';
    } else if (location.pathname.startsWith('/staff')) {
      loginRedirect = '/login/librarian';
    }
    return <Navigate to={loginRedirect} state={{ from: location }} replace />;
  }

  if (user.mustChangePassword && location.pathname !== '/change-password') {
    return <Navigate to="/change-password" replace />;
  }

  if (allowedRoles && allowedRoles.length > 0) {
    const userRoles = user.roles && user.roles.length > 0 ? user.roles : ['Member'];
    const hasRequiredRole = userRoles.some((role) =>
      allowedRoles.some((allowed) => allowed.toLowerCase() === role.toLowerCase())
    );

    if (!hasRequiredRole) {
      return <Navigate to="/unauthorized" replace />;
    }
  }

  return children ? <>{children}</> : <Outlet />;
};
