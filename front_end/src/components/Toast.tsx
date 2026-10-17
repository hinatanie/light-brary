import React from 'react';
import { CheckCircle2, AlertCircle, X } from 'lucide-react';

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'info';
  text: string;
}

interface ToastProps {
  toasts: ToastMessage[];
  onDismiss: (id: string) => void;
}

export const ToastContainer: React.FC<ToastProps> = ({ toasts, onDismiss }) => {
  return (
    <div className="toast-container" aria-live="polite">
      {toasts.map(toast => (
        <div key={toast.id} className={`toast-message toast-${toast.type}`}>
          {toast.type === 'success' ? <CheckCircle2 size={18} className="toast-icon" /> : <AlertCircle size={18} className="toast-icon" />}
          <span>{toast.text}</span>
          <button onClick={() => onDismiss(toast.id)} className="toast-dismiss" aria-label="Đóng thông báo">
            <X size={16} />
          </button>
        </div>
      ))}
    </div>
  );
};
