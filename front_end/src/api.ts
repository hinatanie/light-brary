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

const API_BASE = '/api';

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
    credentials: 'include',
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
  login: async (data: { userName: string; password: string }): Promise<AuthResponse> => {
    const raw = await request<any>('/auth/login', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    const token = raw.accessToken || raw.token;
    const user = raw.user || raw;
    const roles = Array.isArray(user.roles) && user.roles.length > 0 ? user.roles : ['Member'];

    return {
      ...raw,
      token,
      accessToken: token,
      userId: String(user.id ?? user.userId ?? ''),
      fullName: user.fullName || '',
      phoneNumber: user.phoneNumber || '',
      email: user.email || '',
      roles,
      mustChangePassword: Boolean(user.mustChangePassword),
      user: {
        id: user.id ?? user.userId ?? '',
        fullName: user.fullName || '',
        phoneNumber: user.phoneNumber || '',
        address: user.address || null,
        roles,
        createdAt: user.createdAt || new Date().toISOString(),
      },
    };
  },

  register: async (data: {
    fullName: string;
    email?: string;
    password: string;
    phoneNumber: string;
    address?: string;
  }): Promise<AuthResponse> => {
    const raw = await request<any>('/auth/register', {
      method: 'POST',
      body: JSON.stringify({
        fullName: data.fullName,
        phoneNumber: data.phoneNumber,
        password: data.password,
        address: data.address,
      }),
    });
    const token = raw.accessToken || raw.token;
    const user = raw.user || raw;
    const roles = Array.isArray(user.roles) && user.roles.length > 0 ? user.roles : ['Member'];

    return {
      ...raw,
      token,
      accessToken: token,
      userId: String(user.id ?? user.userId ?? ''),
      fullName: user.fullName || '',
      phoneNumber: user.phoneNumber || '',
      email: user.email || '',
      roles,
      mustChangePassword: Boolean(user.mustChangePassword),
      user: {
        id: user.id ?? user.userId ?? '',
        fullName: user.fullName || '',
        phoneNumber: user.phoneNumber || '',
        address: user.address || null,
        roles,
        createdAt: user.createdAt || new Date().toISOString(),
      },
    };
  },

  logout: () => {
    localStorage.removeItem('lightbrary_token');
    localStorage.removeItem('lightbrary_user');
    return Promise.resolve();
  },

  changePassword: (data: { currentPassword: string; newPassword: string }) =>
    request<{ message: string }>('/me/password', {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  getMe: async (): Promise<MeResponse> => {
    const raw = await request<any>('/me');
    const roles = Array.isArray(raw.roles) && raw.roles.length > 0 ? raw.roles : ['Member'];
    return {
      userId: String(raw.id ?? raw.userId ?? ''),
      id: raw.id,
      fullName: raw.fullName || '',
      phoneNumber: raw.phoneNumber || '',
      email: raw.email || '',
      address: raw.address || null,
      roles,
      mustChangePassword: Boolean(raw.mustChangePassword),
    };
  },
};

// ─── 2. Categories API ───────────────────────────────────────────────────────
export const categoriesApi = {
  getAll: async (): Promise<Category[]> => {
    const list = await request<Category[]>('/categories');
    return list.map((c) => ({
      ...c,
      bookCount: c.bookCount ?? 0,
    }));
  },
  getById: (id: string | number) => request<Category>(`/categories/${id}`),
  create: (data: { name: string; description?: string }) =>
    request<Category>('/categories', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  update: (id: string | number, data: { name: string; description?: string }) =>
    request<Category>(`/categories/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
  delete: (id: string | number) =>
    request<void>(`/categories/${id}`, {
      method: 'DELETE',
    }),
};

// ─── 3. Books API ────────────────────────────────────────────────────────────
export const booksApi = {
  getAll: (
    filterOrKeyword?:
      | string
      | { keyword?: string; categoryId?: number | string; author?: string; onlyAvailable?: boolean },
    categoryId?: number,
    author?: string,
    onlyAvailable?: boolean
  ) => {
    const params = new URLSearchParams();
    if (typeof filterOrKeyword === 'object' && filterOrKeyword !== null) {
      if (filterOrKeyword.keyword) params.append('keyword', filterOrKeyword.keyword);
      if (filterOrKeyword.categoryId) params.append('categoryId', filterOrKeyword.categoryId.toString());
      if (filterOrKeyword.author) params.append('author', filterOrKeyword.author);
      if (filterOrKeyword.onlyAvailable !== undefined && filterOrKeyword.onlyAvailable !== null) {
        params.append('onlyAvailable', filterOrKeyword.onlyAvailable.toString());
      }
    } else {
      if (filterOrKeyword) params.append('keyword', filterOrKeyword);
      if (categoryId) params.append('categoryId', categoryId.toString());
      if (author) params.append('author', author);
      if (onlyAvailable !== undefined && onlyAvailable !== null) {
        params.append('onlyAvailable', onlyAvailable.toString());
      }
    }
    const qs = params.toString();
    return request<Book[]>(qs ? `/books?${qs}` : '/books');
  },
  getById: (id: string | number) => request<Book>(`/books/${id}`),
  create: (data: {
    title: string;
    author: string;
    isbn?: string;
    publishYear?: number;
    imageUrl?: string;
    categoryId: string | number;
  }) =>
    request<Book>('/books', {
      method: 'POST',
      body: JSON.stringify({
        title: data.title,
        author: data.author,
        isbn: data.isbn?.trim() || `978-${Math.floor(1000000000 + Math.random() * 9000000000)}`,
        publishYear: data.publishYear,
        imageUrl: data.imageUrl?.trim() || undefined,
        categoryId: data.categoryId,
      }),
    }),
  update: (
    id: string | number,
    data: {
      title: string;
      author: string;
      isbn?: string;
      publishYear?: number;
      imageUrl?: string;
      categoryId: string | number;
    }
  ) =>
    request<Book>(`/books/${id}`, {
      method: 'PUT',
      body: JSON.stringify({
        title: data.title,
        author: data.author,
        isbn: data.isbn?.trim() || `978-${Math.floor(1000000000 + Math.random() * 9000000000)}`,
        publishYear: data.publishYear,
        imageUrl: data.imageUrl?.trim() || undefined,
        categoryId: data.categoryId,
      }),
    }),
  delete: (id: string | number) =>
    request<void>(`/books/${id}`, {
      method: 'DELETE',
    }),
};

// ─── 4. Book Copies API ──────────────────────────────────────────────────────
export const bookCopiesApi = {
  getByBookId: (bookId: string | number) => request<BookCopy[]>(`/books/${bookId}/copies`),
  getById: (id: string | number) => request<BookCopy>(`/book-copies/${id}`),
  getByBarcode: (barcode: string) => request<BookCopy>(`/book-copies/by-barcode/${barcode}`),
  create: (data: { bookId: string | number; barcode: string; conditionNote?: string }) =>
    request<BookCopy>(`/books/${data.bookId}/copies`, {
      method: 'POST',
      body: JSON.stringify({
        barcode: data.barcode,
        conditionNote: data.conditionNote,
      }),
    }),
  update: (id: string | number, data: { barcode: string; conditionNote?: string }) =>
    request<BookCopy>(`/book-copies/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
  updateCondition: async (id: string | number, data: { barcode?: string; conditionNote?: string }) => {
    let barcode = data.barcode;
    if (!barcode) {
      const copy = await request<BookCopy>(`/book-copies/${id}`);
      barcode = copy.barcode;
    }
    return request<BookCopy>(`/book-copies/${id}`, {
      method: 'PUT',
      body: JSON.stringify({
        barcode,
        conditionNote: data.conditionNote,
      }),
    });
  },
  delete: (id: string | number) =>
    request<void>(`/book-copies/${id}`, {
      method: 'DELETE',
    }),
};

const isRoleMatch = (r1: string, r2: string): boolean => {
  const a = r1.trim().toLowerCase();
  const b = r2.trim().toLowerCase();
  if (a === b) return true;
  if ((a === 'staff' || a === 'librarian') && (b === 'staff' || b === 'librarian')) return true;
  if ((a === 'member' || a === 'user') && (b === 'member' || b === 'user')) return true;
  return false;
};

// ─── 5. Users API ────────────────────────────────────────────────────────────
export const usersApi = {
  getAll: (keyword?: string, role?: string) => {
    const params = new URLSearchParams();
    if (keyword) params.append('keyword', keyword);
    if (role) params.append('role', role);
    const qs = params.toString();
    return request<User[]>(qs ? `/users?${qs}` : '/users');
  },
  getById: (id: string | number) => request<User>(`/users/${id}`),
  create: async (data: {
    fullName: string;
    phoneNumber: string;
    password: string;
    address?: string;
    roles?: string[];
  }): Promise<User> => {
    const user = await request<User>('/users', {
      method: 'POST',
      body: JSON.stringify({
        fullName: data.fullName.trim(),
        phoneNumber: data.phoneNumber.trim(),
        password: data.password,
        address: data.address?.trim() || null,
      }),
    });

    if (data.roles && data.roles.length > 0) {
      try {
        await usersApi.updateRoles(user.id, data.roles);
      } catch (err) {
        console.warn('Failed to assign initial roles:', err);
      }
    }

    return user;
  },
  update: (id: string | number, data: UpdateUserRequest) =>
    request<User>(`/users/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }).catch(() =>
      request<User>('/me', {
        method: 'PUT',
        body: JSON.stringify(data),
      })
    ),
  delete: (id: string | number) =>
    request<void>(`/users/${id}`, {
      method: 'DELETE',
    }),
  getUserRoles: async (id: string | number): Promise<string[]> => {
    const roles = await request<Role[]>(`/users/${id}/roles`);
    return roles.map((r) => r.name);
  },
  updateRoles: async (id: string | number, roleNames: string[]) => {
    const [allRoles, currentRoles] = await Promise.all([
      rolesApi.getAll(),
      request<Role[]>(`/users/${id}/roles`).catch(() => [] as Role[]),
    ]);

    // Gán role mới
    for (const rName of roleNames) {
      const alreadyHas = currentRoles.some((cr) => isRoleMatch(cr.name, rName));
      if (!alreadyHas) {
        const found = allRoles.find((x) => isRoleMatch(x.name, rName));
        if (found) {
          await request<void>(`/users/${id}/roles/${found.id}`, { method: 'POST' });
        }
      }
    }

    // Gỡ role cũ
    for (const cr of currentRoles) {
      const shouldKeep = roleNames.some((rn) => isRoleMatch(rn, cr.name));
      if (!shouldKeep) {
        await request<void>(`/users/${id}/roles/${cr.id}`, { method: 'DELETE' });
      }
    }
  },
  getUserBorrowRecords: (userId: string | number) =>
    request<BorrowRecord[]>(`/users/${userId}/borrow-records`),
  updatePassword: (id: string | number, newPassword: string) =>
    request<{ message: string }>(`/users/${id}/password`, {
      method: 'PUT',
      body: JSON.stringify({ newPassword }),
    }),
};

// ─── 6. Roles API ────────────────────────────────────────────────────────────
export const rolesApi = {
  getAll: () => request<Role[]>('/roles'),
};

// ─── 7. Borrow Records API ───────────────────────────────────────────────────
export const borrowRecordsApi = {
  getAll: async (status?: string): Promise<BorrowRecord[]> => {
    const rawList = await request<any[]>(
      status ? `/borrow-records?status=${encodeURIComponent(status)}` : '/borrow-records'
    );
    return rawList.map((r) => ({
      ...r,
      userFullName: r.user?.fullName || r.userFullName || 'Độc giả',
      details: r.details || [],
      totalBooks: r.totalBooks ?? (r.details ? r.details.length : 0),
    }));
  },
  getById: (id: string | number) => request<BorrowRecord>(`/borrow-records/${id}`),
  getMyBorrowRecords: async (): Promise<BorrowRecord[]> => {
    const rawList = await request<any[]>('/me/borrow-records');
    return rawList.map((r) => ({
      ...r,
      userFullName: r.user?.fullName || r.userFullName || 'Tôi',
      details: r.details || [],
      totalBooks: r.totalBooks ?? (r.details ? r.details.length : 0),
    }));
  },
  create: (data: {
    userId: string | number;
    dueDate: string;
    bookCopyIds: (string | number)[];
    note?: string;
  }) =>
    request<BorrowRecord>('/borrow-records', {
      method: 'POST',
      body: JSON.stringify({
        userId: data.userId,
        dueDate: data.dueDate,
        bookCopyIds: data.bookCopyIds,
        note: data.note,
      }),
    }),
  borrowBookForMember: (data: {
    bookId: string | number;
    bookCopyId?: string | number;
    dueDate?: string;
    note?: string;
  }) =>
    request<BorrowRecord>('/me/borrow', {
      method: 'POST',
      body: JSON.stringify({
        bookId: data.bookId,
        bookCopyId: data.bookCopyId || undefined,
        dueDate: data.dueDate,
        note: data.note,
      }),
    }),
  returnBooks: (
    _borrowRecordId: string | number,
    data: { items: { borrowDetailId: string | number }[] }
  ) =>
    request<any>('/returns', {
      method: 'POST',
      body: JSON.stringify({
        borrowDetailIds: data.items.map((i) => i.borrowDetailId),
      }),
    }),
  markLost: (
    _borrowRecordId: string | number,
    detailId: string | number,
    _data?: { note?: string }
  ) =>
    request<any>(`/borrow-details/${detailId}/mark-lost`, {
      method: 'POST',
    }),
};
