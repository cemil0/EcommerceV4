# 🚨 MVP HAZIRLIK PLANI - PART 1: KRİTİK EKSİKLER (HAFTA 1)

**Hedef:** Production'a çıkmadan önce kritik eksiklikleri gidermek  
**Süre:** 1 hafta (5 gün)  
**Öncelik:** P0 (Kritik)

---

## 📊 HAFTA 1 ÖNCELİK MATRİSİ

| # | Eksiklik | Etki | Süre | Gün |
|---|----------|------|------|-----|
| 1 | API Versioning | Yüksek | 4 saat | 1 |
| 2 | Secrets Management | Kritik | 4 saat | 1 |
| 3 | Pagination | Yüksek | 8 saat | 2 |
| 4 | FluentValidation | Yüksek | 16 saat | 3-4 |
| 5 | Database Indexes | Yüksek | 8 saat | 5 |
| 6 | Exception Middleware | Orta | 4 saat | 5 |

---

## 1️⃣ GÜN 1: API VERSIONING (4 saat)

### Adım 1: NuGet Packages
```bash
dotnet add package Microsoft.AspNetCore.Mvc.Versioning --version 5.1.0
dotnet add package Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer --version 5.1.0
```

### Adım 2: Program.cs Konfigürasyonu
```csharp
// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

### Adım 3: Controller Güncelleme
```csharp
// ÖNCE:
[Route("api/[controller]")]
public class ProductsController : ControllerBase { }

// SONRA:
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase { }
```

### Adım 4: Tüm Controller'ları Güncelle
- ✅ ProductsController
- ✅ CategoriesController
- ✅ OrdersController
- ✅ CartController
- ✅ AuthController
- ✅ AdminController

---

## 2️⃣ GÜN 1: SECRETS MANAGEMENT (4 saat)

### Adım 1: .env Dosyası
```bash
# .env (GIT'E EKLEME!)
SA_PASSWORD=YourStrong@Password123
REDIS_PASSWORD=StrongRedisPassword123!
JWT_SECRET=YourSuperSecretKeyForJWTTokenGeneration
```

### Adım 2: .gitignore
```
.env
appsettings.Production.json
```

### Adım 3: docker-compose.yml
```yaml
services:
  sqlserver:
    environment:
      SA_PASSWORD: ${SA_PASSWORD}
  api:
    environment:
      Jwt__Secret: ${JWT_SECRET}
```

---

## 3️⃣ GÜN 2: PAGINATION (8 saat)

### Adım 1: Helper Sınıfları

**PagedRequest.cs**
```csharp
public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
```

**PagedResponse.cs**
```csharp
public class PagedResponse<T>
{
    public List<T> Data { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

### Adım 2: Repository Güncelleme
```csharp
public interface IRepository<T>
{
    Task<PagedResponse<T>> GetPagedAsync(PagedRequest request);
}
```

### Adım 3: Controller Güncelleme
```csharp
[HttpGet]
public async Task<ActionResult<PagedResponse<ProductDto>>> GetAll(
    [FromQuery] PagedRequest request)
{
    var products = await _productService.GetPagedAsync(request);
    return Ok(products);
}
```

---

## 4️⃣ GÜN 3-4: FLUENTVALIDATION (16 saat)

### Adım 1: NuGet Package
```bash
dotnet add package FluentValidation.AspNetCore --version 11.3.0
```

### Adım 2: Program.cs
```csharp
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

### Adım 3: Validator Sınıfları

**CreateProductDtoValidator.cs**
```csharp
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Ürün adı zorunludur")
            .MaximumLength(200);
        
        RuleFor(x => x.SKU)
            .NotEmpty()
            .Matches("^[A-Z0-9-]+$");
        
        RuleFor(x => x.CategoryId)
            .GreaterThan(0);
    }
}
```

**RegisterDtoValidator.cs**
```csharp
public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("En az bir büyük harf")
            .Matches(@"[0-9]").WithMessage("En az bir rakam");
    }
}
```

### Tüm DTO'lar için Validator Oluştur:
- ✅ CreateProductDto
- ✅ CreateProductVariantDto
- ✅ AddToCartDto
- ✅ CreateOrderDto
- ✅ RegisterDto
- ✅ LoginDto
- ✅ UpdateProfileDto

---

## 5️⃣ GÜN 5: DATABASE INDEXES (8 saat)

### Adım 1: Migration Oluştur
```bash
dotnet ef migrations add AddCriticalIndexes
```

### Adım 2: Migration Dosyası
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Products
    migrationBuilder.CreateIndex(
        name: "IX_Products_CategoryId",
        table: "Products",
        column: "CategoryId");
    
    migrationBuilder.CreateIndex(
        name: "IX_Products_SKU",
        table: "Products",
        column: "SKU",
        unique: true);
    
    // Orders
    migrationBuilder.CreateIndex(
        name: "IX_Orders_CustomerId_CreatedAt",
        table: "Orders",
        columns: new[] { "CustomerId", "CreatedAt" });
    
    // CartItems
    migrationBuilder.CreateIndex(
        name: "IX_CartItems_CartId",
        table: "CartItems",
        column: "CartId");
}
```

### Adım 3: Migration Uygula
```bash
dotnet ef database update
```

---

## 6️⃣ GÜN 5: EXCEPTION MIDDLEWARE (4 saat)

### GlobalExceptionMiddleware.cs
```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var response = new ErrorResponse
        {
            Success = false,
            StatusCode = 500,
            Message = ex.Message
        };
        
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

---

## ✅ HAFTA 1 CHECKLIST

- [ ] API Versioning uygulandı
- [ ] Secrets .env'e taşındı
- [ ] Pagination tüm endpoint'lerde
- [ ] FluentValidation tüm DTO'larda
- [ ] Database indexleri eklendi
- [ ] Exception middleware aktif

---

## 🎯 BAŞARI KRİTERLERİ

1. ✅ Tüm endpoint'ler `/api/v1/...` formatında
2. ✅ Hiçbir secret source control'de yok
3. ✅ Tüm list endpoint'lerde pagination var
4. ✅ Tüm input'lar validate ediliyor
5. ✅ Query performansı 50% arttı
6. ✅ Hatalar düzgün handle ediliyor

**Sonraki Adım:** Part 2 - Unit Tests & Monitoring (Hafta 2)
