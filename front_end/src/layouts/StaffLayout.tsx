import React from 'react';
import { Outlet } from 'react-router-dom';
import { Header } from '../components/Header';
import { Sidebar } from '../components/Sidebar';

export const StaffLayout: React.FC = () => {
  return (
    <div className="app-layout">
      <Header portalTitle="Cổng Thủ Thư (Staff)" />
      <div className="main-content-wrapper">
        <Sidebar role="staff" />
        <main className="app-body">
          <Outlet />
        </main>
      </div>
    </div>
  );
};
