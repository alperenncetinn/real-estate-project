# 🏠 Senin Evin - Railway Deployment Guide

## 🚀 Railway'de Deploy Etme

### Adım 1: GitHub'a Push

```bash
cd /Users/alperen/Desktop/real-estate-project/real-estate-project
git add .
git commit -m "feat: Add email verification and Railway deployment"
git push origin main
```

### Adım 2: Railway'de 2 Ayrı Servis Oluştur

Railway Dashboard'da **New Project** → **Deploy from GitHub repo** seçin.

#### API Servisi:
1. New Service → GitHub Repo seçin
2. **Root Directory**: `RealEstate/RealEstate.Api`
3. **Settings** → **Deploy** → Custom Dockerfile: `../Dockerfile.api` veya Nixpacks kullanın

#### Web Servisi:
1. New Service → GitHub Repo seçin
2. **Root Directory**: `RealEstate/RealEstate.Web`
3. **Settings** → **Deploy** → Custom Dockerfile: `../Dockerfile.web` veya Nixpacks kullanın

---

## 🔑 Environment Variables

### API Servisi için:

| Variable | Değer |
|----------|-------|
| `ConnectionStrings__DefaultConnection` | Supabase PostgreSQL connection string |
| `JwtSettings__SecretKey` | Güçlü bir secret key (min 32 karakter) |
| `JwtSettings__Issuer` | `RealEstateApi` |
| `JwtSettings__Audience` | `RealEstateApp` |
| `AdminSettings__DefaultAdminEmail` | `admin@seninevin.com` |
| `AdminSettings__DefaultAdminPassword` | Güçlü bir şifre |
| `Email__SenderEmail` | `seninevinauth@gmail.com` |
| `Email__SenderPassword` | `fhxg kfdz yxsk tjnj` |

### Web Servisi için:

| Variable | Değer |
|----------|-------|
| `ApiSettings__BaseUrl` | API servisinin Railway URL'i (örn: `https://api-xxxxx.railway.app`) |

---

## 📦 Nixpacks Kullanımı (Railway Otomatik)

Railway otomatik olarak .NET projeleri algılar. Eğer Dockerfile kullanmak istemezseniz:

### API için `nixpacks.toml`:
```toml
[phases.build]
cmds = ["dotnet publish -c Release -o out"]

[start]
cmd = "dotnet out/RealEstate.Api.dll"
```

### Web için `nixpacks.toml`:
```toml
[phases.build]
cmds = ["dotnet publish -c Release -o out"]

[start]
cmd = "dotnet out/RealEstate.Web.dll"
```

---

## ⚠️ Önemli Notlar

1. **CORS Ayarları**: API'nin Program.cs dosyasında Web URL'ini eklemeyi unutmayın
2. **Database**: Supabase PostgreSQL kullanılıyor, connection string'i environment variable olarak verin
3. **Email**: Gmail App Password kullanılıyor
4. **HTTPS**: Railway otomatik SSL sağlar

---

## 🔧 Local Development

```bash
# API'yi çalıştır
cd RealEstate/RealEstate.Api
dotnet run

# Web'i çalıştır (yeni terminal)
cd RealEstate/RealEstate.Web
dotnet run
```

## 📝 Supabase'de Çalıştırılması Gereken SQL

```sql
CREATE TABLE IF NOT EXISTS verification_codes (
    id SERIAL PRIMARY KEY,
    email VARCHAR(256) NOT NULL,
    code VARCHAR(10) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    is_used BOOLEAN DEFAULT FALSE,
    type VARCHAR(50) DEFAULT 'email_verification'
);

CREATE INDEX IF NOT EXISTS idx_verification_codes_email ON verification_codes(email);
```
