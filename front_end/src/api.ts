import type {
  Category,
  Book,
  BookCopy,
  User,
  Role,
  BorrowRecord,
  AuthResponse,
  MeResponse,
  UpdateUserRequest,
} from './types';

const API_BASE = '/api/v1';

export class ApiError extends Error {
  status: number;
  code?: string;
  lockoutEndUtc?: string;

  constructor(message: string, status: number, code?: string, lockoutEndUtc?: string) {
    super(message);
    this.status = status;
    this.code = code;
    this.lockoutEndUtc = lockoutEndUtc;
    this.name = 'ApiError';
  }
}

let isRefreshing = false;
let refreshSubscribers: ((token: string | null) => void)[] = [];

function onRefreshed(token: string | null) {
  refreshSubscribers.forEach((cb) => cb(token));
  refreshSubscribers = [];
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
    credentials: 'include', // Gửi kèm HttpOnly cookie cho refresh token
  });

  if (response.status === 204) {
    return {} as T;
  }

  // Xử lý tự động Refresh Token khi Access Token hết hạn (401)
  const isAuthEndpoint =
    endpoint.includes('/auth/login') ||
    endpoint.includes('/auth/register') ||
    endpoint.includes('/auth/refresh') ||
    endpoint.includes('/auth/logout');

  if (response.status === 401 && !isAuthEndpoint && token) {
    if (!isRefreshing) {
      isRefreshing = true;
      try {
        const refreshRes = await fetch(`${API_BASE}/auth/refresh`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          credentials: 'include',
        });

        if (refreshRes.ok) {
          const authData: AuthResponse = await refreshRes.json();
          localStorage.setItem('lightbrary_token', authData.token);
          localStorage.setItem('lightbrary_user', JSON.stringify({
            userId: authData.userId,
            fullName: authData.fullName,
            email: authData.email,
            phoneNumber: authData.phoneNumber,
            roles: authData.roles,
            mustChangePassword: authData.mustChangePassword,
          }));
          isRefreshing = false;
          onRefreshed(authData.token);
          // Thử lại request ban đầu với token mới
          return request<T>(endpoint, options);
        } else {
          isRefreshing = false;
          onRefreshed(null);
          localStorage.removeItem('lightbrary_token');
          localStorage.removeItem('lightbrary_user');
          window.location.href = '/login';
          throw new ApiError('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.', 401, 'SESSION_EXPIRED');
        }
      } catch (err) {
        isRefreshing = false;
        onRefreshed(null);
        throw err;
      }
    }

    // Đang có request khác refresh, xếp hàng chờ
    return new Promise<T>((resolve, reject) => {
      refreshSubscribers.push((newToken) => {
        if (newToken) {
          resolve(request<T>(endpoint, options));
        } else {
          reject(new ApiError('Phiên đăng nhập đã hết hạn.', 401, 'SESSION_EXPIRED'));
        }
      });
    });
  }

  let data: any = null;
  const contentType = response.headers.get('content-type');
  if (contentType && contentType.includes('application/json')) {
    data = await response.json();
  } else {
    data = await response.text();
  }

  if (!response.ok) {
    const errorMsg =
      data?.message ||
      data?.title ||
      (typeof data === 'string' && data) ||
      `Yêu cầu thất bại (${response.status})`;
    throw new ApiError(errorMsg, response.status, data?.code, data?.lockoutEndUtc);
  }

  return data as T;
}

// ─── 1. Auth API ─────────────────────────────────────────────────────────────
export const authApi = {
  login: (data: { phoneNumber: string; password: string }) =>
    request<AuthResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  register: (data: {
    fullName: string;
    email: string;
    password: string;
    phoneNumber: string;
    address?: string;
  }) =>
    request<AuthResponse>('/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  refresh: () =>
    request<AuthResponse>('/auth/refresh', {
      method: 'POST',
    }),

  logout: () =>
    request<void>('/auth/logout', {
      method: 'POST',
    }),

  changePassword: (data: { currentPassword: string; newPassword: string }) =>
    request<AuthResponse>('/auth/change-password', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  getMe: () => request<MeResponse>('/auth/me'),
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
    imageUrl?: string;
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
      imageUrl?: string;
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
  update: (id: string, data: UpdateUserRequest) =>
    request<User>(`/users/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
  getUserRoles: (id: string) => request<string[]>(`/users/${id}/roles`),
  updateRoles: (id: string, roleNames: string[]) =>
    request<void>(`/users/${id}/roles`, {
      method: 'PUT',
      body: JSON.stringify({ roleNames }),
    }),
  getUserBorrowRecords: (userId: string) =>
    request<BorrowRecord[]>(`/users/${userId}/borrow-records`),
};

// ─── 6. Roles API ────────────────────────────────────────────────────────────
export const rolesApi = {
  getAll: () => request<Role[]>('/roles'),
};

// ─── 7. Borrow Records API ───────────────────────────────────────────────────
export const borrowRecordsApi = {
  getAll: (status?: string) =>
    request<BorrowRecord[]>(status ? `/borrow-records?status=${encodeURIComponent(status)}` : '/borrow-records'),
  getById: (id: number) => request<BorrowRecord>(`/borrow-records/${id}`),
  getMyBorrowRecords: () => request<BorrowRecord[]>('/my/borrow-records'),
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
  markLost: (
    borrowRecordId: number,
    detailId: number,
    data: { fineAmount: number; note?: string }
  ) =>
    request<BorrowRecord>(`/borrow-records/${borrowRecordId}/details/${detailId}/lost`, {
      method: 'PATCH',
      body: JSON.stringify(data),
    }),
};
