import { useState, useEffect, useCallback } from 'react';
import type { Category, Book, BookCopy, BorrowRecord, User, AuthResponse } from './types';
import { categoriesApi, booksApi, bookCopiesApi, borrowRecordsApi, usersApi } from './api';
import { Header } from './components/Header';
import { DashboardTab } from './components/DashboardTab';
import { BooksTab } from './components/BooksTab';
import { CategoriesTab } from './components/CategoriesTab';
import { BookCopiesTab } from './components/BookCopiesTab';
import { BorrowRecordsTab } from './components/BorrowRecordsTab';
import { UsersTab } from './components/UsersTab';
import { AuthModal } from './components/AuthModal';
import { ToastContainer } from './components/Toast';
import type { ToastMessage } from './components/Toast';
import {
  LayoutDashboard,
  BookOpen,
  Bookmark,
  Layers,
  FileText,
  Users,
  RefreshCw,
} from 'lucide-react';
import './App.css';

export function App() {
  const [activeTab, setActiveTab] = useState<string>('dashboard');
  const [loading, setLoading] = useState<boolean>(true);
  const [isConnected, setIsConnected] = useState<boolean>(true);

  // Data states
  const [categories, setCategories] = useState<Category[]>([]);
  const [books, setBooks] = useState<Book[]>([]);
  const [copies, setCopies] = useState<BookCopy[]>([]);
  const [borrowRecords, setBorrowRecords] = useState<BorrowRecord[]>([]);
  const [users, setUsers] = useState<User[]>([]);

  // Navigation state for Book Copies
  const [selectedBookIdForCopies, setSelectedBookIdForCopies] = useState<number | null>(null);

  // Auth & Modal states
  const [isAuthModalOpen, setIsAuthModalOpen] = useState(false);
  const [currentUser, setCurrentUser] = useState<AuthResponse | null>(() => {
    const saved = localStorage.getItem('lightbrary_user');
    return saved ? JSON.parse(saved) : null;
  });

  // Toasts
  const [toasts, setToasts] = useState<ToastMessage[]>([]);

  const showToast = (text: string, type: 'success' | 'error' | 'info' = 'success') => {
    const id = Date.now().toString() + Math.random().toString();
    setToasts((prev) => [...prev, { id, text, type }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 4500);
  };

  const dismissToast = (id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  };

  // Load all initial data from APIs
  const loadAllData = useCallback(async () => {
    setLoading(true);
    try {
      // 1. Categories
      const cats = await categoriesApi.getAll();
      setCategories(cats);

      // 2. Books
      const bks = await booksApi.getAll();
      setBooks(bks);

      // 3. Copies: Lấy bản sao của tất cả các đầu sách
      const allCopiesPromises = bks.map((b) =>
        bookCopiesApi.getByBookId(b.id).catch(() => [] as BookCopy[])
      );
      const copiesArrays = await Promise.all(allCopiesPromises);
      const flattenedCopies = copiesArrays.flat();
      setCopies(flattenedCopies);

      // 4. Borrow Records
      const borrows = await borrowRecordsApi.getAll();
      setBorrowRecords(borrows);

      // 5. Users
      const usrs = await usersApi.getAll();
      setUsers(usrs);

      setIsConnected(true);
    } catch (err: any) {
      setIsConnected(false);
      console.error('Lỗi khi tải dữ liệu từ backend:', err);
      showToast(
        err.message || 'Không thể kết nối đến Backend API (cổng 5048). Vui lòng kiểm tra server C#.',
        'error'
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadAllData();
  }, [loadAllData]);

  const handleLogout = () => {
    localStorage.removeItem('lightbrary_token');
    localStorage.removeItem('lightbrary_user');
    setCurrentUser(null);
    showToast('Đã đăng xuất khỏi hệ thống.');
  };

  const handleViewCopiesForBook = (bookId: number) => {
    setSelectedBookIdForCopies(bookId);
    setActiveTab('copies');
  };

  const availableCopies = copies.filter((c) => c.status === 'Available');

  return (
    <div className="app-layout">
      <Header
        currentUser={currentUser}
        onOpenAuth={() => setIsAuthModalOpen(true)}
        onLogout={handleLogout}
        isConnected={isConnected}
      />

      <div className="main-content-wrapper">
        {/* Navigation Sidebar */}
        <nav className="app-sidebar">
          <div className="sidebar-group-title">QUẢN LÝ THƯ VIỆN</div>
          <button
            className={`nav-item ${activeTab === 'dashboard' ? 'active' : ''}`}
            onClick={() => setActiveTab('dashboard')}
          >
            <LayoutDashboard size={18} />
            <span>Tổng quan</span>
          </button>

          <button
            className={`nav-item ${activeTab === 'books' ? 'active' : ''}`}
            onClick={() => setActiveTab('books')}
          >
            <BookOpen size={18} />
            <span>Đầu sách ({books.length})</span>
          </button>

          <button
            className={`nav-item ${activeTab === 'categories' ? 'active' : ''}`}
            onClick={() => setActiveTab('categories')}
          >
            <Bookmark size={18} />
            <span>Thể loại ({categories.length})</span>
          </button>

          <button
            className={`nav-item ${activeTab === 'copies' ? 'active' : ''}`}
            onClick={() => {
              setSelectedBookIdForCopies(null);
              setActiveTab('copies');
            }}
          >
            <Layers size={18} />
            <span>Bản sao trên kệ ({copies.length})</span>
          </button>

          <button
            className={`nav-item ${activeTab === 'borrows' ? 'active' : ''}`}
            onClick={() => setActiveTab('borrows')}
          >
            <FileText size={18} />
            <span>Mượn - Trả sách ({borrowRecords.length})</span>
          </button>

          <div className="sidebar-group-title" style={{ marginTop: '20px' }}>
            HỆ THỐNG ĐỘC GIẢ
          </div>
          <button
            className={`nav-item ${activeTab === 'users' ? 'active' : ''}`}
            onClick={() => setActiveTab('users')}
          >
            <Users size={18} />
            <span>Độc giả ({users.length})</span>
          </button>

          <div style={{ marginTop: 'auto', padding: '16px 8px' }}>
            <button
              className="btn btn-secondary"
              style={{ width: '100%', justifyContent: 'center' }}
              onClick={loadAllData}
              disabled={loading}
            >
              <RefreshCw size={16} className={loading ? 'spin' : ''} />
              <span>Làm mới dữ liệu</span>
            </button>
          </div>
        </nav>

        {/* Content Area */}
        <main className="app-body">
          {loading && (
            <div className="loading-banner">
              <RefreshCw size={18} className="spin" />
              <span>Đang đồng bộ dữ liệu từ Library.Api...</span>
            </div>
          )}

          {activeTab === 'dashboard' && (
            <DashboardTab
              books={books}
              categories={categories}
              copies={copies}
              borrowRecords={borrowRecords}
              users={users}
              onNavigate={setActiveTab}
            />
          )}

          {activeTab === 'books' && (
            <BooksTab
              books={books}
              categories={categories}
              onRefresh={loadAllData}
              onNotify={showToast}
              onViewCopies={handleViewCopiesForBook}
            />
          )}

          {activeTab === 'categories' && (
            <CategoriesTab
              categories={categories}
              onRefresh={loadAllData}
              onNotify={showToast}
            />
          )}

          {activeTab === 'copies' && (
            <BookCopiesTab
              copies={copies}
              books={books}
              selectedBookId={selectedBookIdForCopies}
              onClearSelectedBook={() => setSelectedBookIdForCopies(null)}
              onRefresh={loadAllData}
              onNotify={showToast}
            />
          )}

          {activeTab === 'borrows' && (
            <BorrowRecordsTab
              borrowRecords={borrowRecords}
              users={users}
              availableCopies={availableCopies}
              onRefresh={loadAllData}
              onNotify={showToast}
            />
          )}

          {activeTab === 'users' && (
            <UsersTab users={users} onNotify={showToast} />
          )}
        </main>
      </div>

      <AuthModal
        isOpen={isAuthModalOpen}
        onClose={() => setIsAuthModalOpen(false)}
        onSuccess={(auth) => setCurrentUser(auth)}
        onNotify={showToast}
      />

      <ToastContainer toasts={toasts} onDismiss={dismissToast} />
    </div>
  );
}

export default App;
