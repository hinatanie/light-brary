import React from 'react';
import { Outlet } from 'react-router-dom';
import { Header } from '../components/Header';
import { Sidebar } from '../components/Sidebar';

interface PortalLayoutProps {
  portalTitle: string;
  role: 'admin' | 'staff' | 'member';
}

export const PortalLayout: React.FC<PortalLayoutProps> = ({ portalTitle, role }) => {
  return (
    <div className="app-layout">
      <Header portalTitle={portalTitle} />
      <div className="main-content-wrapper">
        <Sidebar role={role} />
        <main className="app-body">
          <Outlet />
        </main>
      </div>
    </div>
  );
};
