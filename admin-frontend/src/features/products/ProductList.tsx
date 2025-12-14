import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { productsApi } from '@/api/admin'
import { formatCurrency } from '@/lib/utils'
import { Search, Eye, Power, PowerOff, Plus, AlertTriangle, X, FileSpreadsheet, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import { useSearchParams } from 'react-router-dom'

export default function ProductList() {
    const [searchParams, setSearchParams] = useSearchParams()
    const [page, setPage] = useState(1)
    const [searchTerm, setSearchTerm] = useState('')
    const pageSize = 20

    // Delete modal state
    const [deleteModal, setDeleteModal] = useState<{ show: boolean; productId: number | null; productName: string }>({
        show: false,
        productId: null,
        productName: ''
    })
    const [isDeleting, setIsDeleting] = useState(false)

    const isLowStockFilter = searchParams.get('filter') === 'low-stock'

    const { data, isLoading, refetch } = useQuery({
        queryKey: ['products', page, searchTerm, isLowStockFilter],
        queryFn: () => productsApi.getProducts({
            page,
            pageSize,
            searchTerm,
            lowStock: isLowStockFilter
        }),
    })

    const handleActivate = async (id: number) => {
        try {
            await productsApi.activateProduct(id)
            toast.success('Ürün aktifleştirildi')
            refetch()
        } catch (error) {
            toast.error('Hata oluştu')
        }
    }

    const handleDeactivate = async (id: number) => {
        try {
            await productsApi.deactivateProduct(id)
            toast.success('Ürün pasifleştirildi')
            refetch()
        } catch (error) {
            toast.error('Hata oluştu')
        }
    }

    const openDeleteModal = (id: number, productName: string) => {
        setDeleteModal({ show: true, productId: id, productName })
    }

    const closeDeleteModal = () => {
        setDeleteModal({ show: false, productId: null, productName: '' })
    }

    const confirmDelete = async () => {
        if (!deleteModal.productId) return

        setIsDeleting(true)
        try {
            await productsApi.deleteProduct(deleteModal.productId)
            toast.success('Ürün başarıyla silindi')
            refetch()
            closeDeleteModal()
        } catch (error) {
            toast.error('Ürün silinirken hata oluştu')
        } finally {
            setIsDeleting(false)
        }
    }

    return (
        <>
            {/* Delete Confirmation Modal */}
            {deleteModal.show && (
                <div className="fixed inset-0 z-50 overflow-y-auto">
                    <div className="flex min-h-full items-center justify-center p-4">
                        {/* Backdrop */}
                        <div
                            className="fixed inset-0 bg-black/50 transition-opacity"
                            onClick={closeDeleteModal}
                        />

                        {/* Modal */}
                        <div className="relative bg-white rounded-xl shadow-2xl max-w-md w-full p-6 transform transition-all">
                            <div className="flex items-center justify-center w-12 h-12 mx-auto mb-4 rounded-full bg-red-100">
                                <Trash2 className="w-6 h-6 text-red-600" />
                            </div>

                            <h3 className="text-lg font-semibold text-gray-900 text-center mb-2">
                                Ürünü Sil
                            </h3>

                            <p className="text-gray-600 text-center mb-6">
                                <span className="font-medium text-gray-900">"{deleteModal.productName}"</span> ürününü silmek istediğinizden emin misiniz?
                                <br />
                                <span className="text-red-600 text-sm font-medium">Bu işlem geri alınamaz.</span>
                            </p>

                            <div className="flex gap-3">
                                <button
                                    onClick={closeDeleteModal}
                                    disabled={isDeleting}
                                    className="flex-1 px-4 py-2.5 border border-gray-300 rounded-lg text-gray-700 font-medium hover:bg-gray-50 transition-colors disabled:opacity-50"
                                >
                                    İptal
                                </button>
                                <button
                                    onClick={confirmDelete}
                                    disabled={isDeleting}
                                    className="flex-1 px-4 py-2.5 bg-red-600 text-white rounded-lg font-medium hover:bg-red-700 transition-colors disabled:opacity-50 flex items-center justify-center"
                                >
                                    {isDeleting ? (
                                        <>
                                            <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent mr-2" />
                                            Siliniyor...
                                        </>
                                    ) : (
                                        'Evet, Sil'
                                    )}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            <div className="space-y-6">
                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">Ürünler</h1>
                        <p className="text-gray-600 mt-1">Tüm ürünleri görüntüle ve yönet</p>
                    </div>
                    <div className="flex space-x-3">
                        <Link
                            to="/admin/products/import"
                            className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                        >
                            <FileSpreadsheet className="w-5 h-5 mr-2 text-green-600" />
                            Toplu Ekle
                        </Link>
                        <Link
                            to="/admin/products/new"
                            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-primary-600 hover:bg-primary-700"
                        >
                            <Plus className="w-5 h-5 mr-2" />
                            Yeni Ürün Ekle
                        </Link>
                    </div>
                </div>

                {/* Search and Filters */}
                <div className="bg-white rounded-lg shadow p-4 flex flex-col sm:flex-row gap-4 items-center justify-between">
                    <div className="relative flex-1 w-full">
                        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                        <input
                            type="text"
                            placeholder="Ürün ara..."
                            value={searchTerm}
                            onChange={(e) => {
                                setSearchTerm(e.target.value)
                                setPage(1)
                            }}
                            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                        />
                    </div>
                    {isLowStockFilter && (
                        <div className="flex items-center space-x-2 bg-orange-50 px-3 py-2 rounded-lg border border-orange-200">
                            <AlertTriangle className="w-4 h-4 text-orange-600" />
                            <span className="text-sm font-medium text-orange-800">Düşük Stok Filtresi</span>
                            <button
                                onClick={() => setSearchParams({})}
                                className="text-orange-500 hover:text-orange-700 ml-2"
                            >
                                <X className="w-4 h-4" />
                            </button>
                        </div>
                    )}
                </div>

                {/* Products Table */}
                <div className="bg-white rounded-lg shadow overflow-hidden">
                    <div className="overflow-x-auto">
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50">
                                <tr>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                        Ürün
                                    </th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                        SKU
                                    </th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                        Kategori
                                    </th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                        Varyant
                                    </th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                        Stok
                                    </th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                        Fiyat
                                    </th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                        Durum
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
                                    data.data.map((product) => (
                                        <tr key={product.productId} className="hover:bg-gray-50">
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <div className="text-sm font-medium text-gray-900">{product.productName}</div>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                                {product.sku}
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                                {product.categoryName}
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                                {product.variantCount}
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                                <div className="flex items-center">
                                                    <span className={product.totalStock <= 30 ? 'text-orange-600 font-bold' : ''}>
                                                        {product.totalStock}
                                                    </span>
                                                    {product.totalStock <= 30 && (
                                                        <div className="group relative ml-2">
                                                            <AlertTriangle className="w-4 h-4 text-orange-500" />
                                                            <div className="absolute bottom-full left-1/2 transform -translate-x-1/2 mb-2 px-2 py-1 text-xs text-white bg-gray-800 rounded opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap">
                                                                Düşük Stok (&le;30)
                                                            </div>
                                                        </div>
                                                    )}
                                                </div>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                                                {formatCurrency(product.basePrice)}
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <span
                                                    className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${product.isActive
                                                        ? 'bg-green-100 text-green-800'
                                                        : 'bg-red-100 text-red-800'
                                                        }`}
                                                >
                                                    {product.isActive ? 'Aktif' : 'Pasif'}
                                                </span>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm font-medium space-x-2">
                                                <Link
                                                    to={`/admin/products/${product.productId}`}
                                                    className="text-primary-600 hover:text-primary-900 inline-flex items-center"
                                                >
                                                    <Eye className="w-4 h-4 mr-1" />
                                                    Görüntüle
                                                </Link>
                                                {product.isActive ? (
                                                    <button
                                                        onClick={() => handleDeactivate(product.productId)}
                                                        className="text-red-600 hover:text-red-900 inline-flex items-center ml-3"
                                                    >
                                                        <PowerOff className="w-4 h-4 mr-1" />
                                                        Pasifleştir
                                                    </button>
                                                ) : (
                                                    <button
                                                        onClick={() => handleActivate(product.productId)}
                                                        className="text-green-600 hover:text-green-900 inline-flex items-center ml-3"
                                                    >
                                                        <Power className="w-4 h-4 mr-1" />
                                                        Aktifleştir
                                                    </button>
                                                )}
                                                <button
                                                    onClick={() => openDeleteModal(product.productId, product.productName)}
                                                    className="text-gray-500 hover:text-red-700 inline-flex items-center ml-3"
                                                    title="Ürünü Kaldır"
                                                >
                                                    <Trash2 className="w-4 h-4 mr-1" />
                                                    Kaldır
                                                </button>
                                            </td>
                                        </tr>
                                    ))
                                ) : (
                                    <tr>
                                        <td colSpan={8} className="px-6 py-8 text-center text-gray-500">
                                            Ürün bulunamadı
                                        </td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>

                    {/* Pagination */}
                    {data && data.totalPages > 1 && (
                        <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
                            <div className="flex-1 flex justify-between sm:hidden">
                                <button
                                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                                    disabled={page === 1}
                                    className="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                                >
                                    Önceki
                                </button>
                                <button
                                    onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
                                    disabled={page === data.totalPages}
                                    className="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                                >
                                    Sonraki
                                </button>
                            </div>
                            <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                                <div>
                                    <p className="text-sm text-gray-700">
                                        Toplam <span className="font-medium">{data.totalCount}</span> üründen{' '}
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
                                            className="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                                        >
                                            Önceki
                                        </button>
                                        <button
                                            onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
                                            disabled={page === data.totalPages}
                                            className="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
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
        </>
    )
}
