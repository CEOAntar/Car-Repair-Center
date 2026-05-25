import { useState, useRef, useEffect, useCallback } from 'react';
import { Search, X, ChevronDown } from 'lucide-react';

interface SearchableSelectOption {
  value: string;
  label: string;
  sublabel?: string;
}

interface SearchableSelectProps {
  options: SearchableSelectOption[];
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  required?: boolean;
  disabled?: boolean;
}

export default function SearchableSelect({
  options,
  value,
  onChange,
  placeholder = 'بحث واختيار...',
  required = false,
  disabled = false,
}: SearchableSelectProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const containerRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const selectedOption = options.find((o) => o.value === value);

  const filtered = options.filter((o) => {
    const term = searchTerm.toLowerCase();
    return (
      o.label.toLowerCase().includes(term) ||
      (o.sublabel && o.sublabel.toLowerCase().includes(term)) ||
      o.value.toLowerCase().includes(term)
    );
  });

  const handleSelect = useCallback(
    (val: string) => {
      onChange(val);
      setIsOpen(false);
      setSearchTerm('');
    },
    [onChange]
  );

  const handleClear = useCallback(() => {
    onChange('');
    setSearchTerm('');
    setIsOpen(false);
  }, [onChange]);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false);
        setSearchTerm('');
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  // Focus input when dropdown opens
  useEffect(() => {
    if (isOpen && inputRef.current) {
      inputRef.current.focus();
    }
  }, [isOpen]);

  return (
    <div ref={containerRef} className="relative">
      {/* Hidden input for form validation */}
      {required && (
        <input
          tabIndex={-1}
          autoComplete="off"
          style={{ position: 'absolute', opacity: 0, width: 0, height: 0, pointerEvents: 'none' }}
          value={value}
          required={required}
          onChange={() => {}}
        />
      )}

      {/* Display button */}
      <button
        type="button"
        disabled={disabled}
        onClick={() => setIsOpen(!isOpen)}
        className="mk-input w-full flex items-center justify-between gap-2 text-right disabled:opacity-50"
        style={{
          cursor: disabled ? 'not-allowed' : 'pointer',
          minHeight: '42px',
        }}
      >
        <span
          style={{
            color: selectedOption ? 'var(--text-primary)' : 'var(--text-muted)',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
            flex: 1,
          }}
        >
          {selectedOption ? selectedOption.label : placeholder}
        </span>
        <div className="flex items-center gap-1 shrink-0">
          {value && (
            <span
              onClick={(e) => {
                e.stopPropagation();
                handleClear();
              }}
              className="p-0.5 rounded-md transition-colors hover:bg-[var(--bg-secondary)]"
              style={{ color: 'var(--text-muted)', cursor: 'pointer' }}
            >
              <X size={14} />
            </span>
          )}
          <ChevronDown
            size={16}
            style={{
              color: 'var(--text-muted)',
              transform: isOpen ? 'rotate(180deg)' : 'rotate(0deg)',
              transition: 'transform 0.2s ease',
            }}
          />
        </div>
      </button>

      {/* Dropdown */}
      {isOpen && (
        <div
          className="absolute z-50 w-full mt-1 rounded-xl overflow-hidden shadow-2xl"
          style={{
            background: 'var(--bg-card)',
            border: '1px solid var(--border-color)',
            maxHeight: '280px',
            animation: 'slideIn 0.15s ease-out',
          }}
        >
          {/* Search Input */}
          <div
            className="flex items-center gap-2 px-3 py-2.5"
            style={{ borderBottom: '1px solid var(--border-color)', background: 'var(--bg-secondary)' }}
          >
            <Search size={14} style={{ color: 'var(--text-muted)', shrink: 0 }} />
            <input
              ref={inputRef}
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="ابحث بالاسم أو رقم الهاتف..."
              className="w-full bg-transparent outline-none text-sm"
              style={{ color: 'var(--text-primary)' }}
            />
          </div>

          {/* Options List */}
          <div className="overflow-y-auto" style={{ maxHeight: '220px' }}>
            {filtered.length === 0 ? (
              <div className="text-center py-4 text-sm" style={{ color: 'var(--text-muted)' }}>
                لا توجد نتائج مطابقة
              </div>
            ) : (
              filtered.map((option) => (
                <div
                  key={option.value}
                  onClick={() => handleSelect(option.value)}
                  className="px-4 py-2.5 cursor-pointer transition-colors text-sm"
                  style={{
                    background: option.value === value ? 'color-mix(in srgb, var(--color-accent) 10%, transparent)' : 'transparent',
                    borderRight: option.value === value ? '3px solid var(--color-accent)' : '3px solid transparent',
                  }}
                  onMouseEnter={(e) => {
                    if (option.value !== value) {
                      e.currentTarget.style.background = 'var(--bg-secondary)';
                    }
                  }}
                  onMouseLeave={(e) => {
                    if (option.value !== value) {
                      e.currentTarget.style.background = 'transparent';
                    }
                  }}
                >
                  <span className="font-semibold" style={{ color: 'var(--text-primary)' }}>
                    {option.label}
                  </span>
                  {option.sublabel && (
                    <span className="block text-xs mt-0.5" style={{ color: 'var(--text-muted)' }}>
                      {option.sublabel}
                    </span>
                  )}
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
}
