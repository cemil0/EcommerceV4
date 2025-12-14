import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ordersApi } from '@/api/admin'
import { formatCurrency, formatDateTime } from '@/lib/utils'
import { ArrowLeft, MapPin, User, Building, Package, ExternalLink, CreditCard } from 'lucide-react'
import { toast } from 'sonner'
import { useState } from 'react'
import { ConfirmModal } from '@/components/ui/Modal'

const orderStatuses = [
    { value: 'Pending', label: 'Beklemede', color: 'bg-yellow-100 text-yellow-800' },
    { value: 'Processing', label: 'İşleniyor', color: 'bg-blue-100 text-blue-800' },
    { value: 'Shipped', label: 'Kargoda', color: 'bg-indigo-100 text-indigo-800' },
    { value: 'Delivered', label: 'Teslim Edildi', color: 'bg-green-100 text-green-800' },
    { value: 'Cancelled', label: 'İptal Edildi', color: 'bg-red-100 text-red-800' },
]

export default function OrderDetail() {
    const { id } = useParams()
    const navigate = useNavigate()
    const orderId = Number(id)
    const queryClient = useQueryClient()
    const [status, setStatus] = useState('')

    // Modal state
    const [isConfirmOpen, setIsConfirmOpen] = useState(false)
    const [pendingStatus, setPendingStatus] = useState('')

    const { data: order, isLoading } = useQuery({
        queryKey: ['order', orderId],
        queryFn: () => ordersApi.getOrderDetail(orderId),
        enabled: !!orderId,
    })

    const updateStatusMutation = useMutation({
        mutationFn: (newStatus: string) => ordersApi.updateOrderStatus(orderId, newStatus),
        onSuccess: () => {
            toast.success('Sipariş durumu güncellendi')
            queryClient.invalidateQueries({ queryKey: ['order', orderId] })
            queryClient.invalidateQueries({ queryKey: ['orders'] })
            setIsConfirmOpen(false)
        },
        onError: () => {
            toast.error('Durum güncellenemedi')
            setIsConfirmOpen(false)
        }
    })

    if (isLoading) return <div>Yükleniyor...</div>
    if (!order) return <div>Sipariş bulunamadı</div>

    const handleStatusChange = (newStatus: string) => {
        setPendingStatus(newStatus)
        setIsConfirmOpen(true)
    }

    const confirmStatusChange = () => {
        if (pendingStatus) {
            setStatus(pendingStatus)
            updateStatusMutation.mutate(pendingStatus)
        }
    }

    const currentStatus = orderStatuses.find(s => s.value === order.orderStatus)

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                <div className="flex items-center space-x-4">
                    <button
                        onClick={() => navigate('/admin/orders')}
                        className="p-2 hover:bg-gray-100 rounded-full transition-colors"
                    >
                        <ArrowLeft className="w-6 h-6 text-gray-600" />
                    </button>
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">Sipariş #{order.orderNumber}</h1>
                        <p className="text-gray-500 text-sm">
                            {formatDateTime(order.createdAt)} tarihinde oluşturuldu
                        </p>
                    </div>
                </div>

                <div className="flex items-center space-x-3">
                    <span className={`px-3 py-1 rounded-full text-sm font-semibold ${currentStatus?.color}`}>
                        {currentStatus?.label || order.orderStatus}
                    </span>
                    <select
                        value={order.orderStatus}
                        onChange={(e) => handleStatusChange(e.target.value)}
                        className="block rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
                    >
                        {orderStatuses.map((s) => (
                            <option key={s.value} value={s.value}>{s.label}</option>
                        ))}
                    </select>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Main Content */}
                <div className="lg:col-span-2 space-y-6">
                    {/* Order Items */}
                    <div className="bg-white rounded-lg shadow overflow-hidden">
                        <div className="px-6 py-4 border-b border-gray-200">
                            <h2 className="text-lg font-semibold text-gray-900 flex items-center">
                                <Package className="w-5 h-5 mr-2 text-gray-500" />
                                Sipariş İçeriği
                            </h2>
                        </div>
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Ürün</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Birim Fiyat</th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Adet</th>
                                        <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Toplam</th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                    {order.items.map((item) => (
                                        <tr key={item.orderItemId}>
                                            <td className="px-6 py-4">
                                                <div className="text-sm font-medium text-gray-900">{item.productName}</div>
                                                <div className="text-xs text-gray-500">Varyant: {item.variantName}</div>
                                            </td>
                                            <td className="px-6 py-4 text-sm text-gray-500">
                                                {formatCurrency(item.unitPrice)}
                                            </td>
                                            <td className="px-6 py-4 text-sm text-gray-900">
                                                {item.quantity}
                                            </td>
                                            <td className="px-6 py-4 text-right text-sm font-medium text-gray-900">
                                                {formatCurrency(item.totalPrice)}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                        {/* Order Totals */}
                        <div className="bg-gray-50 px-6 py-4 border-t border-gray-200">
                            <div className="flex flex-col space-y-2 items-end">
                                <div className="flex justify-between w-full sm:w-1/3 text-sm">
                                    <span className="text-gray-500">Ara Toplam:</span>
                                    <span className="font-medium">{formatCurrency(order.subTotal)}</span>
                                </div>
                                <div className="flex justify-between w-full sm:w-1/3 text-sm">
                                    <span className="text-gray-500">KDV:</span>
                                    <span className="font-medium">{formatCurrency(order.taxAmount)}</span>
                                </div>
                                <div className="flex justify-between w-full sm:w-1/3 text-sm">
                                    <span className="text-gray-500">Kargo:</span>
                                    <span className="font-medium">{formatCurrency(order.shippingCost)}</span>
                                </div>
                                <div className="border-t border-gray-300 w-full sm:w-1/3 my-2"></div>
                                <div className="flex justify-between w-full sm:w-1/3 text-lg font-bold text-gray-900">
                                    <span>Genel Toplam:</span>
                                    <span>{formatCurrency(order.totalAmount)}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Sidebar Info */}
                <div className="space-y-6">
                    {/* Customer Info */}
                    <div className="bg-white rounded-lg shadow p-6">
                        <h2 className="text-lg font-semibold text-gray-900 border-b pb-2 mb-4 flex items-center">
                            <User className="w-5 h-5 mr-2 text-gray-500" />
                            Müşteri Bilgileri
                        </h2>
                        <div className="space-y-3">
                            <div>
                                <p className="text-sm text-gray-500">Ad Soyad</p>
                                <p className="font-medium text-gray-900">{order.customer.fullName}</p>
                            </div>
                            <div>
                                <p className="text-sm text-gray-500">E-posta</p>
                                <p className="font-medium text-gray-900">{order.customer.email}</p>
                            </div>
                            <div>
                                <p className="text-sm text-gray-500">Telefon</p>
                                <p className="font-medium text-gray-900">{order.customer.phone || '-'}</p>
                            </div>
                        </div>
                    </div>

                    {/* Company Info (if exists) */}
                    {order.company && (
                        <div className="bg-white rounded-lg shadow p-6">
                            <h2 className="text-lg font-semibold text-gray-900 border-b pb-2 mb-4 flex items-center">
                                <Building className="w-5 h-5 mr-2 text-gray-500" />
                                Şirket Bilgileri
                            </h2>
                            <div className="space-y-3">
                                <div>
                                    <p className="text-sm text-gray-500">Şirket Adı</p>
                                    <p className="font-medium text-gray-900">{order.company.companyName}</p>
                                </div>
                                <div>
                                    <p className="text-sm text-gray-500">Vergi No</p>
                                    <p className="font-medium text-gray-900">{order.company.taxNumber}</p>
                                </div>
                            </div>
                        </div>
                    )}

                    {/* Addresses */}
                    <div className="bg-white rounded-lg shadow p-6">
                        <h2 className="text-lg font-semibold text-gray-900 border-b pb-2 mb-4 flex items-center">
                            <MapPin className="w-5 h-5 mr-2 text-gray-500" />
                            Adresler
                        </h2>
                        <div className="space-y-4">
                            <div>
                                <h3 className="text-sm font-medium text-gray-900 mb-1">Teslimat Adresi</h3>
                                <p className="text-sm text-gray-600">
                                    {order.shippingAddress.addressLine}<br />
                                    {order.shippingAddress.district} / {order.shippingAddress.city}<br />
                                    {order.shippingAddress.postalCode}
                                </p>
                            </div>
                            <div>
                                <h3 className="text-sm font-medium text-gray-900 mb-1">Fatura Adresi</h3>
                                <p className="text-sm text-gray-600">
                                    {order.billingAddress.addressLine}<br />
                                    {order.billingAddress.district} / {order.billingAddress.city}<br />
                                    {order.billingAddress.postalCode}
                                </p>
                            </div>
                        </div>
                    </div>

                    {/* Payment Info */}
                    <div className="bg-white rounded-lg shadow p-6">
                        <h2 className="text-lg font-semibold text-gray-900 border-b pb-2 mb-4 flex items-center">
                            <CreditCard className="w-5 h-5 mr-2 text-gray-500" />
                            Ödeme Bilgisi
                        </h2>
                        <div className="flex items-center justify-between">
                            <span className="text-sm text-gray-600">Ödeme Durumu:</span>
                            <span className="px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800">
                                {order.paymentStatus === 'Completed' ? 'Tamamlandı' : order.paymentStatus === 'Pending' ? 'Beklemede' : order.paymentStatus}
                            </span>
                        </div>
                    </div>
                </div>
            </div>

            <ConfirmModal
                isOpen={isConfirmOpen}
                onClose={() => setIsConfirmOpen(false)}
                onConfirm={confirmStatusChange}
                title="Sipariş Durumu Güncelleme"
                message={`Sipariş durumunu "${orderStatuses.find(s => s.value === pendingStatus)?.label}" olarak değiştirmek istediğinize emin misiniz?`}
                confirmText="Evet, Değiştir"
                cancelText="İptal"
                isLoading={updateStatusMutation.isPending}
            />
        </div>
    )
}
