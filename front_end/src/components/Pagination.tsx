interface PaginationProps {
  page: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  disabled?: boolean;
  onPageChange: (page: number) => void;
}

export function Pagination({ page, pageSize, totalPages, totalCount, disabled, onPageChange }: PaginationProps) {
  const first = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const last = Math.min(page * pageSize, totalCount);

  return (
    <nav aria-label="Phân trang sách" className="pagination">
      <span aria-live="polite">Hiển thị {first}–{last} / {totalCount} đầu sách</span>
      <div className="pagination-actions">
        <button type="button" className="btn btn-secondary" disabled={disabled || page <= 1}
          onClick={() => onPageChange(page - 1)}>
          Trang trước
        </button>
        <span>Trang {totalPages === 0 ? 0 : page} / {totalPages}</span>
        <button type="button" className="btn btn-secondary" disabled={disabled || page >= totalPages}
          onClick={() => onPageChange(page + 1)}>
          Trang sau
        </button>
      </div>
    </nav>
  );
}
