import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { productsApi } from '@/api/admin'
import { Upload, FileSpreadsheet, CheckCircle, XCircle, ArrowLeft, AlertTriangle } from 'lucide-react'
import { toast } from 'sonner'
import type { BulkImportResultDto } from '@/types/api'
import * as XLSX from 'xlsx'



export default function ProductImport() {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const [file, setFile] = useState<File | null>(null)
    const [result, setResult] = useState<BulkImportResultDto | null>(null)
    const [isDragging, setIsDragging] = useState(false)

    const uploadMutation = useMutation({
        mutationFn: productsApi.bulkUpload,
        onSuccess: (data) => {
            setResult(data)
            if (data.failedCount === 0) {
                toast.success(`Toplam ${data.successCount} ürün başarıyla eklendi!`)
                queryClient.invalidateQueries({ queryKey: ['products'] })
            } else {
                toast.warning(`${data.successCount} ürün eklendi, ${data.failedCount} ürün hataya takıldı.`)
            }
        },
        onError: (error: any) => {
            console.error('Upload Error:', error)
            const errorMsg =
                error?.response?.data?.message ||
                error?.response?.data?.title ||
                error?.message ||
                'Dosya yüklenirken bir hata oluştu.'

            // If backend returned a specific error DTO in the failure response
            if (error?.response?.data?.errors && Array.isArray(error.response.data.errors)) {
                const firstError = error.response.data.errors[0]
                toast.error(`Kritik Hata: ${firstError.message || JSON.stringify(firstError)}`)
            } else {
                toast.error(typeof errorMsg === 'string' ? errorMsg : 'Kritik bir sunucu hatası oluştu.')
            }
        }
    })

    const validateFile = (file: File): boolean => {
        // 1. File Size Check (Max 10MB)
        const MAX_FILE_SIZE = 10 * 1024 * 1024 // 10MB
        if (file.size > MAX_FILE_SIZE) {
            toast.error('Dosya boyutu 10MB\'dan büyük olamaz.')
            return false
        }

        // 2. MIME Type / Extension Check
        const validExtensions = ['.xlsx', '.csv']
        const fileName = file.name.toLowerCase()
        const isValidExtension = validExtensions.some(ext => fileName.endsWith(ext))

        if (!isValidExtension) {
            toast.error('Sadece .xlsx veya .csv dosyaları desteklenir.')
            return false
        }

        return true
    }

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files[0]) {
            const selectedFile = e.target.files[0]
            if (validateFile(selectedFile)) {
                setFile(selectedFile)
                setResult(null)
            } else {
                e.target.value = '' // Reset input
            }
        }
    }

    const handleDrop = (e: React.DragEvent) => {
        e.preventDefault()
        setIsDragging(false)
        if (e.dataTransfer.files && e.dataTransfer.files[0]) {
            const droppedFile = e.dataTransfer.files[0]
            if (validateFile(droppedFile)) {
                setFile(droppedFile)
                setResult(null)
            }
        }
    }

    const handleDragOver = (e: React.DragEvent) => {
        e.preventDefault()
        setIsDragging(true)
    }

    const handleDragLeave = (e: React.DragEvent) => {
        e.preventDefault()
        setIsDragging(false)
    }

    const handleUpload = () => {
        if (!file) return
        uploadMutation.mutate(file)
    }

    const downloadTemplate = () => {
        // Enterprise Grade Template with Instructions
        const wb = XLSX.utils.book_new()

        // 1. Instructions Sheet
        const wsInfo = XLSX.utils.aoa_to_sheet([
            ['Toplu Ürün Yükleme Şablonu Kullanım Kılavuzu'],
            [''],
            ['Zorunlu Alanlar:'],
            ['ProductName', 'Ürün Adı (Benzersiz olması önerilir)'],
            ['CategorySlug', 'Kategori Linki (Örn: elektronik, giyim)'],
            ['VariantSKU', 'Varyant Stok Kodu (Benzersiz ZORUNLU)'],
            ['BasePrice', 'Satış Fiyatı (Sayısal, örn: 199.99)'],
            ['StockQuantity', 'Stok Adedi (Sayısal, tam sayı)'],
            [''],
            ['Opsiyonel Alanlar:'],
            ['Brand', 'Marka (Örn: Apple, Samsung, Sony)'],
            ['Manufacturer', 'Üretici Firma'],
            ['Model', 'Model Bilgisi'],
            ['Description', 'Ürün Açıklaması'],
            ['ShortDescription', 'Kısa Açıklama'],
            ['Color', 'Renk'],
            ['Size', 'Beden'],
            ['CostPrice', 'Maliyet Fiyatı'],
            ['IsFeatured', 'Öne Çıkan Ürün (TRUE/FALSE)'],
            ['IsNewArrival', 'Yeni Ürün (TRUE/FALSE)'],
            ['IsActive', 'Aktif (TRUE/FALSE, Varsayılan: TRUE)']
        ])

        // Adjust column widths for Info
        wsInfo['!cols'] = [{ wch: 20 }, { wch: 50 }]
        XLSX.utils.book_append_sheet(wb, wsInfo, 'Kullanım Kılavuzu')

        // 2. Data Sheet
        const wsData = XLSX.utils.json_to_sheet([
            {
                ProductName: 'Örnek Akıllı Telefon',
                CategorySlug: 'elektronik',
                VariantSKU: 'TEL-001-SIYAH',
                BasePrice: 15999.90,
                StockQuantity: 50,
                Brand: 'TechBrand',
                Manufacturer: 'TechBrand Inc.',
                Model: 'Pro X',
                Description: 'Son model akıllı telefon',
                ShortDescription: 'Akıllı telefon',
                Color: 'Siyah',
                Size: '128GB',
                CostPrice: 12000,
                IsFeatured: true,
                IsNewArrival: true,
                IsActive: true
            },
            {
                ProductName: 'Örnek Tişört',
                CategorySlug: 'giyim',
                VariantSKU: 'TSHIRT-001-M',
                BasePrice: 499.99,
                StockQuantity: 200,
                Brand: 'ModaMarka',
                Manufacturer: 'ModaMarka Tekstil',
                Model: 'Basic V Yaka',
                Description: '%100 Pamuklu Tişört',
                ShortDescription: 'Pamuklu tişört',
                Color: 'Beyaz',
                Size: 'M',
                CostPrice: 150,
                IsFeatured: false,
                IsNewArrival: false,
                IsActive: true
            }
        ])

        // Adjust column widths for Data
        wsData['!cols'] = [
            { wch: 22 }, { wch: 15 }, { wch: 18 }, { wch: 12 }, { wch: 12 },
            { wch: 15 }, { wch: 18 }, { wch: 15 }, { wch: 30 }, { wch: 20 },
            { wch: 10 }, { wch: 10 }, { wch: 12 }, { wch: 12 }, { wch: 12 }, { wch: 10 }
        ]

        XLSX.utils.book_append_sheet(wb, wsData, 'Products')

        // Save file
        XLSX.writeFile(wb, 'Urun_Import_Sablonu.xlsx')
    }

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                    <button
                        onClick={() => navigate('/admin/products')}
                        className="p-2 hover:bg-gray-100 rounded-full transition-colors"
                        type="button"
                    >
                        <ArrowLeft className="w-6 h-6 text-gray-600" />
                    </button>
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">Toplu Ürün Ekle</h1>
                        <p className="text-gray-600 mt-1">Excel dosyası ile ürünleri toplu olarak yükleyin</p>
                    </div>
                </div>
                <button
                    onClick={downloadTemplate}
                    type="button"
                    className="inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
                >
                    <FileSpreadsheet className="w-5 h-5 mr-2 text-green-600" />
                    Şablonu İndir
                </button>
            </div>

            <div className="bg-white rounded-lg shadow p-8">
                {!file ? (
                    <div
                        onDrop={handleDrop}
                        onDragOver={handleDragOver}
                        onDragLeave={handleDragLeave}
                        className={`border-2 border-dashed rounded-xl p-12 text-center transition-all duration-300 ${isDragging
                                ? 'border-primary-500 bg-primary-50 scale-[1.02]'
                                : 'border-gray-300 hover:border-primary-400 hover:bg-gray-50'
                            }`}
                    >
                        <div className="flex flex-col items-center justify-center">
                            <div className={`p-4 rounded-full mb-4 transition-colors ${isDragging ? 'bg-primary-100' : 'bg-gray-100'
                                }`}>
                                <Upload className={`w-10 h-10 ${isDragging ? 'text-primary-500' : 'text-gray-400'}`} />
                            </div>
                            <h3 className="text-lg font-semibold text-gray-900">
                                Dosyayı buraya sürükleyin
                            </h3>
                            <p className="mt-2 text-sm text-gray-500">
                                veya bilgisayarınızdan seçin
                            </p>
                            <p className="mt-1 text-xs text-gray-400">
                                .xlsx veya .csv formatında (Maksimum 10MB)
                            </p>
                            <input
                                type="file"
                                className="hidden"
                                id="file-upload"
                                accept=".xlsx,.csv"
                                onChange={handleFileChange}
                            />
                            <label
                                htmlFor="file-upload"
                                className="mt-6 inline-flex items-center px-6 py-3 border-2 border-primary-500 text-sm font-medium rounded-lg text-primary-600 bg-white hover:bg-primary-50 cursor-pointer transition-colors"
                            >
                                <FileSpreadsheet className="w-5 h-5 mr-2" />
                                Dosya Seç
                            </label>
                        </div>
                    </div>
                ) : (
                    <div className="border-2 border-green-400 bg-green-50 rounded-xl p-8">
                        <div className="flex items-center justify-between">
                            <div className="flex items-center space-x-4">
                                <div className="p-4 bg-green-100 rounded-xl">
                                    <FileSpreadsheet className="w-10 h-10 text-green-600" />
                                </div>
                                <div>
                                    <h3 className="text-lg font-semibold text-gray-900 flex items-center">
                                        <CheckCircle className="w-5 h-5 text-green-500 mr-2" />
                                        Dosya Seçildi
                                    </h3>
                                    <p className="text-sm font-medium text-gray-700 mt-1">{file.name}</p>
                                    <p className="text-xs text-gray-500 mt-0.5">
                                        {(file.size / 1024).toFixed(1)} KB
                                    </p>
                                </div>
                            </div>
                            <div className="flex items-center space-x-3">
                                <button
                                    onClick={() => { setFile(null); setResult(null) }}
                                    type="button"
                                    className="px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                                >
                                    Değiştir
                                </button>
                                <button
                                    onClick={handleUpload}
                                    type="button"
                                    disabled={uploadMutation.isPending}
                                    className="inline-flex items-center px-6 py-3 border border-transparent text-sm font-medium rounded-lg shadow-sm text-white bg-green-600 hover:bg-green-700 disabled:opacity-50 transition-colors"
                                >
                                    {uploadMutation.isPending ? (
                                        <>
                                            <svg className="animate-spin -ml-1 mr-2 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                            </svg>
                                            Yükleniyor...
                                        </>
                                    ) : (
                                        <>
                                            <Upload className="w-5 h-5 mr-2" />
                                            Yüklemeyi Başlat
                                        </>
                                    )}
                                </button>
                            </div>
                        </div>
                    </div>
                )}
            </div>

            {result && (
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                    <div className="bg-white rounded-lg shadow p-6 border-l-4 border-green-500">
                        <div className="flex items-center">
                            <div className="flex-shrink-0 bg-green-100 rounded-full p-3">
                                <CheckCircle className="w-6 h-6 text-green-600" />
                            </div>
                            <div className="ml-4">
                                <h3 className="text-lg font-medium text-gray-900">Başarılı</h3>
                                <p className="text-2xl font-bold text-green-600">{result.successCount}</p>
                            </div>
                        </div>
                    </div>
                    <div className="bg-white rounded-lg shadow p-6 border-l-4 border-red-500">
                        <div className="flex items-center">
                            <div className="flex-shrink-0 bg-red-100 rounded-full p-3">
                                <XCircle className="w-6 h-6 text-red-600" />
                            </div>
                            <div className="ml-4">
                                <h3 className="text-lg font-medium text-gray-900">Hatalı</h3>
                                <p className="text-2xl font-bold text-red-600">{result.failedCount}</p>
                            </div>
                        </div>
                    </div>
                    <div className="bg-white rounded-lg shadow p-6 border-l-4 border-blue-500">
                        <div className="flex items-center">
                            <div className="flex-shrink-0 bg-blue-100 rounded-full p-3">
                                <FileSpreadsheet className="w-6 h-6 text-blue-600" />
                            </div>
                            <div className="ml-4">
                                <h3 className="text-lg font-medium text-gray-900">İşlenen</h3>
                                { /* Safely access totalRows as verified in backend DTO */}
                                <p className="text-2xl font-bold text-blue-600">{result.totalRows ?? (result.successCount + result.failedCount)}</p>
                            </div>
                        </div>
                    </div>

                    {/* Success Report */}
                    {result.successes && result.successes.length > 0 && (
                        <div className="md:col-span-3 bg-white rounded-lg shadow overflow-hidden">
                            <div className="px-6 py-4 border-b border-gray-200 bg-green-50">
                                <h3 className="text-lg font-medium text-green-800 flex items-center">
                                    <CheckCircle className="w-5 h-5 mr-2" />
                                    Başarıyla İşlenen Kayıtlar ({result.successes.length})
                                </h3>
                            </div>
                            <div className="max-h-64 overflow-y-auto">
                                <table className="min-w-full divide-y divide-gray-200">
                                    <thead className="bg-gray-50">
                                        <tr>
                                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Satır</th>
                                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">SKU</th>
                                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">İşlem</th>
                                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Mesaj</th>
                                        </tr>
                                    </thead>
                                    <tbody className="bg-white divide-y divide-gray-200">
                                        {result.successes.map((success, idx) => (
                                            <tr key={`success-${idx}`}>
                                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{success.rowNumber}</td>
                                                <td className="px-6 py-4 whitespace-nowrap text-sm font-mono text-gray-500">{success.sku}</td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <span className="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800">
                                                        {success.action === 'Created' ? 'Oluşturuldu' : success.action === 'Updated' ? 'Güncellendi' : success.action}
                                                    </span>
                                                </td>
                                                <td className="px-6 py-4 text-sm text-green-600">{success.message}</td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    )}

                    {/* Error and Warning Report */}
                    {(result.errors.length > 0 || (result.warnings && result.warnings.length > 0)) && (
                        <div className="md:col-span-3 bg-white rounded-lg shadow overflow-hidden">
                            <div className="px-6 py-4 border-b border-gray-200 bg-red-50">
                                <h3 className="text-lg font-medium text-red-800 flex items-center">
                                    <AlertTriangle className="w-5 h-5 mr-2" />
                                    Hata ve Uyarı Raporu
                                </h3>
                            </div>
                            <div className="max-h-96 overflow-y-auto">
                                <table className="min-w-full divide-y divide-gray-200">
                                    <thead className="bg-gray-50">
                                        <tr>
                                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Satır</th>
                                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Durum</th>
                                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">SKU / Alan</th>
                                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Mesaj</th>
                                        </tr>
                                    </thead>
                                    <tbody className="bg-white divide-y divide-gray-200">
                                        {result.errors.map((error, idx) => (
                                            <tr key={`err-${idx}`}>
                                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{error.rowNumber}</td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <span className="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-red-100 text-red-800">
                                                        {error.severity || 'Error'}
                                                    </span>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                                    {error.sku && <span className="block font-mono text-xs">{error.sku}</span>}
                                                    {error.field && <span className="block text-xs text-gray-400">{error.field}</span>}
                                                </td>
                                                <td className="px-6 py-4 text-sm text-red-600">{error.message}</td>
                                            </tr>
                                        ))}
                                        {result.warnings && result.warnings.map((warning, idx) => (
                                            <tr key={`warn-${idx}`}>
                                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{warning.rowNumber}</td>
                                                <td className="px-6 py-4 whitespace-nowrap">
                                                    <span className="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-orange-100 text-orange-800">
                                                        Warning
                                                    </span>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                                    {warning.sku && <span className="block font-mono text-xs">{warning.sku}</span>}
                                                </td>
                                                <td className="px-6 py-4 text-sm text-orange-600">{warning.message}</td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    )}
                </div>
            )}
        </div>
    )
}
