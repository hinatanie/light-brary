import React from 'react';
import { useNavigate } from 'react-router-dom';
import { BooksTab } from '../../components/BooksTab';
import { useToast } from '../../contexts/ToastContext';

export const StaffBooksPage: React.FC = () => {
  const navigate = useNavigate();
  const { showToast } = useToast();

  return (
    <BooksTab
      onNotify={showToast}
      onViewCopies={(bookId) => navigate(`/staff/book-copies?bookId=${bookId}`)}
      onViewBook={(bookId) => navigate(`/staff/books/${bookId}`)}
      readOnly={true}
    />
  );
};
