import { useToastStore } from '../../store/toastStore';
import { CheckCircle, AlertTriangle, Info, X } from 'lucide-react';

const icons = {
  success: <CheckCircle size={18} className="shrink-0" style={{ color: 'var(--color-success)' }} />,
  error: <AlertTriangle size={18} className="shrink-0" style={{ color: 'var(--color-danger)' }} />,
  warning: <AlertTriangle size={18} className="shrink-0" style={{ color: 'var(--color-warning)' }} />,
  info: <Info size={18} className="shrink-0" style={{ color: 'var(--color-info)' }} />,
};

const borderColors = {
  success: 'var(--color-success)',
  error: 'var(--color-danger)',
  warning: 'var(--color-warning)',
  info: 'var(--color-info)',
};

export default function ToastContainer() {
  const { toasts, removeToast } = useToastStore();

  if (toasts.length === 0) return null;

  return (
    <div className="fixed bottom-6 left-6 z-50 flex flex-col gap-3 pointer-events-none max-w-sm w-full no-print">
      {toasts.map((toast) => (
        <div
          key={toast.id}
          className="flex items-center justify-between p-4 rounded-xl shadow-lg border backdrop-blur-md pointer-events-auto transition-all animate-slide-in"
          style={{
            background: 'color-mix(in srgb, var(--bg-card) 85%, transparent)',
            borderColor: borderColors[toast.type] || 'var(--border-color)',
          }}
        >
          <div className="flex items-center gap-3 min-w-0">
            {icons[toast.type]}
            <p className="text-xs font-semibold" style={{ color: 'var(--text-primary)' }}>
              {toast.message}
            </p>
          </div>
          <button
            onClick={() => removeToast(toast.id)}
            className="p-1 rounded-lg hover:bg-black/5 dark:hover:bg-white/5 transition-colors shrink-0 cursor-pointer"
            style={{ color: 'var(--text-muted)' }}
          >
            <X size={14} />
          </button>
        </div>
      ))}
    </div>
  );
}
