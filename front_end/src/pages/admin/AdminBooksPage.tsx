import React from 'react';
import { useNavigate } from 'react-router-dom';
import { BooksTab } from '../../components/BooksTab';
import { useToast } from '../../contexts/ToastContext';

export const AdminBooksPage: React.FC = () => {
  const navigate = useNavigate();
  const { showToast } = useToast();

  return (
    <BooksTab
      onNotify={showToast}
      onViewCopies={(bookId) => navigate(`/admin/book-copies?bookId=${bookId}`)}
      onViewBook={(bookId) => navigate(`/admin/books/${bookId}`)}
    />
  );
};
