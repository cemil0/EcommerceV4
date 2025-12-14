# ASP.NET Core MVC Backend Geliştirme - Görev Listesi

## 📋 Proje Kurulumu
- [x] Solution ve proje yapısı oluşturma
- [x] NuGet paketlerini yükleme
- [x] Veritabanı bağlantısı yapılandırma
- [x] Entity Framework Core setup

## 🗂️ Domain Layer
- [x] Entity sınıfları oluşturma (10 entity)
- [x] Navigation properties tanımlama
- [x] Enums (CustomerType, OrderStatus, OrderType)

## 💾 Data Access Layer
- [x] DbContext + Configurations
- [x] Repository Pattern (12 interfaces + implementations)
- [x] Unit of Work Pattern

## 🎯 Business Logic Layer
- [x] Service interfaces (4)
- [x] Service implementations (Product, Category, Cart, Order)
- [x] DTOs (CommonDTOs, RequestDTOs)
- [x] AutoMapper yapılandırması

## 🌐 Presentation Layer (MVC)
- [x] ProductsController + Views
- [x] CartController + Views
- [x] OrderController + Views
- [x] Seed Data (3 kategori, 4 ürün, 6 varyant)

## 🔌 API Layer
- [x] CartController (API)
- [x] OrderController (API)
- [ ] Swagger Documentation (detaylı)

## 🔐 Authentication & Authorization
- [x] Identity setup
  - [x] ApplicationUser entity
  - [x] Customer-User relationship
  - [x] Identity migration
- [x] Login/Register pages
- [x] AccountController (Register, Login, Logout)
- [x] Cart merging on login (BR-006)
- [x] JWT token (API)
  - [x] JwtService (token generation/validation)
  - [x] API AuthController
  - [x] JWT middleware
- [x] Role-based authorization
  - [x] Admin role
  - [x] Customer role
  - [x] AdminController
- [x] [Authorize] attributes
  - [x] OrderController (all endpoints)
  - [x] CartController (selective)

## ⚙️ Infrastructure
- [x] Dependency Injection
- [x] Session support
- [ ] Exception handling middleware
- [ ] Logging
- [ ] Caching

## ✅ Testing
- [ ] Unit tests
- [ ] Integration tests

## 📦 Deployment
- [ ] Configuration management
- [ ] CI/CD pipeline
