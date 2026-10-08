import React, { useState } from 'react';
import type { Category } from '../types';
import { categoriesApi } from '../api';
import { Plus, Edit2, Trash2, Bookmark, X, BookOpen } from 'lucide-react';

interface CategoriesTabProps {
  categories: Category[];
  onRefresh: () => void;
  onNotify: (text: string, type?: 'success' | 'error') => void;
}

export const CategoriesTab: React.FC<CategoriesTabProps> = ({
  categories,
  onRefresh,
  onNotify,
}) => {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<Category | null>(null);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const openCreateModal = () => {
    setEditingCategory(null);
    setName('');
    setDescription('');
    setIsModalOpen(true);
  };

  const openEditModal = (cat: Category) => {
    setEditingCategory(cat);
    setName(cat.name);
    setDescription(cat.description || '');
    setIsModalOpen(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      const payload = {
        name: name.trim(),
        description: description.trim() || undefined,
      };

      if (editingCategory) {
        await categoriesApi.update(editingCategory.id, payload);
        onNotify(`Cập nhật thể loại "${name}" thành công!`);
      } else {
        await categoriesApi.create(payload);
        onNotify(`Thêm thể loại "${name}" thành công!`);
      }
      setIsModalOpen(false);
      onRefresh();
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi lưu thể loại', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (cat: Category) => {
    if ((cat.bookCount ?? 0) > 0) {
      onNotify(`Thể loại "${cat.name}" đang chứa ${cat.bookCount} đầu sách, không thể xóa!`, 'error');
      return;
    }
    if (!window.confirm(`Bạn có chắc chắn muốn xóa thể loại "${cat.name}"?`)) return;

    try {
      await categoriesApi.delete(cat.id);
      onNotify(`Đã xóa thể loại "${cat.name}"`);
      onRefresh();
    } catch (err: any) {
      onNotify(err.message || 'Lỗi khi xóa thể loại', 'error');
    }
  };

  return (
    <div className="tab-container">
      <div className="section-header">
        <div>
          <h2>🏷️ Phân Loại & Thể Loại Sách</h2>
          <p className="section-desc">Quản lý các danh mục phân loại sách trong kho thư viện</p>
        </div>
        <button className="btn btn-primary" onClick={openCreateModal}>
          <Plus size={18} /> Thêm Thể Loại
        </button>
      </div>

      <div className="categories-grid">
        {categories.map((c) => (
          <div key={c.id} className="category-card">
            <div className="category-card-top">
              <div className="cat-icon-wrap">
                <Bookmark size={20} color="#d97706" />
              </div>
              <div className="cat-book-badge">
                <BookOpen size={14} /> {c.bookCount !== undefined ? `${c.bookCount} đầu sách` : 'Thể loại'}
              </div>
            </div>

            <h3 className="cat-title">{c.name}</h3>
            <p className="cat-desc">{c.description || 'Chưa có phần mô tả chi tiết.'}</p>

            <div className="cat-card-actions">
              <button
                className="btn-action"
                title="Sửa thể loại"
                onClick={() => openEditModal(c)}
              >
                <Edit2 size={16} /> Sửa
              </button>
              <button
                className="btn-action btn-danger"
                title="Xóa thể loại"
                onClick={() => handleDelete(c)}
              >
                <Trash2 size={16} /> Xóa
              </button>
            </div>
          </div>
        ))}
      </div>

      {isModalOpen && (
        <div className="modal-backdrop" onClick={() => setIsModalOpen(false)}>
          <div className="modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>{editingCategory ? '✏️ Sửa Thể Loại' : '🏷️ Thêm Thể Loại Mới'}</h3>
              <button className="btn-icon" onClick={() => setIsModalOpen(false)}>
                <X size={18} />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="form-content">
              <div className="form-group">
                <label>Tên thể loại *</label>
                <input
                  type="text"
                  required
                  placeholder="Ví dụ: Khoa học viễn tưởng"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                />
              </div>

              <div className="form-group">
                <label>Mô tả chi tiết</label>
                <textarea
                  rows={3}
                  placeholder="Mô tả tóm tắt về loại sách này..."
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                />
              </div>

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setIsModalOpen(false)}
                >
                  Hủy
                </button>
                <button type="submit" className="btn btn-primary" disabled={submitting}>
                  {submitting ? 'Đang lưu...' : editingCategory ? 'Cập Nhật' : 'Tạo Mới'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
