import { useEffect, useState } from 'react';
import {
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
  PieChart, Pie, Cell,
  BarChart, Bar,
} from 'recharts';
import {
  TrendingUp, TrendingDown, DollarSign, Clock, Percent, UserPlus,
  Activity, BarChart3,
} from 'lucide-react';
import api from '../../services/api';

const formatCurrency = (val: number) => `${val.toLocaleString('ar-EG')} ج.م`;

const methodLabels: Record<string, string> = {
  Cash: 'نقدي',
  Visa: 'فيزا / كارت',
  VodafoneCash: 'فودافون كاش',
  InstaPay: 'انستا باي',
};

const PIE_COLORS = ['#22c55e', '#3b82f6', '#f59e0b', '#a855f7', '#f43f5e', '#06b6d4'];

const statusLabels: Record<string, { label: string; color: string }> = {
  Waiting: { label: 'في الانتظار', color: '#38bdf8' },
  InProgress: { label: 'قيد التنفيذ', color: '#fbbf24' },
  Done: { label: 'مكتمل', color: '#22c55e' },
  Delivered: { label: 'تم التسليم', color: '#94a3b8' },
};

interface KpisData {
  avgOrderValue: number;
  avgCompletionHours: number;
  collectionRate: number;
  newCustomersThisMonth: number;
  ordersThisMonth: number;
  ordersLastMonth: number;
  revenueThisMonth: number;
  revenueLastMonth: number;
}

interface RevenueTrendItem {
  date: string;
  dateFull: string;
  revenue: number;
  count: number;
}

interface PaymentDistItem {
  method: string;
  amount: number;
  count: number;
  percentage: number;
}

interface TopServiceItem {
  serviceName: string;
  timesUsed: number;
  totalRevenue: number;
}

interface OrderStatusItem {
  status: string;
  count: number;
}

export default function AnalyticsPage() {
  const [kpis, setKpis] = useState<KpisData | null>(null);
  const [revenueTrend, setRevenueTrend] = useState<RevenueTrendItem[]>([]);
  const [paymentDist, setPaymentDist] = useState<PaymentDistItem[]>([]);
  const [topServices, setTopServices] = useState<TopServiceItem[]>([]);
  const [orderStatus, setOrderStatus] = useState<OrderStatusItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      api.get('/analytics/kpis'),
      api.get('/analytics/revenue-trend?days=14'),
      api.get('/analytics/payment-distribution?days=30'),
      api.get('/analytics/top-services?limit=6'),
      api.get('/analytics/orders-status'),
    ]).then(([kRes, rRes, pRes, sRes, oRes]) => {
      setKpis(kRes.data);
      setRevenueTrend(rRes.data);
      setPaymentDist(pRes.data);
      setTopServices(sRes.data);
      setOrderStatus(oRes.data);
      setLoading(false);
    }).catch(() => setLoading(false));
  }, []);

  if (loading) return (
    <div className="flex items-center justify-center h-64" style={{ color: 'var(--text-muted)' }}>
      <svg className="animate-spin h-6 w-6 mr-2" viewBox="0 0 24 24">
        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
      </svg>
      جاري تحميل التحليلات...
    </div>
  );

  const revenueChange = kpis && kpis.revenueLastMonth > 0
    ? Math.round(((kpis.revenueThisMonth - kpis.revenueLastMonth) / kpis.revenueLastMonth) * 100)
    : 0;
  const ordersChange = kpis && kpis.ordersLastMonth > 0
    ? Math.round(((kpis.ordersThisMonth - kpis.ordersLastMonth) / kpis.ordersLastMonth) * 100)
    : 0;

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Activity size={24} style={{ color: 'var(--color-accent)' }} />
        <h2 className="text-2xl font-display font-bold" style={{ color: 'var(--text-primary)' }}>
          التحليلات المتقدمة
        </h2>
      </div>

      {/* KPI Cards */}
      {kpis && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 animate-stagger">
          {/* Avg Order Value */}
          <div className="mk-card diagonal-stripe p-5">
            <div className="flex items-center justify-between mb-3">
              <span className="text-xs font-medium" style={{ color: 'var(--text-muted)' }}>متوسط قيمة الأمر</span>
              <div className="w-9 h-9 rounded-lg flex items-center justify-center" style={{ background: 'color-mix(in srgb, #22c55e 12%, transparent)' }}>
                <DollarSign size={18} style={{ color: '#22c55e' }} />
              </div>
            </div>
            <p className="text-2xl font-bold font-mono-code" style={{ color: 'var(--text-primary)' }}>
              {formatCurrency(kpis.avgOrderValue)}
            </p>
          </div>

          {/* Avg Completion Time */}
          <div className="mk-card diagonal-stripe p-5">
            <div className="flex items-center justify-between mb-3">
              <span className="text-xs font-medium" style={{ color: 'var(--text-muted)' }}>متوسط وقت الإنجاز</span>
              <div className="w-9 h-9 rounded-lg flex items-center justify-center" style={{ background: 'color-mix(in srgb, #3b82f6 12%, transparent)' }}>
                <Clock size={18} style={{ color: '#3b82f6' }} />
              </div>
            </div>
            <p className="text-2xl font-bold font-mono-code" style={{ color: 'var(--text-primary)' }}>
              {kpis.avgCompletionHours} <span className="text-sm font-normal" style={{ color: 'var(--text-muted)' }}>ساعة</span>
            </p>
          </div>

          {/* Collection Rate */}
          <div className="mk-card diagonal-stripe p-5">
            <div className="flex items-center justify-between mb-3">
              <span className="text-xs font-medium" style={{ color: 'var(--text-muted)' }}>نسبة التحصيل</span>
              <div className="w-9 h-9 rounded-lg flex items-center justify-center" style={{ background: 'color-mix(in srgb, #f59e0b 12%, transparent)' }}>
                <Percent size={18} style={{ color: '#f59e0b' }} />
              </div>
            </div>
            <p className="text-2xl font-bold font-mono-code" style={{ color: kpis.collectionRate >= 80 ? '#22c55e' : '#f43f5e' }}>
              {kpis.collectionRate}%
            </p>
          </div>

          {/* New Customers */}
          <div className="mk-card diagonal-stripe p-5">
            <div className="flex items-center justify-between mb-3">
              <span className="text-xs font-medium" style={{ color: 'var(--text-muted)' }}>عملاء جدد هذا الشهر</span>
              <div className="w-9 h-9 rounded-lg flex items-center justify-center" style={{ background: 'color-mix(in srgb, #a855f7 12%, transparent)' }}>
                <UserPlus size={18} style={{ color: '#a855f7' }} />
              </div>
            </div>
            <p className="text-2xl font-bold font-mono-code" style={{ color: 'var(--text-primary)' }}>
              {kpis.newCustomersThisMonth} <span className="text-sm font-normal" style={{ color: 'var(--text-muted)' }}>عميل</span>
            </p>
          </div>
        </div>
      )}

      {/* Month Comparison Row */}
      {kpis && (
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div className="mk-card p-5 flex items-center justify-between">
            <div>
              <span className="text-xs font-medium" style={{ color: 'var(--text-muted)' }}>إيرادات هذا الشهر</span>
              <p className="text-xl font-bold font-mono-code mt-1" style={{ color: 'var(--text-primary)' }}>
                {formatCurrency(kpis.revenueThisMonth)}
              </p>
            </div>
            <div className="flex items-center gap-1.5 px-3 py-1.5 rounded-full text-sm font-bold"
              style={{
                background: revenueChange >= 0 ? 'color-mix(in srgb, #22c55e 12%, transparent)' : 'color-mix(in srgb, #f43f5e 12%, transparent)',
                color: revenueChange >= 0 ? '#22c55e' : '#f43f5e',
              }}>
              {revenueChange >= 0 ? <TrendingUp size={16} /> : <TrendingDown size={16} />}
              {revenueChange >= 0 ? '+' : ''}{revenueChange}%
            </div>
          </div>
          <div className="mk-card p-5 flex items-center justify-between">
            <div>
              <span className="text-xs font-medium" style={{ color: 'var(--text-muted)' }}>أوامر هذا الشهر</span>
              <p className="text-xl font-bold font-mono-code mt-1" style={{ color: 'var(--text-primary)' }}>
                {kpis.ordersThisMonth} <span className="text-sm font-normal" style={{ color: 'var(--text-muted)' }}>أمر</span>
              </p>
            </div>
            <div className="flex items-center gap-1.5 px-3 py-1.5 rounded-full text-sm font-bold"
              style={{
                background: ordersChange >= 0 ? 'color-mix(in srgb, #22c55e 12%, transparent)' : 'color-mix(in srgb, #f43f5e 12%, transparent)',
                color: ordersChange >= 0 ? '#22c55e' : '#f43f5e',
              }}>
              {ordersChange >= 0 ? <TrendingUp size={16} /> : <TrendingDown size={16} />}
              {ordersChange >= 0 ? '+' : ''}{ordersChange}%
            </div>
          </div>
        </div>
      )}

      {/* Revenue Trend Chart + Payment Distribution */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Revenue Trend — takes 2/3 */}
        <div className="mk-card p-6 lg:col-span-2">
          <div className="flex items-center gap-2 mb-6">
            <BarChart3 size={18} style={{ color: 'var(--color-accent)' }} />
            <h3 className="text-base font-display font-bold" style={{ color: 'var(--text-primary)' }}>اتجاه الإيرادات (آخر 14 يوم)</h3>
          </div>
          <div style={{ width: '100%', height: 280, direction: 'ltr' }}>
            <ResponsiveContainer>
              <LineChart data={revenueTrend}>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--border-color)" />
                <XAxis dataKey="date" stroke="var(--text-muted)" tick={{ fontSize: 11 }} />
                <YAxis stroke="var(--text-muted)" tick={{ fontSize: 11 }} tickFormatter={(v) => `${(v / 1000).toFixed(0)}k`} />
                <Tooltip
                  contentStyle={{
                    background: 'var(--bg-card)',
                    border: '1px solid var(--border-color)',
                    borderRadius: '12px',
                    color: 'var(--text-primary)',
                    fontSize: '13px',
                    direction: 'rtl',
                  }}
                  formatter={(value: any) => [formatCurrency(Number(value)), 'الإيراد']}
                  labelFormatter={(label) => `التاريخ: ${label}`}
                />
                <Line
                  type="monotone"
                  dataKey="revenue"
                  stroke="#22c55e"
                  strokeWidth={2.5}
                  dot={{ fill: '#22c55e', r: 4, strokeWidth: 0 }}
                  activeDot={{ r: 6, fill: '#22c55e', stroke: 'white', strokeWidth: 2 }}
                />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Payment Distribution Donut — takes 1/3 */}
        <div className="mk-card p-6">
          <h3 className="text-base font-display font-bold mb-4" style={{ color: 'var(--text-primary)' }}>توزيع طرق الدفع</h3>
          {paymentDist.length === 0 ? (
            <p className="text-center py-8 text-sm" style={{ color: 'var(--text-muted)' }}>لا توجد مدفوعات</p>
          ) : (
            <>
              <div style={{ width: '100%', height: 200, direction: 'ltr' }}>
                <ResponsiveContainer>
                  <PieChart>
                    <Pie
                      data={paymentDist}
                      cx="50%"
                      cy="50%"
                      innerRadius={50}
                      outerRadius={80}
                      paddingAngle={3}
                      dataKey="amount"
                      nameKey="method"
                    >
                      {paymentDist.map((_, index) => (
                        <Cell key={`cell-${index}`} fill={PIE_COLORS[index % PIE_COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip
                      contentStyle={{
                        background: 'var(--bg-card)',
                        border: '1px solid var(--border-color)',
                        borderRadius: '12px',
                        color: 'var(--text-primary)',
                        fontSize: '12px',
                        direction: 'rtl',
                      }}
                      formatter={(value: any, name: any) => [formatCurrency(Number(value)), methodLabels[String(name)] || String(name)]}
                    />
                  </PieChart>
                </ResponsiveContainer>
              </div>
              <div className="space-y-2 mt-2">
                {paymentDist.map((item, i) => (
                  <div key={item.method} className="flex items-center justify-between text-sm">
                    <div className="flex items-center gap-2">
                      <div className="w-3 h-3 rounded-full" style={{ background: PIE_COLORS[i % PIE_COLORS.length] }} />
                      <span style={{ color: 'var(--text-secondary)' }}>{methodLabels[item.method] || item.method}</span>
                    </div>
                    <span className="font-mono-code font-semibold" style={{ color: 'var(--text-primary)' }}>{item.percentage}%</span>
                  </div>
                ))}
              </div>
            </>
          )}
        </div>
      </div>

      {/* Top Services + Orders Status */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Top Services */}
        <div className="mk-card p-6">
          <h3 className="text-base font-display font-bold mb-4" style={{ color: 'var(--text-primary)' }}> أكثر الخدمات طلباً</h3>
          {topServices.length === 0 ? (
            <p className="text-center py-8 text-sm" style={{ color: 'var(--text-muted)' }}>لا توجد بيانات</p>
          ) : (
            <div style={{ width: '100%', height: 260, direction: 'ltr' }}>
              <ResponsiveContainer>
                <BarChart data={topServices} layout="vertical" margin={{ left: 20, right: 20 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--border-color)" horizontal={false} />
                  <XAxis type="number" stroke="var(--text-muted)" tick={{ fontSize: 11 }} />
                  <YAxis
                    type="category"
                    dataKey="serviceName"
                    stroke="var(--text-muted)"
                    tick={{ fontSize: 11, fill: 'var(--text-secondary)' }}
                    width={110}
                  />
                  <Tooltip
                    contentStyle={{
                      background: 'var(--bg-card)',
                      border: '1px solid var(--border-color)',
                      borderRadius: '12px',
                      color: 'var(--text-primary)',
                      fontSize: '12px',
                      direction: 'rtl',
                    }}
                    formatter={(value: any, name: any) => {
                      if (String(name) === 'timesUsed') return [Number(value), 'عدد المرات'];
                      return [formatCurrency(Number(value)), 'الإيراد'];
                    }}
                  />
                  <Bar dataKey="timesUsed" fill="#3b82f6" radius={[0, 6, 6, 0]} barSize={20} name="timesUsed" />
                </BarChart>
              </ResponsiveContainer>
            </div>
          )}
        </div>

        {/* Orders Status */}
        <div className="mk-card p-6">
          <h3 className="text-base font-display font-bold mb-4" style={{ color: 'var(--text-primary)' }}> حالات الأوامر الحالية</h3>
          {orderStatus.length === 0 ? (
            <p className="text-center py-8 text-sm" style={{ color: 'var(--text-muted)' }}>لا توجد أوامر</p>
          ) : (
            <>
              <div style={{ width: '100%', height: 200, direction: 'ltr' }}>
                <ResponsiveContainer>
                  <BarChart data={orderStatus}>
                    <CartesianGrid strokeDasharray="3 3" stroke="var(--border-color)" />
                    <XAxis
                      dataKey="status"
                      stroke="var(--text-muted)"
                      tick={{ fontSize: 11 }}
                      tickFormatter={(val) => statusLabels[val]?.label || val}
                    />
                    <YAxis stroke="var(--text-muted)" tick={{ fontSize: 11 }} allowDecimals={false} />
                    <Tooltip
                      contentStyle={{
                        background: 'var(--bg-card)',
                        border: '1px solid var(--border-color)',
                        borderRadius: '12px',
                        color: 'var(--text-primary)',
                        fontSize: '12px',
                        direction: 'rtl',
                      }}
                      formatter={(value: any) => [Number(value), 'عدد الأوامر']}
                      labelFormatter={(label) => statusLabels[label]?.label || label}
                    />
                    <Bar dataKey="count" barSize={40} radius={[8, 8, 0, 0]}>
                      {orderStatus.map((entry) => (
                        <Cell key={entry.status} fill={statusLabels[entry.status]?.color || '#94a3b8'} />
                      ))}
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
              </div>
              <div className="grid grid-cols-2 gap-3 mt-4">
                {orderStatus.map((entry) => {
                  const info = statusLabels[entry.status] || { label: entry.status, color: '#94a3b8' };
                  return (
                    <div key={entry.status} className="flex items-center gap-2 p-2.5 rounded-lg" style={{ background: 'var(--bg-input)', border: '1px solid var(--border-color)' }}>
                      <div className="w-3 h-3 rounded-full" style={{ background: info.color }} />
                      <span className="text-xs" style={{ color: 'var(--text-secondary)' }}>{info.label}</span>
                      <span className="text-sm font-bold font-mono-code mr-auto" style={{ color: 'var(--text-primary)' }}>{entry.count}</span>
                    </div>
                  );
                })}
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
