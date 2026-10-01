import type {
  Category,
  Book,
  BookCopy,
  User,
  BorrowRecord,
  AuthResponse
} from './types';

const API_BASE = '/api/v1';

export class ApiError extends Error {
  status: number;
  constructor(message: string, status: number) {
    super(message);
    this.status = status;
    this.name = 'ApiError';
  }
}

async function request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('lightbrary_token');
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE}${endpoint}`, {
    ...options,
    headers,
  });

  if (response.status === 204) {
    return {} as T;
  }

  let data: any = null;
  const contentType = response.headers.get('content-type');
  if (contentType && contentType.includes('application/json')) {
    data = await response.json();
  } else {
    data = await response.text();
  }

  if (!response.ok) {
    const errorMsg = data?.message || data?.title || (typeof data === 'string' && data) || `Yêu cầu thất bại (${response.status})`;
    throw new ApiError(errorMsg, response.status);
  }

  return data as T;
}

// ─── 1. Auth API ─────────────────────────────────────────────────────────────
export const authApi = {
  login: (data: { email: string; password: string }) =>
    request<AuthResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  register: (data: {
    fullName: string;
    email: string;
    password: string;
    phoneNumber?: string;
    address?: string;
  }) =>
    request<AuthResponse>('/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
};

// ─── 2. Categories API ───────────────────────────────────────────────────────
export const categoriesApi = {
  getAll: () => request<Category[]>('/categories'),
  getById: (id: number) => request<Category>(`/categories/${id}`),
  create: (data: { name: string; description?: string }) =>
    request<Category>('/categories', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  update: (id: number, data: { name: string; description?: string }) =>
    request<Category>(`/categories/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
  delete: (id: number) =>
    request<void>(`/categories/${id}`, {
      method: 'DELETE',
    }),
};

// ─── 3. Books API ────────────────────────────────────────────────────────────
export const booksApi = {
  getAll: () => request<Book[]>('/books'),
  getById: (id: number) => request<Book>(`/books/${id}`),
  create: (data: {
    title: string;
    author: string;
    isbn?: string;
    publishYear?: number;
    categoryId: number;
  }) =>
    request<Book>('/books', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  update: (
    id: number,
    data: {
      title: string;
      author: string;
      isbn?: string;
      publishYear?: number;
      categoryId: number;
    }
  ) =>
    request<Book>(`/books/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
  delete: (id: number) =>
    request<void>(`/books/${id}`, {
      method: 'DELETE',
    }),
};

// ─── 4. Book Copies API ──────────────────────────────────────────────────────
export const bookCopiesApi = {
  getByBookId: (bookId: number) => request<BookCopy[]>(`/books/${bookId}/copies`),
  getById: (id: number) => request<BookCopy>(`/book-copies/${id}`),
  create: (data: { bookId: number; barcode: string; conditionNote?: string }) =>
    request<BookCopy>('/book-copies', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  updateCondition: (id: number, data: { conditionNote?: string }) =>
    request<BookCopy>(`/book-copies/${id}/condition`, {
      method: 'PATCH',
      body: JSON.stringify(data),
    }),
  delete: (id: number) =>
    request<void>(`/book-copies/${id}`, {
      method: 'DELETE',
    }),
};

// ─── 5. Users API ────────────────────────────────────────────────────────────
export const usersApi = {
  getAll: () => request<User[]>('/users'),
  getById: (id: string) => request<User>(`/users/${id}`),
  getUserBorrowRecords: (userId: string) =>
    request<BorrowRecord[]>(`/users/${userId}/borrow-records`),
};

// ─── 6. Borrow Records API ───────────────────────────────────────────────────
export const borrowRecordsApi = {
  getAll: () => request<BorrowRecord[]>('/borrow-records'),
  getById: (id: number) => request<BorrowRecord>(`/borrow-records/${id}`),
  create: (data: {
    userId: string;
    dueDate: string;
    bookCopyIds: number[];
    note?: string;
  }) =>
    request<BorrowRecord>('/borrow-records', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  returnBooks: (
    borrowRecordId: number,
    data: { items: { borrowDetailId: number; fineAmount: number }[] }
  ) =>
    request<BorrowRecord>(`/borrow-records/${borrowRecordId}/return`, {
      method: 'PATCH',
      body: JSON.stringify(data),
    }),
};
