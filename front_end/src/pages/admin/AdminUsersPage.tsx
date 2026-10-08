import React, { useState, useEffect, useCallback } from 'react';
import type { User } from '../../types';
import { usersApi } from '../../api';
import { UsersTab } from '../../components/UsersTab';
import { useToast } from '../../contexts/ToastContext';
import { RefreshCw } from 'lucide-react';

export const AdminUsersPage: React.FC = () => {
  const { showToast } = useToast();
  const [loading, setLoading] = useState(true);
  const [users, setUsers] = useState<User[]>([]);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const usrs = await usersApi.getAll();
      setUsers(usrs);
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi tải danh sách người dùng', 'error');
    } finally {
      setLoading(false);
    }
  }, [showToast]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  if (loading) {
    return (
      <div className="tab-container">
        <div className="loading-banner">
          <RefreshCw size={18} className="spin" />
          <span>Đang tải danh sách người dùng...</span>
        </div>
      </div>
    );
  }

  return (
    <UsersTab
      users={users}
      onNotify={showToast}
      onRefresh={loadData}
    />
  );
};
