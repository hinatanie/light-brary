export interface Category {
  id: string | number;
  name: string;
  description?: string | null;
  bookCount?: number;
}

export interface Book {
  id: string | number;
  title: string;
  author: string;
  isbn?: string | null;
  publishYear?: number | null;
  imageUrl?: string | null;
  categoryId: string | number;
  categoryName?: string | null;
  createdAt?: string;
  totalCopies?: number;
  availableCopies?: number;
}

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export type BookSort = 'newest' | 'title' | 'available' | 'year';

export interface BookPageQuery {
  page: number;
  pageSize?: number;
  keyword?: string;
  categoryId?: string | number;
  author?: string;
  onlyAvailable?: boolean;
  sortBy?: BookSort;
}

export type BookCopyStatus = 'Available' | 'Borrowed' | 'Lost';

export interface BookCopy {
  id: string | number;
  bookId: string | number;
  bookTitle?: string;
  bookAuthor?: string;
  barcode: string;
  status: BookCopyStatus | string;
  conditionNote?: string | null;
}

export interface User {
  id: string | number;
  fullName: string;
  email?: string;
  phoneNumber: string;
  address?: string | null;
  createdAt?: string;
  roles: string[];
}

export interface Role {
  id: string | number;
  name: string;
}

export type BorrowRecordStatus = 'Borrowing' | 'Returned' | 'Overdue';
export type BorrowDetailStatus = 'Borrowing' | 'Returned' | 'Lost';

export interface BorrowDetail {
  id: string | number;
  bookCopyId: string | number;
  barcode: string;
  bookTitle: string;
  bookAuthor?: string;
  returnDate?: string | null;
  status: BorrowDetailStatus | string;
  isOverdue?: boolean;
}

export interface BorrowRecord {
  id: string | number;
  userId: string | number;
  userFullName?: string;
  user?: User;
  borrowDate: string;
  dueDate: string;
  status: BorrowRecordStatus | string;
  note?: string | null;
  details: BorrowDetail[];
  totalBooks?: number;
  borrowingCount?: number;
  returnedCount?: number;
  lostCount?: number;
  isOverdue?: boolean;
}

export interface AuthResponse {
  token: string;
  accessToken?: string;
  tokenType?: string;
  expiresIn?: number;
  userId: string;
  fullName: string;
  email?: string;
  phoneNumber: string;
  expiresAt?: string;
  roles: string[];
  user?: User;
}

export interface MeResponse {
  userId: string;
  id?: string | number;
  fullName: string;
  email?: string;
  phoneNumber: string;
  address?: string | null;
  createdAt?: string;
  roles: string[];
}
