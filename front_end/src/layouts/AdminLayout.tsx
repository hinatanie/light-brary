import React from 'react';
import { Outlet } from 'react-router-dom';
import { Header } from '../components/Header';
import { Sidebar } from '../components/Sidebar';

export const AdminLayout: React.FC = () => {
  return (
    <div className="app-layout">
      <Header portalTitle="Cổng Quản Trị Viên (Admin)" />
      <div className="main-content-wrapper">
        <Sidebar role="admin" />
        <main className="app-body">
          <Outlet />
        </main>
      </div>
    </div>
  );
};
