import React, { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import type { BookCopy } from '../../types';
import { bookCopiesApi } from '../../api';
import { BookCopiesTab } from '../../components/BookCopiesTab';
import { useToast } from '../../contexts/ToastContext';
import { RefreshCw } from 'lucide-react';

export const StaffBookCopiesPage: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const bookIdParam = searchParams.get('bookId');
  const selectedBookId = bookIdParam || null;

  const { showToast } = useToast();
  const [loading, setLoading] = useState(true);
  const [copies, setCopies] = useState<BookCopy[]>([]);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const allCopies = await bookCopiesApi.getAll();
      setCopies(allCopies);
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi tải danh sách bản sao', 'error');
    } finally {
      setLoading(false);
    }
  }, [showToast]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleClearSelectedBook = () => {
    searchParams.delete('bookId');
    setSearchParams(searchParams);
  };

  if (loading) {
    return (
      <div className="tab-container">
        <div className="loading-banner">
          <RefreshCw size={18} className="spin" />
          <span>Đang tải danh sách bản sao sách...</span>
        </div>
      </div>
    );
  }

  return (
    <BookCopiesTab
      copies={copies}
      selectedBookId={selectedBookId}
      onClearSelectedBook={handleClearSelectedBook}
      onRefresh={loadData}
      onNotify={showToast}
      readOnly={true}
    />
  );
};
