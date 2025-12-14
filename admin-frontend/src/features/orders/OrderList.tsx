import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link, useSearchParams } from 'react-router-dom'
import { ordersApi } from '@/api/admin'
import { formatCurrency, formatDateTime, formatDate } from '@/lib/utils'
import { Search, Eye, Filter } from 'lucide-react'

const statusOptions = [
    { value: '', label: 'Tümü' },
    { value: 'Pending', label: 'Beklemede' },
    { value: 'Processing', label: 'İşleniyor' },
    { value: 'Shipped', label: 'Kargoda' },
    { value: 'Delivered', label: 'Teslim Edildi' },
    { value: 'Cancelled', label: 'İptal' },
]

export default function OrderList() {
    const [searchParams, setSearchParams] = useSearchParams()
    const [page, setPage] = useState(1)
    const [searchTerm, setSearchTerm] = useState('')
    const [status, setStatus] = useState(searchParams.get('status') || '')
    const [startDate, setStartDate] = useState(searchParams.get('startDate')?.split('T')[0] || '')
    const [endDate, setEndDate] = useState(searchParams.get('endDate')?.split('T')[0] || '')
    const pageSize = 20

    const { data, isLoading } = useQuery({
        queryKey: ['orders', page, searchTerm, status, startDate, endDate],
        queryFn: () => ordersApi.getOrders({
            page,
            pageSize,
            searchTerm,
            status,
            startDate: startDate || undefined,
            endDate: endDate || undefined
        }),
    })

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">Siparişler</h1>
                    <p className="text-gray-600 mt-1">Tüm siparişleri görüntüle ve yönet</p>
                </div>
            </div>

            {/* Filters */}
            <div className="bg-white rounded-lg shadow p-4 space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                    <div className="relative">
                        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                        <input
                            type="text"
                            placeholder="Sipariş ara..."
                            value={searchTerm}
                            onChange={(e) => {
                                setSearchTerm(e.target.value)
                                setPage(1)
                            }}
                            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                        />
                    </div>

                    <div className="relative">
                        <Filter className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                        <select
                            value={status}
                            onChange={(e) => {
                                const newStatus = e.target.value
                                setStatus(newStatus)
                                setPage(1)
                                const params: any = {}
                                if (newStatus) params.status = newStatus
                                if (startDate) params.startDate = startDate
                                if (endDate) params.endDate = endDate
                                setSearchParams(params)
                            }}
                            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                        >
                            {statusOptions.map((option) => (
                                <option key={option.value} value={option.value}>
                                    {option.label}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="relative">
                        <input
                            type="date"
                            value={startDate}
                            onChange={(e) => {
                                const newDate = e.target.value
                                setStartDate(newDate)
                                setPage(1)
                                const params: any = {}
                                if (status) params.status = status
                                if (newDate) params.startDate = newDate
                                if (endDate) params.endDate = endDate
                                setSearchParams(params)
                            }}
                            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                            placeholder="Başlangıç Tarihi"
                        />
                    </div>

                    <div className="relative">
                        <input
                            type="date"
                            value={endDate}
                            onChange={(e) => {
                                const newDate = e.target.value
                                setEndDate(newDate)
                                setPage(1)
                                const params: any = {}
                                if (status) params.status = status
                                if (startDate) params.startDate = startDate
                                if (newDate) params.endDate = newDate
                                setSearchParams(params)
                            }}
                            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                            placeholder="Bitiş Tarihi"
                        />
                    </div>
                </div>
            </div>

            {/* Active Date Filter Warning */}
            {(startDate || endDate) && (
                <div className="bg-blue-50 border-l-4 border-blue-400 p-4 mb-6 rounded shadow-sm flex items-center justify-between">
                    <div className="flex items-center">
                        <div className="flex-shrink-0">
                            <span className="text-blue-500">📅</span>
                        </div>
                        <div className="ml-3">
                            <p className="text-sm text-blue-700">
                                Şu an tarih filtresi uygulanıyor:
                                <span className="font-semibold ml-1">
                                    {startDate ? formatDate(startDate) : 'Başlangıç'}
                                    {' - '}
                                    {endDate ? formatDate(endDate) : 'Günümüz'}
                                </span>
                            </p>
                        </div>
                    </div>
                    <div>
                        <button
                            onClick={() => {
                                setStartDate('')
                                setEndDate('')
                                setStatus('')
                                setSearchParams({})
                                setPage(1)
                            }}
                            className="text-sm font-medium text-blue-700 hover:text-blue-600 underline"
                        >
                            Filtreleri Temizle
                        </button>
                    </div>
                </div>
            )}

            {/* Orders Table */}
            <div className="bg-white rounded-lg shadow overflow-hidden">
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
                                    Şirket
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Tutar
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Durum
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Ödeme
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Tarih
                                </th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    İşlemler
                                </th>
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            {isLoading ? (
                                <tr>
                                    <td colSpan={8} className="px-6 py-8 text-center">
                                        <div className="flex justify-center">
                                            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
                                        </div>
                                    </td>
                                </tr>
                            ) : data?.data && data.data.length > 0 ? (
                                data.data.map((order) => (
                                    <tr key={order.orderId} className="hover:bg-gray-50">
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <div className="text-sm font-medium text-gray-900">{order.orderNumber}</div>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                                            {order.customerName}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            {order.companyName || '-'}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                                            {formatCurrency(order.totalAmount)}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            {(() => {
                                                // Default styling if not found in list (e.g. for cases not in filter list)
                                                // Mapping matching the backend/OrderDetail colors:
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
                                                    <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${colors[order.orderStatus] || 'bg-gray-100 text-gray-800'}`}>
                                                        {labels[order.orderStatus] || order.orderStatus}
                                                    </span>
                                                )
                                            })()}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <span className="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800">
                                                {order.paymentStatus === 'Completed' ? 'Tamamlandı' : order.paymentStatus === 'Pending' ? 'Beklemede' : order.paymentStatus}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            {formatDateTime(order.createdAt)}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                                            <Link
                                                to={`/admin/orders/${order.orderId}`}
                                                className="text-primary-600 hover:text-primary-900 inline-flex items-center"
                                            >
                                                <Eye className="w-4 h-4 mr-1" />
                                                Görüntüle
                                            </Link>
                                        </td>
                                    </tr>
                                ))
                            ) : (
                                <tr>
                                    <td colSpan={8} className="px-6 py-8 text-center text-gray-500">
                                        Sipariş bulunamadı
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>

                {/* Pagination */}
                {data && data.totalPages > 1 && (
                    <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
                        <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                            <div>
                                <p className="text-sm text-gray-700">
                                    Toplam <span className="font-medium">{data.totalCount}</span> siparişten{' '}
                                    <span className="font-medium">{(page - 1) * pageSize + 1}</span> -{' '}
                                    <span className="font-medium">
                                        {Math.min(page * pageSize, data.totalCount)}
                                    </span>{' '}
                                    arası gösteriliyor
                                </p>
                            </div>
                            <div>
                                <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px">
                                    <button
                                        onClick={() => setPage((p) => Math.max(1, p - 1))}
                                        disabled={page === 1}
                                        className="relative inline-flex items-center px-4 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                                    >
                                        Önceki
                                    </button>
                                    <button
                                        onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
                                        disabled={page === data.totalPages}
                                        className="relative inline-flex items-center px-4 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                                    >
                                        Sonraki
                                    </button>
                                </nav>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    )
}
