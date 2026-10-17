import React from 'react';
import { Outlet } from 'react-router-dom';
import { Header } from '../components/Header';
import { Sidebar } from '../components/Sidebar';

export const MemberLayout: React.FC = () => {
  return (
    <div className="app-layout">
      <Header portalTitle="Thư viện · Cổng sinh viên" />
      <div className="main-content-wrapper">
        <Sidebar role="member" />
        <main className="app-body">
          <Outlet />
        </main>
      </div>
    </div>
  );
};
