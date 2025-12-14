import { X } from 'lucide-react'
import { useEffect, useRef } from 'react'

interface ModalProps {
    isOpen: boolean
    onClose: () => void
    title: string
    children: React.ReactNode
    footer?: React.ReactNode
    size?: 'sm' | 'md' | 'lg' | 'xl'
}

export default function Modal({
    isOpen,
    onClose,
    title,
    children,
    footer,
    size = 'md',
}: ModalProps) {
    const modalRef = useRef<HTMLDivElement>(null)

    // Close on escape key
    useEffect(() => {
        const handleEscape = (e: KeyboardEvent) => {
            if (e.key === 'Escape') onClose()
        }

        if (isOpen) {
            document.addEventListener('keydown', handleEscape)
            document.body.style.overflow = 'hidden' // Prevent scrolling
        }

        return () => {
            document.removeEventListener('keydown', handleEscape)
            document.body.style.overflow = 'unset'
        }
    }, [isOpen, onClose])

    if (!isOpen) return null

    const sizeClasses = {
        sm: 'max-w-md',
        md: 'max-w-xl',
        lg: 'max-w-2xl',
        xl: 'max-w-4xl',
    }

    return (
        <div className="fixed inset-0 z-50 overflow-y-auto" aria-labelledby="modal-title" role="dialog" aria-modal="true">
            {/* Backdrop */}
            <div
                className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity"
                onClick={onClose}
                aria-hidden="true"
            ></div>

            {/* Modal Panel */}
            <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
                <div
                    ref={modalRef}
                    className={`relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all sm:my-8 w-full ${sizeClasses[size]}`}
                >
                    {/* Header */}
                    <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4 border-b border-gray-100">
                        <div className="sm:flex sm:items-center sm:justify-between">
                            <h3 className="text-lg font-semibold leading-6 text-gray-900" id="modal-title">
                                {title}
                            </h3>
                            <div className="mt-3 sm:mt-0 sm:ml-4 sm:text-right">
                                <button
                                    type="button"
                                    className="rounded-md bg-white text-gray-400 hover:text-gray-500 focus:outline-none"
                                    onClick={onClose}
                                >
                                    <span className="sr-only">Kapat</span>
                                    <X className="h-6 w-6" aria-hidden="true" />
                                </button>
                            </div>
                        </div>
                    </div>

                    {/* Content */}
                    <div className="bg-white px-4 pt-5 pb-4 sm:p-6">
                        {children}
                    </div>

                    {/* Footer */}
                    {footer && (
                        <div className="bg-gray-50 px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6 border-t border-gray-100">
                            {footer}
                        </div>
                    )}
                </div>
            </div>
        </div>
    )
}

interface ConfirmModalProps {
    isOpen: boolean
    onClose: () => void
    onConfirm: () => void
    title: string
    message: string
    confirmText?: string
    cancelText?: string
    variant?: 'primary' | 'danger'
    isLoading?: boolean
}

export function ConfirmModal({
    isOpen,
    onClose,
    onConfirm,
    title,
    message,
    confirmText = 'Onayla',
    cancelText = 'İptal',
    variant = 'primary',
    isLoading = false
}: ConfirmModalProps) {
    return (
        <Modal isOpen={isOpen} onClose={onClose} title={title} size="sm">
            <div className="mt-2">
                <p className="text-sm text-gray-500">
                    {message}
                </p>
            </div>
            <div className="mt-5 sm:mt-6 sm:grid sm:grid-flow-row-dense sm:grid-cols-2 sm:gap-3">
                <button
                    type="button"
                    className={`inline-flex w-full justify-center rounded-md px-3 py-2 text-sm font-semibold text-white shadow-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 sm:col-start-2 ${variant === 'danger'
                            ? 'bg-red-600 hover:bg-red-500 focus-visible:outline-red-600'
                            : 'bg-primary-600 hover:bg-primary-500 focus-visible:outline-primary-600'
                        } ${isLoading ? 'opacity-70 cursor-not-allowed' : ''}`}
                    onClick={() => {
                        if (!isLoading) onConfirm();
                    }}
                    disabled={isLoading}
                >
                    {isLoading ? (
                        <span className="flex items-center">
                            <span className="animate-spin h-4 w-4 border-2 border-white border-b-transparent rounded-full mr-2"></span>
                            İşleniyor...
                        </span>
                    ) : confirmText}
                </button>
                <button
                    type="button"
                    className="mt-3 inline-flex w-full justify-center rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50 sm:col-start-1 sm:mt-0"
                    onClick={onClose}
                    disabled={isLoading}
                >
                    {cancelText}
                </button>
            </div>
        </Modal>
    )
}
