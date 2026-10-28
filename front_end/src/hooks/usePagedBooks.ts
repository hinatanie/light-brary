import { useCallback, useEffect, useState } from 'react';
import { booksApi, categoriesApi } from '../api';
import type { Book, BookPageQuery, BookSort, Category, PagedResponse } from '../types';
import { useToast } from '../contexts/ToastContext';

export function usePagedBooks(sortBy: BookSort = 'newest', enabled = true) {
  const { showToast } = useToast();
  const [query, setQuery] = useState<BookPageQuery>({ page: 1, sortBy });
  const [result, setResult] = useState<PagedResponse<Book>>({
    items: [], pageNumber: 1, pageSize: 10, totalCount: 0, totalPages: 0,
  });
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [revision, setRevision] = useState(0);

  useEffect(() => {
    if (!enabled) return;
    const controller = new AbortController();
    categoriesApi.getAll(controller.signal).then(setCategories).catch((err: unknown) => {
      if (!controller.signal.aborted) {
        showToast(err instanceof Error ? err.message : 'Lỗi khi tải thể loại sách', 'error');
      }
    });
    return () => controller.abort();
  }, [enabled, showToast]);

  useEffect(() => {
    if (!enabled) return;
    const controller = new AbortController();
    const load = async () => {
      setLoading(true);
      setError('');
      try {
        const response = await booksApi.getPaged(query, controller.signal);
        if (controller.signal.aborted) return;
        // A deletion on the last page may reduce the total number of pages.
        const lastPage = Math.max(1, response.totalPages);
        if (query.page > lastPage) {
          setQuery(current => ({ ...current, page: lastPage }));
          return;
        }
        setResult(response);
      } catch (err) {
        if (controller.signal.aborted) return;
        const message = err instanceof Error ? err.message : 'Lỗi khi tải danh sách sách';
        setError(message);
        showToast(message, 'error');
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    };
    void load();
    return () => controller.abort();
  }, [enabled, query, revision, showToast]);

  const setFilters = useCallback((filters: Partial<Omit<BookPageQuery, 'page'>>) => {
    setQuery(current => ({ ...current, ...filters, page: 1 }));
  }, []);
  const setPage = useCallback((page: number) => {
    setQuery(current => ({ ...current, page }));
  }, []);
  const refresh = useCallback(() => setRevision(current => current + 1), []);

  return { ...result, query, categories, loading, error, setFilters, setPage, refresh };
}
