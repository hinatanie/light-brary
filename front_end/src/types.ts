export interface Category {
  id: string;
  name: string;
  description?: string | null;
  bookCount: number;
}

export interface Book {
  id: string;
  title: string;
  author: string;
  isbn?: string | null;
  publishYear?: number | null;
  imageUrl?: string | null;
  categoryId: string;
  categoryName?: string | null;
  createdAt: string;
}

export type BookCopyStatus = 'Available' | 'Borrowed' | 'Lost';

export interface BookCopy {
  id: string;
  bookId: string;
  bookTitle: string;
  bookAuthor: string;
  barcode: string;
  status: BookCopyStatus | string;
  conditionNote?: string | null;
}

export interface User {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  address?: string | null;
  createdAt: string;
  roles: string[];
  mustChangePassword?: boolean;
}

export interface Role {
  id: string;
  name: string;
}

export type BorrowRecordStatus = 'Borrowing' | 'Returned' | 'Overdue';
export type BorrowDetailStatus = 'Borrowing' | 'Returned' | 'Lost';

export interface BorrowDetail {
  id: string;
  bookCopyId: string;
  barcode: string;
  bookTitle: string;
  bookAuthor: string;
  returnDate?: string | null;
  status: BorrowDetailStatus | string;
  fineAmount: number;
  isOverdue?: boolean;
}

export interface BorrowRecord {
  id: string;
  userId: string;
  userFullName: string;
  borrowDate: string;
  dueDate: string;
  status: BorrowRecordStatus | string;
  note?: string | null;
  details: BorrowDetail[];
}

export interface AuthResponse {
  token: string;
  userId: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  expiresAt: string;
  roles: string[];
  mustChangePassword?: boolean;
}

export interface MeResponse {
  userId: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  address?: string | null;
  roles: string[];
  mustChangePassword?: boolean;
}

export interface UpdateUserRequest {
  fullName: string;
  phoneNumber: string;
  address?: string | null;
}
