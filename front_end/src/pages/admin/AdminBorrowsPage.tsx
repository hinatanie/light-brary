import React, { useState, useEffect, useCallback } from 'react';
import type { BorrowRecord, User, BookCopy } from '../../types';
import { borrowRecordsApi, usersApi, booksApi, bookCopiesApi } from '../../api';
import { BorrowRecordsTab } from '../../components/BorrowRecordsTab';
import { useToast } from '../../contexts/ToastContext';
import { RefreshCw } from 'lucide-react';

export const AdminBorrowsPage: React.FC = () => {
  const { showToast } = useToast();
  const [loading, setLoading] = useState(true);
  const [borrowRecords, setBorrowRecords] = useState<BorrowRecord[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [availableCopies, setAvailableCopies] = useState<BookCopy[]>([]);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [borrows, usrs, bks] = await Promise.all([
        borrowRecordsApi.getAll(),
        usersApi.getAll(),
        booksApi.getAll(),
      ]);
      setBorrowRecords(borrows);
      setUsers(usrs);

      const copiesArrays = await Promise.all(
        bks.map((b) => bookCopiesApi.getByBookId(b.id).catch(() => [] as BookCopy[]))
      );
      const allCopies = copiesArrays.flat();
      setAvailableCopies(allCopies.filter((c) => c.status === 'Available'));
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi tải dữ liệu mượn trả', 'error');
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
          <span>Đang tải danh sách phiếu mượn...</span>
        </div>
      </div>
    );
  }

  return (
    <BorrowRecordsTab
      borrowRecords={borrowRecords}
      users={users}
      availableCopies={availableCopies}
      onRefresh={loadData}
      onNotify={showToast}
    />
  );
};
