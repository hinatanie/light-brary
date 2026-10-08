import React, { useState, useEffect, useCallback } from 'react';
import type { Category } from '../../types';
import { categoriesApi } from '../../api';
import { CategoriesTab } from '../../components/CategoriesTab';
import { useToast } from '../../contexts/ToastContext';
import { RefreshCw } from 'lucide-react';

export const AdminCategoriesPage: React.FC = () => {
  const { showToast } = useToast();
  const [loading, setLoading] = useState(true);
  const [categories, setCategories] = useState<Category[]>([]);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const cats = await categoriesApi.getAll();
      setCategories(cats);
    } catch (err: any) {
      showToast(err.message || 'Lỗi khi tải danh sách thể loại', 'error');
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
          <span>Đang tải danh mục thể loại...</span>
        </div>
      </div>
    );
  }

  return (
    <CategoriesTab
      categories={categories}
      onRefresh={loadData}
      onNotify={showToast}
    />
  );
};
