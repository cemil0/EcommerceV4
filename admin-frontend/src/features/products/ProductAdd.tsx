import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { productsApi, categoriesApi } from '@/api/admin'
import { ArrowLeft, Save, Upload, X } from 'lucide-react'
import { toast } from 'sonner'
import type { CreateProductRequest } from '@/types/api'

export default function ProductAdd() {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const [selectedFiles, setSelectedFiles] = useState<File[]>([])
    const [previews, setPreviews] = useState<string[]>([])

    const [formData, setFormData] = useState<CreateProductRequest & { basePrice: string | number }>({
        productName: '',
        sku: '',
        categoryId: 0,
        basePrice: 0,
        shortDescription: '',
        longDescription: '',
        brand: '',
        manufacturer: '',
        model: '',
        metaTitle: '',
        metaDescription: '',
        metaKeywords: '',
        isFeatured: false,
        isNewArrival: false,
        isActive: true,
    })

    // Fetch categories
    const { data: categories } = useQuery({
        queryKey: ['categories'],
        queryFn: categoriesApi.getCategories,
    })

    const uploadImagesMutation = useMutation({
        mutationFn: async ({ id, files }: { id: number; files: File[] }) => {
            return productsApi.uploadImages(id, files)
        },
    })

    const createMutation = useMutation({
        mutationFn: (data: CreateProductRequest) => productsApi.createProduct({
            ...data,
            basePrice: Number(data.basePrice), // Parse on submit
        }),
        onSuccess: async (data) => {
            if (selectedFiles.length > 0) {
                try {
                    toast.info('Ürün oluşturuldu, resimler yükleniyor...')
                    await uploadImagesMutation.mutateAsync({
                        id: data.productId,
                        files: selectedFiles,
                    })
                } catch (error) {
                    toast.error('Resimler yüklenirken hata oluştu, ancak ürün oluşturuldu.')
                }
            }

            queryClient.invalidateQueries({ queryKey: ['products'] })
            toast.success('İşlem başarıyla tamamlandı')
            navigate('/admin/products')
        },
        onError: (error) => {
            console.error(error)
            toast.error('Ürün oluşturulurken bir hata oluştu')
        },
    })

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault()
        if (formData.categoryId === 0) {
            toast.error('Lütfen bir kategori seçin')
            return
        }
        createMutation.mutate(formData)
    }

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
        const { name, value, type } = e.target
        setFormData((prev) => ({
            ...prev,
            [name]: type === 'checkbox' ? (e.target as HTMLInputElement).checked :
                name === 'categoryId' ? Number(value) : value,
        }))
    }

    const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const newFiles = Array.from(e.target.files)
            setSelectedFiles((prev) => [...prev, ...newFiles])

            // Generate previews
            const newPreviews = newFiles.map(file => URL.createObjectURL(file))
            setPreviews((prev) => [...prev, ...newPreviews])
        }
    }

    const removeFile = (index: number) => {
        setSelectedFiles((prev) => prev.filter((_, i) => i !== index))
        setPreviews((prev) => {
            // Revoke URL to prevent memory leaks
            URL.revokeObjectURL(prev[index])
            return prev.filter((_, i) => i !== index)
        })
    }

    const isSubmitting = createMutation.isPending || uploadImagesMutation.isPending

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                    <button
                        onClick={() => navigate('/admin/products')}
                        className="p-2 hover:bg-gray-100 rounded-full transition-colors"
                    >
                        <ArrowLeft className="w-6 h-6 text-gray-600" />
                    </button>
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">Yeni Ürün Ekle</h1>
                        <p className="text-gray-600 mt-1">Yeni bir ürünü kataloğa ekleyin</p>
                    </div>
                </div>
                <button
                    onClick={handleSubmit}
                    disabled={isSubmitting}
                    className="inline-flex items-center px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50"
                >
                    <Save className="w-5 h-5 mr-2" />
                    {isSubmitting ? 'İşleniyor...' : 'Kaydet'}
                </button>
            </div>

            <form onSubmit={handleSubmit} className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                {/* Main Info */}
                <div className="lg:col-span-2 space-y-6">
                    <div className="bg-white rounded-lg shadow p-6 space-y-4">
                        <h2 className="text-lg font-semibold text-gray-900 border-b pb-2">Temel Bilgiler</h2>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700">Ürün Adı *</label>
                                <input
                                    type="text"
                                    name="productName"
                                    required
                                    value={formData.productName}
                                    onChange={handleChange}
                                    className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700">SKU (Stok Kodu) *</label>
                                <input
                                    type="text"
                                    name="sku"
                                    required
                                    value={formData.sku}
                                    onChange={handleChange}
                                    className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                                />
                            </div>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700">Kategori *</label>
                                <select
                                    name="categoryId"
                                    required
                                    value={formData.categoryId}
                                    onChange={handleChange}
                                    className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                                >
                                    <option value={0}>Seçiniz</option>
                                    {categories?.map((cat) => (
                                        <option key={cat.categoryId} value={cat.categoryId}>
                                            {cat.categoryName}
                                        </option>
                                    ))}
                                </select>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700">Başlangıç Fiyatı (₺) *</label>
                                <input
                                    type="number"
                                    name="basePrice"
                                    required
                                    min="0"
                                    step="0.01"
                                    value={formData.basePrice}
                                    onChange={handleChange}
                                    className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                                />
                            </div>
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-700">Kısa Açıklama</label>
                            <textarea
                                name="shortDescription"
                                rows={3}
                                value={formData.shortDescription}
                                onChange={handleChange}
                                className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-700">Detaylı Açıklama</label>
                            <textarea
                                name="longDescription"
                                rows={6}
                                value={formData.longDescription}
                                onChange={handleChange}
                                className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                            />
                        </div>
                    </div>

                    {/* Image Upload */}
                    <div className="bg-white rounded-lg shadow p-6 space-y-4">
                        <h2 className="text-lg font-semibold text-gray-900 border-b pb-2">Ürün Görselleri</h2>

                        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4">
                            {previews.map((preview, index) => (
                                <div key={index} className="relative group aspect-square bg-gray-100 rounded-lg overflow-hidden border border-gray-200">
                                    <img src={preview} alt={`Preview ${index}`} className="w-full h-full object-cover" />
                                    <button
                                        type="button"
                                        onClick={() => removeFile(index)}
                                        className="absolute top-1 right-1 bg-red-500 text-white rounded-full p-1 opacity-0 group-hover:opacity-100 transition-opacity"
                                    >
                                        <X className="w-4 h-4" />
                                    </button>
                                </div>
                            ))}
                            <label className="flex flex-col items-center justify-center aspect-square border-2 border-dashed border-gray-300 rounded-lg cursor-pointer hover:border-primary-500 hover:bg-primary-50 transition-colors">
                                <div className="flex flex-col items-center justify-center pt-5 pb-6">
                                    <Upload className="w-8 h-8 text-gray-400 mb-2" />
                                    <p className="text-xs text-gray-500">Resim Ekle</p>
                                </div>
                                <input
                                    type="file"
                                    className="hidden"
                                    multiple
                                    accept="image/*"
                                    onChange={handleFileSelect}
                                />
                            </label>
                        </div>
                        <p className="text-sm text-gray-500">
                            Desteklenen formatlar: JPG, PNG, WEBP. Maksimum 5MB.
                        </p>
                    </div>
                </div>

                {/* Sidebar Info */}
                <div className="space-y-6">
                    <div className="bg-white rounded-lg shadow p-6 space-y-4">
                        <h2 className="text-lg font-semibold text-gray-900 border-b pb-2">Ürün Detayları</h2>

                        <div>
                            <label className="block text-sm font-medium text-gray-700">Marka</label>
                            <input
                                type="text"
                                name="brand"
                                value={formData.brand}
                                onChange={handleChange}
                                className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Üretici</label>
                            <input
                                type="text"
                                name="manufacturer"
                                value={formData.manufacturer}
                                onChange={handleChange}
                                className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Model</label>
                            <input
                                type="text"
                                name="model"
                                value={formData.model}
                                onChange={handleChange}
                                className="mt-1 block w-full rounded-md border-2 border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
                            />
                        </div>
                    </div>

                    <div className="bg-white rounded-lg shadow p-6 space-y-4">
                        <h2 className="text-lg font-semibold text-gray-900 border-b pb-2">Görünürlük</h2>

                        <div className="flex items-center">
                            <input
                                type="checkbox"
                                name="isActive"
                                id="isActive"
                                checked={formData.isActive}
                                onChange={handleChange}
                                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                            />
                            <label htmlFor="isActive" className="ml-2 block text-sm text-gray-900">
                                Aktif (Satışta)
                            </label>
                        </div>
                        <div className="flex items-center">
                            <input
                                type="checkbox"
                                name="isFeatured"
                                id="isFeatured"
                                checked={formData.isFeatured}
                                onChange={handleChange}
                                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                            />
                            <label htmlFor="isFeatured" className="ml-2 block text-sm text-gray-900">
                                Öne Çıkan Ürün
                            </label>
                        </div>
                        <div className="flex items-center">
                            <input
                                type="checkbox"
                                name="isNewArrival"
                                id="isNewArrival"
                                checked={formData.isNewArrival}
                                onChange={handleChange}
                                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                            />
                            <label htmlFor="isNewArrival" className="ml-2 block text-sm text-gray-900">
                                Yeni Gelenler
                            </label>
                        </div>
                    </div>
                </div>
            </form>
        </div>
    )
}
