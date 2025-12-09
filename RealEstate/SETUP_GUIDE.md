# RealEstate Projesi - Kurulum ve Çalıştırma Rehberi

## 📦 Gereksinimler

- **.NET SDK**: 8.0 veya üzeri
- **Git**: Versiyon kontrol için
- **VS Code** veya **Visual Studio**
- **Tarayıcı**: Chrome, Firefox, Edge, Safari

Kurulu olup olmadığını kontrol et:

```bash
dotnet --version
git --version
```

## 🔧 Kurulum Adımları

### 1. Repository'yi Klonla

```bash
git clone <repository-url>
cd RealEstate
```

### 2. Dependencies Yükle

```bash
# API projesi
cd RealEstate.Api
dotnet restore

# Web projesi
cd ../RealEstate.Web
dotnet restore
```

### 3. Projeyi Derle

```bash
cd ..
dotnet build
```

## 🚀 Uygulamayı Çalıştırma

### Yöntem 1: Terminal ile (Önerilen)

**Terminal 1 - API Sunucusu:**

```bash
cd RealEstate/RealEstate.Api
dotnet run
```

✅ Çıktı: `Now listening on: https://localhost:7180`

**Terminal 2 - Web Uygulaması:**

```bash
cd RealEstate/RealEstate.Web
dotnet run
```

✅ Çıktı: `Now listening on: https://localhost:7173`

### Yöntem 2: IDE ile

#### Visual Studio

1. Solution dosyasını aç: `RealEstate.sln`
2. Sağ tıkla → Set Startup Projects
3. Seç: "Multiple startup projects"
4. Her iki projeyi de "Start" olarak ayarla
5. F5 tuşuna bas

#### VS Code

1. Terminalden: `cd RealEstate`
2. Her projede ayrı terminalden `dotnet run` çalıştır

## 🌐 Uygulamaya Erişim

| Uygulama   | URL                                   | Açıklama            |
| ---------- | ------------------------------------- | ------------------- |
| Web Sitesi | https://localhost:7173                | Ana sayfa           |
| API        | https://localhost:7180/swagger        | API dökümentasyonu  |
| API Base   | https://localhost:7180/api/properties | Property endpoint'i |

## ✅ Başarılı Kurulum Kontrolü

1. **API Sağlıyor mu?**

   ```bash
   curl https://localhost:7180/api/properties --insecure
   ```

   Çıktı: JSON array ile 2 emlak ilanı görüntülenmeli

2. **Web Uygulaması Yükleniyor mu?**
   - https://localhost:7173 adresini tarayıcıda aç
   - "Hayalinizdeki Evi Bulun" başlığını gör
   - 2 adet emlak kartı görünmeli

## 🔑 Port Yapılandırması

Eğer portlar zaten kullanımdaysa:

### RealEstate.Api

Dosya: `RealEstate.Api/Properties/launchSettings.json`

```json
"applicationUrl": "https://localhost:7180;http://localhost:5180"
```

### RealEstate.Web

Dosya: `RealEstate.Web/Properties/launchSettings.json`

```json
"applicationUrl": "https://localhost:7173;http://localhost:5173"
```

**Sonra appsettings.json'ı güncelle:**

```bash
# RealEstate.Web/appsettings.json
"ApiSettings": {
  "BaseUrl": "https://localhost:7180"
}
```

## 🐛 Sık Karşılaşılan Sorunlar

### Problem: "Connection refused" hatası

**Çözüm:**

1. API sunucusu çalışıyor mu kontrol et
2. Portların doğru olup olmadığını doğrula
3. Firewall ayarlarını kontrol et

### Problem: SSL/TLS sertifikası hatası

**Çözüm:**

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Problem: "DbContext not found" hatası (gelecek)

**Çözüm:** Veritabanı migrate işlemini çalıştır

```bash
dotnet ef database update
```

### Problem: Bootstrap/CSS yüklenmedi

**Çözüm:**

```bash
cd RealEstate.Web
dotnet restore
```

## 📊 Proje Yapısı

```
RealEstate/
├── RealEstate.Api/
│   ├── Controllers/
│   │   └── PropertiesController.cs    ← API endpoint'leri
│   ├── Models/
│   │   └── Property.cs                ← Veri modeli
│   ├── Program.cs                     ← Konfigürasyon
│   └── RealEstate.Api.csproj
│
├── RealEstate.Web/
│   ├── Controllers/
│   │   └── HomeController.cs          ← Sayfa kontrolörü
│   ├── Models/
│   │   └── Property.cs                ← Veri modeli
│   ├── Services/
│   │   └── PropertyApiClient.cs       ← API istemci
│   ├── Views/
│   │   └── Home/
│   │       └── Index.cshtml           ← Ana sayfa
│   ├── wwwroot/                       ← Statik dosyalar
│   ├── Program.cs                     ← Konfigürasyon
│   └── RealEstate.Web.csproj
│
├── RealEstate.sln                     ← Solution dosyası
└── DEVELOPERS_NOTES.md                ← Geliştirici notları
```

## 🚦 Geliştirme Workflow

1. **Yeni feature için branch oluştur:**

   ```bash
   git checkout -b feature/yeni-ozellik
   ```

2. **Değişiklikleri yap:**

   - API tarafında: `RealEstate.Api/` klasöründe çalış
   - Web tarafında: `RealEstate.Web/` klasöründe çalış

3. **Test et:**

   ```bash
   dotnet test
   ```

4. **Commit ve push yap:**
   ```bash
   git add .
   git commit -m "feat: yeni özellik açıklaması"
   git push origin feature/yeni-ozellik
   ```

## 🔄 Bilgisayar Değiştirirken

Yeni bir bilgisayara geçildiğinde:

1. Repository'yi klonla
2. `dotnet restore` çalıştır
3. SSL sertifikasını yenile: `dotnet dev-certs https --trust`
4. Uygulamayı çalıştır

## 💾 Veritabanı Hazırlığı (Gelecek)

Entity Framework Core setup'ı için:

```bash
# NuGet package'i ekle
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# Migration oluştur
dotnet ef migrations add InitialCreate

# Veritabanını oluştur
dotnet ef database update
```

## 📚 Kaynaklar

- [ASP.NET Core Dökümentasyonu](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Bootstrap 5](https://getbootstrap.com)
- [Razor Syntax](https://docs.microsoft.com/aspnet/core/mvc/views/razor)

## 🎯 Sonraki Adımlar

- [ ] Veritabanı bağlantısı kurulacak
- [ ] Authentication sistemi eklenecek
- [ ] Unit testler yazılacak
- [ ] Error handling geliştirileceğek
- [ ] Frontend validasyonları eklenecek

---

**Son Güncelleme:** 9 Aralık 2025
