# RealEstate Projesi - Geliştirici Notları

## 📋 Proje Yapısı

```
RealEstate/
├── RealEstate.Api/       (ASP.NET Core Web API)
│   ├── Controllers/      (API endpoint'leri)
│   ├── Models/           (Veri modelleri)
│   └── Program.cs        (Başlangıç konfigürasyonu)
│
└── RealEstate.Web/       (ASP.NET Core MVC Web Uygulaması)
    ├── Controllers/      (Sayfa kontrolörleri)
    ├── Models/           (Veri modelleri)
    ├── Services/         (API entegrasyonu)
    ├── Views/            (Razor templates)
    └── wwwroot/          (Statik dosyalar)
```

## 🚀 Başlama

### Portlar

- **RealEstate.Api**: `http://localhost:5180` (HTTP) / `https://localhost:7180` (HTTPS)
- **RealEstate.Web**: `http://localhost:5173` (HTTP) / `https://localhost:7173` (HTTPS)

### Çalıştırma (HTTP - Development)

#### Terminal 1 - API Sunucusu

```bash
cd RealEstate/RealEstate.Api
dotnet run --launch-profile http
```

✅ Çıktı: `Now listening on: http://localhost:5180`

#### Terminal 2 - Web Uygulaması

```bash
cd RealEstate/RealEstate.Web
dotnet run --launch-profile http
```

✅ Çıktı: `Now listening on: http://localhost:5173`

Ardından tarayıcıda açın: **http://localhost:5173**

### Çalıştırma (HTTPS - Production/Testing)

#### Terminal 1 - API Sunucusu

```bash
cd RealEstate/RealEstate.Api
dotnet run --launch-profile https
```

#### Terminal 2 - Web Uygulaması

```bash
cd RealEstate/RealEstate.Web
dotnet run --launch-profile https
```

Ardından tarayıcıda açın: **https://localhost:7173**

**Not:** HTTPS kullanmadan önce SSL sertifikasını güvenilir hale getirin:

```bash
dotnet dev-certs https --trust
```

## ✅ Son Düzeltlemeler ve İyileştirmeler (9 Aralık 2025)

### 1. **CORS Desteği Eklendi**

- `RealEstate.Api/Program.cs` dosyasında CORS yapılandırması eklenmiştir
- Web uygulaması artık API'ye sorunsuzca bağlanabilir
- **Policy Adı**: `AllowWeb`
- **İzin Verilen Kaynaklar**:
  - `http://localhost:5173`
  - `https://localhost:7173`

### 2. **Konfigürasyon Merkezileştirildi**

- API Base URL artık `appsettings.json` dosyasında tanımlanır
- Development: `http://localhost:5180`
- Üretim ortamında port değişikliği sadece config dosyasından yapılır
- **Ayar Yolu**: `ApiSettings:BaseUrl`

### 3. **Launch Profiles Düzeltildi**

- `RealEstate.Api/Properties/launchSettings.json`:
  - HTTPS port'u: `7027` → `7180` (sabitlendi)
  - HTTP port'u: `5180` (korundu)
- `RealEstate.Web/Properties/launchSettings.json`:
  - HTTPS port'u: `7005` → `7173` (sabitlendi)
  - HTTP port'u: `5173` (korundu)

### 4. **Hata Yönetimi İyileştirildi**

- `PropertyApiClient` sınıfına logging eklendi
- HTTP bağlantı hataları detaylı log'lanıyor
- `PropertiesController` da exception handling ve response type documentation eklendi
- Test edildi: API başarıyla 2 emlak ilanı döndürüyor ✅

### 5. **Dokumentasyon Eklendi**

- XML documentation yorumları tüm public metodlara eklenmiştir
- API endpoint'leri `ProducesResponseType` ile belgelenmiştir
- Swagger/OpenAPI desteği tam olarak aktif (http://localhost:5180/swagger)

### 6. **HTML/Razor Hataları Düzeltildi**

- `Index.cshtml` dosyasında `@(item.RoomCount + 1)` ifadesi düzeltildi
- Artık RoomCount değeri doğru şekilde görüntüleniyor (3 oda olarak gösterilme)
- Sayfa başarıyla API'den veri alıyor ve gösteriyor ✅

## 📁 Önemli Dosyalar

| Dosya                                                | Açıklama                                             |
| ---------------------------------------------------- | ---------------------------------------------------- |
| `RealEstate.Api/Program.cs`                          | API başlangıç konfigürasyonu, CORS ayarları          |
| `RealEstate.Web/Program.cs`                          | Web uygulaması başlangıcı, HttpClient konfigürasyonu |
| `RealEstate.Web/appsettings.json`                    | API URL konfigürasyonu                               |
| `RealEstate.Web/Services/PropertyApiClient.cs`       | API entegrasyon sınıfı                               |
| `RealEstate.Api/Controllers/PropertiesController.cs` | Emlak ilanları API endpoint'i                        |
| `RealEstate.Web/Views/Home/Index.cshtml`             | Ana sayfa görünümü                                   |

## 🔧 Yaygın Görevler

### API'yi Test Etme

#### Swagger UI

```
HTTP: http://localhost:5180/swagger
HTTPS: https://localhost:7180/swagger
```

#### cURL ile

```bash
# HTTP
curl http://localhost:5180/api/properties

# HTTPS (SSL doğrulaması yok)
curl -k https://localhost:7180/api/properties
```

### Yeni Property Eklemek

1. `RealEstate.Api/Controllers/PropertiesController.cs` dosyasındaki `GetAll()` metodunda liste genişletin
2. Sahte veriyi gerçek veritabanı ile değiştirin (Entity Framework Core)

### API URL'sini Değiştirme

`RealEstate.Web/appsettings.json` dosyasında:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5180" // ← Burası değişir
  }
}
```

### Stil Değişiklikleri

- CSS dosyaları: `RealEstate.Web/wwwroot/css/`
- Bootstrap 5 zaten entegre edilmiştir
- Scoped CSS: `RealEstate.Web/Views/Shared/_Layout.cshtml.css`

### Logging Seviyesini Değiştirme

`appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug", // Information, Warning, Error
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## ⚠️ Bilinen Sorunlar ve Yapılacaklar

- [ ] Veritabanı entegrasyonu (şu anda mock data kullanılıyor)
- [ ] Authentication/Authorization
- [ ] Unit test yazımı
- [ ] Error logging provider (Serilog vs)
- [ ] Frontend form validasyonları
- [ ] Responsive tasarım iyileştirmeleri

## 💡 Best Practices

### 1. **Dependency Injection Kullanın**

```csharp
// ✅ Doğru
public class HomeController : Controller
{
    private readonly PropertyApiClient _client;

    public HomeController(PropertyApiClient client)
    {
        _client = client;
    }
}

// ❌ Yanlış - Hard dependency
var client = new PropertyApiClient(httpClient);
```

### 2. **Error Handling**

```csharp
// ✅ Doğru - try-catch ve logging
try
{
    var result = await _httpClient.GetFromJsonAsync<List<Property>>("api/properties");
    _logger.LogInformation("Başarı: {Count} ilan getirildi", result?.Count ?? 0);
    return result ?? new List<Property>();
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "API bağlantısı başarısız");
    return new List<Property>();
}
```

### 3. **Configuration Management**

```csharp
// ✅ Doğru - Configuration'dan oku
var apiUrl = builder.Configuration["ApiSettings:BaseUrl"];

// ❌ Yanlış - Hard-coded değerler
var apiUrl = "http://localhost:5180";
```

### 4. **API Entegrasyonu**

```csharp
// PropertyApiClient sınıfını genişlet
public async Task<Property> GetPropertyByIdAsync(int id)
{
    try
    {
        return await _httpClient.GetFromJsonAsync<Property>($"api/properties/{id}");
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "ID {Id} için property alınamadı", id);
        return null;
    }
}
```

### 5. **View Model Pattern**

```csharp
// DTO kullan, direkt model gönderme
public class PropertyViewModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string FormattedPrice { get; set; }  // UI için formatlanmış
}
```

### 6. **Entity Framework (Gelecek)**

```csharp
// DbContext kullan, in-memory list yerine
public class RealEstateDbContext : DbContext
{
    public DbSet<Property> Properties { get; set; }
}
```

## 📚 Kaynaklar ve Referanslar

- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [HttpClient Best Practices](https://docs.microsoft.com/dotnet/fundamentals/networking/http/httpclient)
- [Dependency Injection](https://docs.microsoft.com/aspnet/core/fundamentals/dependency-injection)
- [Logging](https://docs.microsoft.com/aspnet/core/fundamentals/logging)
- [Bootstrap 5](https://getbootstrap.com)
- [Razor Syntax](https://docs.microsoft.com/aspnet/core/mvc/views/razor)

## 📞 Sorun Çözme Rehberi

### Problem: "Şu anda gösterilecek ilan bulunamadı" hatası

**Çözüm Adımları:**

1. API sunucusunun çalışıp çalışmadığını kontrol et:
   ```bash
   curl http://localhost:5180/api/properties
   ```
2. Yanıt JSON ise API iyi çalışıyor
3. Hata alıyorsan:
   - Port numarasını kontrol et
   - API process'ini yeniden başlat
   - `appsettings.json` dosyasını kontrol et

### Problem: Port zaten kullanımda

```bash
# Çalışan dotnet process'lerini kapat
pkill -f "dotnet run"

# veya belirli port:
lsof -i :5180     # Hangi process kullanıyor?
kill -9 <PID>     # Process'i durdur
```

### Problem: SSL/TLS Hatası

```bash
# Sertifikayı güvenilir hale getir
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Problem: NuGet Package Hataları

```bash
cd RealEstate
dotnet clean
dotnet restore
dotnet build
```

### Konsol Log'larını İnceleme

API çalıştırırken çıkan log'lar:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5180
```

Eğer bu satırı görmezsen port zaten kullanımdadır.

## ✨ Başarı Kontrol Listesi

- [x] API HTTP'de 5180 portunda çalışıyor
- [x] Web HTTP'de 5173 portunda çalışıyor
- [x] Web sayfası API'den veri çekerek gösteriyor
- [x] 2 emlak ilanı (Manisa, İzmir) başarıyla görüntüleniyor
- [x] CORS yapılandırması ayarlanmış
- [x] Logging entegre edilmiş
- [x] Error handling implementasyonu yapılmış
- [x] Swagger dokumentasyonu aktif
