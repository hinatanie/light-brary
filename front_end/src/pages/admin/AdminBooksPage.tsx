import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Book, Category } from '../../types';
import { booksApi, categoriesApi } from '../../api';
import { BooksTab } from '../../components/BooksTab';
import { useToast } from '../../contexts/ToastContext';
import { RefreshCw } from 'lucide-react';

export const AdminBooksPage: React.FC = () => {
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [loading, setLoading] = useState(true);
  const [books, setBooks] = useState<Book[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [bks, cats] = await Promise.all([
        booksApi.getAll(),
        categoriesApi.getAll(),
      ]);
      setBooks(bks);
      setCategories(cats);
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi tải danh sách sách', 'error');
    } finally {
      setLoading(false);
    }
  }, [showToast]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleViewCopies = (bookId: string) => {
    navigate(`/admin/book-copies?bookId=${bookId}`);
  };

  if (loading) {
    return (
      <div className="tab-container">
        <div className="loading-banner">
          <RefreshCw size={18} className="spin" />
          <span>Đang tải danh mục đầu sách...</span>
        </div>
      </div>
    );
  }

  return (
    <BooksTab
      books={books}
      categories={categories}
      onRefresh={loadData}
      onNotify={showToast}
      onViewCopies={handleViewCopies}
    />
  );
};
