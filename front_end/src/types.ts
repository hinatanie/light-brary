export interface Category {
  id: number;
  name: string;
  description?: string | null;
  bookCount: number;
}

export interface Book {
  id: number;
  title: string;
  author: string;
  isbn?: string | null;
  publishYear?: number | null;
  imageUrl?: string | null;
  categoryId: number;
  categoryName?: string | null;
  createdAt: string;
}

export type BookCopyStatus = 'Available' | 'Borrowed' | 'Lost';

export interface BookCopy {
  id: number;
  bookId: number;
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
  phoneNumber?: string | null;
  address?: string | null;
  createdAt: string;
  roles?: string[];
}

export interface Role {
  id: string;
  name: string;
}

export type BorrowRecordStatus = 'Borrowing' | 'Returned' | 'Overdue';
export type BorrowDetailStatus = 'Borrowing' | 'Returned' | 'Lost';

export interface BorrowDetail {
  id: number;
  bookCopyId: number;
  barcode: string;
  bookTitle: string;
  bookAuthor: string;
  returnDate?: string | null;
  status: BorrowDetailStatus | string;
  fineAmount: number;
}

export interface BorrowRecord {
  id: number;
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
  expiresAt: string;
  roles: string[];
}

export interface MeResponse {
  userId: string;
  fullName: string;
  email: string;
  phoneNumber?: string | null;
  roles: string[];
}

export interface UpdateUserRequest {
  fullName: string;
  phoneNumber?: string | null;
  address?: string | null;
}
