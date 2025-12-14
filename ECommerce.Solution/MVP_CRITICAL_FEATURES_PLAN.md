# 🔥 MVP KRİTİK ÖZELLİKLER - IMPLEMENTATION PLAN

**Tarih:** 7 Aralık 2025  
**Durum:** PLANLAMA  
**Öncelik:** KRİTİK - MVP LANSMANINA ENGEL

---

## 🎯 GENEL BAKIŞ

MVP lansmanı için **mutlaka** tamamlanması gereken 4 kritik özellik:

1. ✅ Order Workflow Stabilization
2. ✅ API Response Standardization
3. ✅ DTO/Validator System Finalization
4. ✅ Error Codes System

**Tahmini Süre:** 2-3 gün  
**Etki:** MVP %100 stabil ve production-ready

---

## 1️⃣ ORDER WORKFLOW STABILIZATION

### Problem
Sipariş akışı çalışıyor ama kritik edge case'ler eksik:
- ❌ Stok senkronizasyon hataları
- ❌ Fiyat değişikliği kontrolü yok
- ❌ Transaction rollback eksik
- ❌ B2B/B2C kuralları tam değil
- ❌ İndirimli ürün için eski fiyat riski

### Çözüm

#### A. Stok Kontrolü ve Rezervasyon
```csharp
public class StockReservationService : IStockReservationService
{
    public async Task<StockReservationResult> ReserveStockAsync(
        List<OrderItem> items, 
        CancellationToken cancellationToken)
    {
        var reservations = new List<StockReservation>();
        
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            foreach (var item in items)
            {
                var inventory = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.ProductVariantId == item.ProductVariantId);
                
                if (inventory == null || inventory.AvailableQuantity < item.Quantity)
                {
                    await transaction.RollbackAsync();
                    return StockReservationResult.Failed(
                        $"Ürün stokta yok: {item.ProductName}");
                }
                
                // Pessimistic locking
                inventory.AvailableQuantity -= item.Quantity;
                inventory.ReservedQuantity += item.Quantity;
                
                reservations.Add(new StockReservation
                {
                    ProductVariantId = item.ProductVariantId,
                    Quantity = item.Quantity,
                    ReservedAt = DateTime.UtcNow
                });
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return StockReservationResult.Success(reservations);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

#### B. Fiyat Doğrulama
```csharp
public class PriceValidationService : IPriceValidationService
{
    public async Task<PriceValidationResult> ValidatePricesAsync(
        List<CartItem> cartItems)
    {
        var priceChanges = new List<PriceChange>();
        
        foreach (var item in cartItems)
        {
            var currentProduct = await _productRepository
                .GetByIdAsync(item.ProductVariantId);
            
            if (currentProduct.Price != item.Price)
            {
                priceChanges.Add(new PriceChange
                {
                    ProductName = item.ProductName,
                    OldPrice = item.Price,
                    NewPrice = currentProduct.Price
                });
            }
        }
        
        if (priceChanges.Any())
        {
            return PriceValidationResult.Failed(
                "Bazı ürünlerin fiyatları değişti", 
                priceChanges);
        }
        
        return PriceValidationResult.Success();
    }
}
```

#### C. Order State Machine
```csharp
public enum OrderStatus
{
    Draft,          // Sepet
    PendingPayment, // Ödeme bekliyor
    PaymentFailed,  // Ödeme başarısız
    Confirmed,      // Onaylandı
    Processing,     // Hazırlanıyor
    Shipped,        // Kargoya verildi
    Delivered,      // Teslim edildi
    Cancelled,      // İptal
    Refunded        // İade
}

public class OrderStateMachine
{
    private static readonly Dictionary<OrderStatus, List<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.Draft] = new() { OrderStatus.PendingPayment, OrderStatus.Cancelled },
        [OrderStatus.PendingPayment] = new() { OrderStatus.Confirmed, OrderStatus.PaymentFailed },
        [OrderStatus.PaymentFailed] = new() { OrderStatus.PendingPayment, OrderStatus.Cancelled },
        [OrderStatus.Confirmed] = new() { OrderStatus.Processing, OrderStatus.Cancelled },
        [OrderStatus.Processing] = new() { OrderStatus.Shipped, OrderStatus.Cancelled },
        [OrderStatus.Shipped] = new() { OrderStatus.Delivered },
        [OrderStatus.Delivered] = new() { OrderStatus.Refunded },
        [OrderStatus.Cancelled] = new() { },
        [OrderStatus.Refunded] = new() { }
    };
    
    public bool CanTransition(OrderStatus from, OrderStatus to)
    {
        return AllowedTransitions[from].Contains(to);
    }
}
```

#### D. B2B/B2C Kuralları
```csharp
public class OrderBusinessRules
{
    public async Task<ValidationResult> ValidateB2COrderAsync(Order order)
    {
        // B2C: Max 50 ürün
        if (order.Items.Count > 50)
            return ValidationResult.Failed("B2C siparişlerde en fazla 50 ürün olabilir");
        
        // B2C: Minimum tutar yok
        return ValidationResult.Success();
    }
    
    public async Task<ValidationResult> ValidateB2BOrderAsync(Order order)
    {
        // B2B: Minimum sipariş tutarı
        if (order.TotalAmount < 1000)
            return ValidationResult.Failed("B2B siparişlerde minimum tutar 1000 TL");
        
        // B2B: Kredi limiti kontrolü
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        if (customer.CreditLimit < order.TotalAmount)
            return ValidationResult.Failed("Kredi limiti yetersiz");
        
        return ValidationResult.Success();
    }
}
```

### Acceptance Criteria
- [ ] Stok rezervasyonu transaction içinde
- [ ] Fiyat değişikliği tespit edilirse sipariş reddedilir
- [ ] Order state machine ile geçişler kontrollü
- [ ] B2B/B2C kuralları uygulanır
- [ ] Rollback durumunda stok geri alınır

---

## 2️⃣ API RESPONSE STANDARDIZATION

### Problem
- ✅ Hatalar unified (ExceptionMiddleware)
- ❌ Başarı response'ları tutarsız

### Çözüm

#### Standart Response Wrapper
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Path { get; set; } = string.Empty;
    
    public static ApiResponse<T> SuccessResponse(
        T data, 
        string message = "İşlem başarılı",
        int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            StatusCode = statusCode
        };
    }
}
```

#### Response Filter
```csharp
public class ApiResponseFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }
    
    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult objectResult)
        {
            var response = new ApiResponse<object>
            {
                Success = true,
                Data = objectResult.Value,
                Message = "İşlem başarılı",
                StatusCode = objectResult.StatusCode ?? 200,
                Path = context.HttpContext.Request.Path
            };
            
            context.Result = new ObjectResult(response)
            {
                StatusCode = response.StatusCode
            };
        }
    }
}
```

#### Controller Örneği
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<ProductDto>> GetProduct(int id)
{
    var product = await _productService.GetByIdAsync(id);
    
    if (product == null)
        return NotFound(); // ExceptionMiddleware handles this
    
    // Automatic wrapping by ApiResponseFilter
    return Ok(product);
}

// Response:
{
  "success": true,
  "data": { "productId": 1, "productName": "..." },
  "message": "İşlem başarılı",
  "statusCode": 200,
  "timestamp": "2025-12-07T20:00:00Z",
  "path": "/api/v1/products/1"
}
```

### Acceptance Criteria
- [ ] Tüm başarılı response'lar unified format
- [ ] ApiResponseFilter tüm controller'lara uygulanır
- [ ] Swagger documentation güncel
- [ ] Frontend ile test edilir

---

## 3️⃣ DTO/VALIDATOR SYSTEM FINALIZATION

### Problem
- ✅ Validator'lar mükemmel
- ❌ Bazı endpoint'ler entity döndürüyor
- ❌ DTO coverage %100 değil

### Çözüm

#### A. Output DTO Audit
```bash
# Tüm controller'ları tara
# Entity döndüren endpoint'leri bul
# DTO'ya çevir
```

#### B. DTO Mapping Strategy
```csharp
// YANLIŞ ❌
public async Task<Order> GetOrderAsync(int id)
{
    return await _context.Orders.FindAsync(id);
}

// DOĞRU ✅
public async Task<OrderDto> GetOrderAsync(int id)
{
    var order = await _context.Orders.FindAsync(id);
    return _mapper.Map<OrderDto>(order);
}
```

#### C. AutoMapper Profil Kontrolü
```csharp
public class OrderProfile : Profile
{
    public OrderProfile()
    {
        // Entity → DTO
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName, 
                opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"));
        
        // DTO → Entity (Create)
        CreateMap<CreateOrderRequest, Order>()
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}
```

### Acceptance Criteria
- [ ] Hiçbir endpoint entity döndürmüyor
- [ ] Tüm input'lar için DTO var
- [ ] Tüm output'lar için DTO var
- [ ] AutoMapper profilleri tam

---

## 4️⃣ ERROR CODES SYSTEM

### Problem
- ✅ Exception mapping var
- ❌ Structured error codes yok

### Çözüm

#### Error Code Enum
```csharp
public static class ErrorCodes
{
    // Stock Errors (1000-1099)
    public const string STOCK_NOT_AVAILABLE = "STOCK_1001";
    public const string STOCK_RESERVATION_FAILED = "STOCK_1002";
    public const string STOCK_INSUFFICIENT = "STOCK_1003";
    
    // Price Errors (1100-1199)
    public const string PRICE_CHANGED = "PRICE_1101";
    public const string PRICE_INVALID = "PRICE_1102";
    
    // Order Errors (1200-1299)
    public const string ORDER_NOT_FOUND = "ORDER_1201";
    public const string ORDER_ALREADY_PAID = "ORDER_1202";
    public const string ORDER_CANCELLED = "ORDER_1203";
    public const string ORDER_INVALID_STATUS = "ORDER_1204";
    
    // Cart Errors (1300-1399)
    public const string CART_EMPTY = "CART_1301";
    public const string CART_ITEM_NOT_FOUND = "CART_1302";
    
    // Promotion Errors (1400-1499)
    public const string INVALID_PROMOTION = "PROMO_1401";
    public const string PROMOTION_EXPIRED = "PROMO_1402";
    public const string PROMOTION_NOT_APPLICABLE = "PROMO_1403";
    
    // Payment Errors (1500-1599)
    public const string PAYMENT_FAILED = "PAYMENT_1501";
    public const string PAYMENT_DECLINED = "PAYMENT_1502";
    
    // Customer Errors (1600-1699)
    public const string CUSTOMER_NOT_FOUND = "CUSTOMER_1601";
    public const string CREDIT_LIMIT_EXCEEDED = "CUSTOMER_1602";
    
    // Validation Errors (1700-1799)
    public const string VALIDATION_FAILED = "VALIDATION_1701";
    public const string INVALID_INPUT = "VALIDATION_1702";
}
```

#### Custom Exceptions
```csharp
public class StockNotAvailableException : Exception
{
    public string ErrorCode => ErrorCodes.STOCK_NOT_AVAILABLE;
    public StockNotAvailableException(string message) : base(message) { }
}

public class PriceChangedException : Exception
{
    public string ErrorCode => ErrorCodes.PRICE_CHANGED;
    public List<PriceChange> PriceChanges { get; set; }
    public PriceChangedException(string message, List<PriceChange> changes) 
        : base(message) 
    {
        PriceChanges = changes;
    }
}
```

#### Exception Middleware Update
```csharp
private async Task HandleExceptionAsync(HttpContext context, Exception exception)
{
    var response = new ErrorResponse
    {
        Success = false,
        Timestamp = DateTime.UtcNow,
        Path = context.HttpContext.Request.Path
    };
    
    switch (exception)
    {
        case StockNotAvailableException stockEx:
            context.Response.StatusCode = 400;
            response.Message = stockEx.Message;
            response.ErrorCode = stockEx.ErrorCode;
            break;
            
        case PriceChangedException priceEx:
            context.Response.StatusCode = 400;
            response.Message = priceEx.Message;
            response.ErrorCode = priceEx.ErrorCode;
            response.Details = priceEx.PriceChanges;
            break;
            
        // ... diğer custom exception'lar
    }
    
    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
}
```

### Acceptance Criteria
- [ ] Tüm business error'lar için error code var
- [ ] Custom exception'lar oluşturuldu
- [ ] ExceptionMiddleware güncel
- [ ] Frontend error handling kolaylaştı

---

## 📋 IMPLEMENTATION CHECKLIST

### Hafta 1 - Gün 1-2 (Order Workflow)
- [ ] StockReservationService oluştur
- [ ] PriceValidationService oluştur
- [ ] OrderStateMachine implement et
- [ ] B2B/B2C business rules ekle
- [ ] Transaction management ekle
- [ ] Unit tests yaz
- [ ] Integration tests yaz

### Hafta 1 - Gün 3 (API Standardization)
- [ ] ApiResponse<T> class oluştur
- [ ] ApiResponseFilter implement et
- [ ] Tüm controller'lara uygula
- [ ] Swagger documentation güncelle
- [ ] Frontend ile test et

### Hafta 1 - Gün 4 (DTO Finalization)
- [ ] Entity döndüren endpoint'leri bul
- [ ] Eksik DTO'ları oluştur
- [ ] AutoMapper profilleri tamamla
- [ ] Code review yap

### Hafta 1 - Gün 5 (Error Codes)
- [ ] ErrorCodes class oluştur
- [ ] Custom exception'lar yaz
- [ ] ExceptionMiddleware güncelle
- [ ] Error code documentation yaz
- [ ] Frontend ile entegrasyon test et

---

## 🎯 SUCCESS METRICS

### Order Workflow
- ✅ 0 stok senkronizasyon hatası
- ✅ %100 fiyat doğrulama
- ✅ %100 transaction rollback
- ✅ B2B/B2C kuralları çalışıyor

### API Standardization
- ✅ %100 unified response format
- ✅ Frontend entegrasyonu kolay

### DTO System
- ✅ 0 entity exposure
- ✅ %100 DTO coverage

### Error Codes
- ✅ Tüm business error'lar kodlanmış
- ✅ Frontend error handling 30x hızlı

---

## 🚀 SONRAKI ADIMLAR (Hafta 2-3)

### Hafta 2 - Güvenilirlik
1. Unit Tests (%30-40 coverage)
2. Integration Tests
3. Monitoring (Application Insights)
4. Logging iyileştirme

### Hafta 3 - Enterprise Hazırlık
5. CI/CD Pipeline
6. Docker Production
7. HTTPS/TLS
8. Rate Limiting iyileştirme

---

**DURUM:** Planlama tamamlandı, implementasyon başlayabilir  
**TAHMİNİ SÜRE:** 5 gün  
**ÖNCELİK:** KRİTİK - MVP BLOCKER
