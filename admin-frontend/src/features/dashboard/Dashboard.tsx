import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { dashboardApi } from '@/api/admin'
import { formatCurrency, formatDateTime } from '@/lib/utils'
import {
    ShoppingCart,
    Users,
    Package,
    AlertTriangle,
    Clock,
} from 'lucide-react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, BarChart, Bar, Legend } from 'recharts'

type DateRange = 'today' | 'week' | 'month' | 'all'

export default function Dashboard() {
    const navigate = useNavigate()
    const [dateRange, setDateRange] = useState<DateRange>('all')

    const getDateRangeParams = (range: DateRange) => {
        const now = new Date()
        let startDate: Date
        let days = 30

        if (range === 'today') {
            startDate = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0)
            days = 1
        } else if (range === 'week') {
            startDate = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 7, 0, 0, 0, 0)
            days = 7
        } else if (range === 'month') {
            startDate = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 30, 0, 0, 0, 0)
            days = 30
        } else {
            // All Time
            return {
                startDate: undefined,
                endDate: undefined,
                days: 3650
            }
        }

        const endDate = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59, 999)

        // Format as local date string to avoid timezone conversion issues
        // YYYY-MM-DDTHH:mm:ss format without timezone offset
        const formatLocalDate = (d: Date) => {
            const pad = (n: number) => n.toString().padStart(2, '0')
            return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`
        }

        return {
            startDate: formatLocalDate(startDate),
            endDate: formatLocalDate(endDate),
            days
        }
    }

    const { startDate, endDate, days } = getDateRangeParams(dateRange)

    const { data, isLoading } = useQuery({
        queryKey: ['dashboard', dateRange],
        queryFn: () => dashboardApi.getDashboard(startDate),
    })

    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
            </div>
        )
    }

    const summary = data?.summary

    // Dynamic Labels based on filter
    const periodLabel = dateRange === 'today' ? 'Bugün' :
        dateRange === 'week' ? 'Bu Hafta' :
            dateRange === 'month' ? 'Bu Ay' : 'Tüm Zamanlar'

    const stats = [
        {
            name: `${periodLabel} Siparişler`,
            value: summary?.todayOrders || 0,
            change: formatCurrency(summary?.todayRevenue || 0),
            icon: ShoppingCart,
            color: 'bg-blue-500',
            href: startDate ? `/admin/orders?startDate=${startDate}&endDate=${endDate}` : '/admin/orders'
        },
        {
            name: 'Toplam Müşteri',
            value: summary?.totalCustomers || 0,
            change: 'Aktif',
            icon: Users,
            color: 'bg-green-500',
            href: null
        },
        {
            name: 'Toplam Ürün',
            value: summary?.totalProducts || 0,
            change: 'Katalog',
            icon: Package,
            color: 'bg-purple-500',
            href: '/admin/products'
        },
        {
            name: 'Düşük Stok',
            value: summary?.lowStockProducts || 0,
            change: 'Uyarı',
            icon: AlertTriangle,
            color: 'bg-orange-500',
            href: '/admin/products?filter=low-stock'
        },
        {
            name: 'Bekleyen Siparişler',
            value: summary?.pendingOrders || 0,
            change: 'İşlem Gerekli',
            icon: Clock,
            color: 'bg-red-500',
            href: '/admin/orders?status=Pending'
        },
    ]

    return (
        <div className="space-y-6">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
                    <p className="text-gray-600 mt-1">E-Ticaret yönetim paneline hoş geldiniz</p>
                </div>

                {/* Date Filter Tabs */}
                <div className="bg-white p-1 rounded-lg border shadow-sm inline-flex">
                    <button
                        onClick={() => setDateRange('all')}
                        className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${dateRange === 'all'
                            ? 'bg-primary-50 text-primary-700'
                            : 'text-gray-600 hover:text-gray-900 hover:bg-gray-50'
                            }`}
                    >
                        Tümü
                    </button>
                    <button
                        onClick={() => setDateRange('today')}
                        className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${dateRange === 'today'
                            ? 'bg-primary-50 text-primary-700'
                            : 'text-gray-600 hover:text-gray-900 hover:bg-gray-50'
                            }`}
                    >
                        Bugün
                    </button>
                    <button
                        onClick={() => setDateRange('week')}
                        className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${dateRange === 'week'
                            ? 'bg-primary-50 text-primary-700'
                            : 'text-gray-600 hover:text-gray-900 hover:bg-gray-50'
                            }`}
                    >
                        Bu Hafta
                    </button>
                    <button
                        onClick={() => setDateRange('month')}
                        className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${dateRange === 'month'
                            ? 'bg-primary-50 text-primary-700'
                            : 'text-gray-600 hover:text-gray-900 hover:bg-gray-50'
                            }`}
                    >
                        Bu Ay
                    </button>
                </div>
            </div>

            {/* Stats Grid */}
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5">
                {stats.map((stat) => (
                    <div
                        key={stat.name}
                        onClick={() => stat.href && navigate(stat.href)}
                        className={`bg-white rounded-lg shadow p-6 transition ${stat.href ? 'cursor-pointer hover:shadow-lg hover:bg-gray-50' : ''
                            }`}
                    >
                        <div className="flex items-center justify-between">
                            <div className="flex-1">
                                <p className="text-sm font-medium text-gray-600">{stat.name}</p>
                                <p className="text-3xl font-bold text-gray-900 mt-2">{stat.value}</p>
                                <p className="text-sm text-gray-500 mt-1">{stat.change}</p>
                            </div>
                            <div className={`${stat.color} rounded-full p-3`}>
                                <stat.icon className="w-6 h-6 text-white" />
                            </div>
                        </div>
                    </div>
                ))}
            </div>

            {/* Charts Grid */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Sales Chart */}
                <div className="bg-white rounded-lg shadow p-6">
                    <h2 className="text-lg font-semibold text-gray-900 mb-4">
                        {dateRange === 'today' ? 'Saatlik Satışlar (Bugün)' :
                            dateRange === 'week' ? 'Son 7 Günlük Satışlar' : 'Son 30 Günlük Satışlar'}
                    </h2>
                    <div className="h-80">
                        <DashboardChart days={days} />
                    </div>
                </div>

                {/* Top Selling Products */}
                <div className="bg-white rounded-lg shadow p-6">
                    <h2 className="text-lg font-semibold text-gray-900 mb-4">
                        En Çok Satan Ürünler ({periodLabel})
                    </h2>
                    <div className="h-80">
                        <TopProductsChart startDate={startDate} />
                    </div>
                </div>
            </div>

            {/* Recent Orders */}
            <div className="bg-white rounded-lg shadow">
                <div className="px-6 py-4 border-b">
                    <h2 className="text-lg font-semibold text-gray-900">Son Siparişler ({periodLabel})</h2>
                </div>
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                        <thead className="bg-gray-50">
                            <tr>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Sipariş No
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Müşteri
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Tutar
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Durum
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Tarih
                                </th>
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            {data?.recentOrders && data.recentOrders.length > 0 ? (
                                data.recentOrders.map((order) => (
                                    <tr key={order.orderId} className="hover:bg-gray-50">
                                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                                            {order.orderNumber}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                                            {order.customerName}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                                            {formatCurrency(order.totalAmount)}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            {(() => {
                                                const colors: Record<string, string> = {
                                                    'Pending': 'bg-yellow-100 text-yellow-800',
                                                    'Processing': 'bg-blue-100 text-blue-800',
                                                    'Shipped': 'bg-indigo-100 text-indigo-800',
                                                    'Delivered': 'bg-green-100 text-green-800',
                                                    'Cancelled': 'bg-red-100 text-red-800'
                                                }
                                                const labels: Record<string, string> = {
                                                    'Pending': 'Beklemede',
                                                    'Processing': 'İşleniyor',
                                                    'Shipped': 'Kargoda',
                                                    'Delivered': 'Teslim Edildi',
                                                    'Cancelled': 'İptal Edildi'
                                                }
                                                return (
                                                    <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${colors[order.status] || 'bg-gray-100 text-gray-800'}`}>
                                                        {labels[order.status] || order.status}
                                                    </span>
                                                )
                                            })()}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            {formatDateTime(order.createdAt)}
                                        </td>
                                    </tr>
                                ))
                            ) : (
                                <tr>
                                    <td colSpan={5} className="px-6 py-8 text-center text-gray-500">
                                        Henüz sipariş bulunmuyor
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    )
}

function DashboardChart({ days }: { days: number }) {
    const { data: chartData, isLoading } = useQuery({
        queryKey: ['dashboard-chart', days],
        queryFn: () => dashboardApi.getDashboardChart(days),
    })

    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-full">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
            </div>
        )
    }

    if (!chartData?.data || chartData.data.length === 0) {
        return (
            <div className="flex items-center justify-center h-full text-gray-500">
                Veri bulunamadı
            </div>
        )
    }

    return (
        <ResponsiveContainer width="100%" height="100%">
            <LineChart
                data={chartData.data}
                margin={{
                    top: 5,
                    right: 30,
                    left: 20,
                    bottom: 5,
                }}
            >
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis
                    dataKey="date"
                    tick={{ fontSize: 12 }}
                    tickLine={false}
                    axisLine={false}
                />
                <YAxis
                    tick={{ fontSize: 12 }}
                    tickLine={false}
                    axisLine={false}
                    tickFormatter={(value) => `₺${value}`}
                />
                <Tooltip
                    formatter={(value: number) => [`₺${value.toLocaleString('tr-TR', { minimumFractionDigits: 2 })}`, 'Ciro']}
                    labelStyle={{ color: '#374151' }}
                />
                <Line
                    type="monotone"
                    dataKey="totalRevenue"
                    stroke="#4F46E5"
                    strokeWidth={2}
                    dot={false}
                    activeDot={{ r: 8 }}
                    name="Ciro"
                />
            </LineChart>
        </ResponsiveContainer>
    )
}

function TopProductsChart({ startDate }: { startDate?: string }) {
    const { data: topProducts, isLoading } = useQuery({
        queryKey: ['top-selling-products', startDate],
        queryFn: () => dashboardApi.getTopSellingProducts(5, startDate),
    })

    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-full">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
            </div>
        )
    }

    if (!topProducts || topProducts.length === 0) {
        return (
            <div className="flex items-center justify-center h-full text-gray-500">
                Veri bulunamadı
            </div>
        )
    }

    return (
        <ResponsiveContainer width="100%" height="100%">
            <BarChart
                data={topProducts}
                layout="vertical"
                margin={{
                    top: 5,
                    right: 30,
                    left: 40,
                    bottom: 5,
                }}
            >
                <CartesianGrid strokeDasharray="3 3" horizontal={false} />
                <XAxis type="number" hide />
                <YAxis
                    dataKey="productName"
                    type="category"
                    width={150}
                    tick={{ fontSize: 11 }}
                    tickFormatter={(value) => value.length > 20 ? `${value.substring(0, 20)}...` : value}
                />
                <Tooltip
                    cursor={{ fill: 'transparent' }}
                    formatter={(value: number) => [value, 'Satış Adedi']}
                />
                <Legend />
                <Bar dataKey="totalQuantity" name="Satış Adedi" fill="#8884d8" radius={[0, 4, 4, 0]} />
            </BarChart>
        </ResponsiveContainer>
    )
}
