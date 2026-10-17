import React from 'react';
import { Outlet } from 'react-router-dom';
import { Header } from '../components/Header';
import { Sidebar } from '../components/Sidebar';

export const StaffLayout: React.FC = () => {
  return (
    <div className="app-layout">
      <Header portalTitle="Thư viện · Cổng thủ thư" />
      <div className="main-content-wrapper">
        <Sidebar role="staff" />
        <main className="app-body">
          <Outlet />
        </main>
      </div>
    </div>
  );
};
