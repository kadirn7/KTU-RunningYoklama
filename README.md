# 🏃‍♂️ KTÜ Run Yoklama Sistemi

Modern, mobil uyumlu ve güvenli web tabanlı yoklama uygulaması.

**Canlı Demo:** [kturun-yoklama-hbh7hbbgh8ababg4.westeurope-01.azurewebsites.net](https://kturun-yoklama-hbh7hbbgh8ababg4.westeurope-01.azurewebsites.net/)

---

## ✨ Özellikler

- Mobil uyumlu, modern arayüz
- Güvenli kullanıcı kayıt ve giriş
- Admin paneli ile yoklama ve kullanıcı yönetimi
- CSV formatında veri dışa aktarma
- Kod ile yoklama alma (kod süresi ve geçerliliği Türkiye saatiyle)
- Tüm zaman işlemleri Türkiye saati (UTC+3) ile uyumlu
- Şifreler SHA256 ile hash'lenir
- Session tabanlı kimlik doğrulama
- Rol tabanlı yetkilendirme (Admin/Kullanıcı)

---

## 🚀 Kurulum ve Çalıştırma

### Dotnet ile

```bash
dotnet run
```
Uygulama: http://localhost:5266

### Docker ile

```bash
docker build -t attendance-app .
docker run -p 8080:8080 attendance-app
```

### Azure, Railway, Render ile Deploy

- Azure App Service, Railway veya Render üzerinden kolayca deploy edebilirsiniz.
- [Canlı Demo](https://kturun-yoklama-hbh7hbbgh8ababg4.westeurope-01.azurewebsites.net/)

---

## 🗂️ Dosya Yapısı

```
AttendanceApp/
├── Core/
├── Infrastructure/
├── wwwroot/
│   ├── index.html, login.html, register.html, code.html, admin.html, success.html, error.html, ...
├── Data/
│   ├── users.json
│   ├── AnaSayfa.jpeg
│   ├── AdminSayfası.jpeg
│   ├── YoklamaSayfası.jpeg
├── Program.cs
└── README.md
```

---

## 👤 Kullanıcı ve Yoklama Verisi

### users.json

```json
[
  {
    "Id": 1,
    "Username": "<gizli>",
    "Email": "<gizli>",
    "PasswordHash": "<gizli>",
    "FullName": "Yönetici",
    "CreatedAt": "2025-07-06T15:14:34.9638567+03:00",
    "IsActive": true,
    "Role": "Admin"
  }
]
```

### yoklama.json

> Not: yoklama.json dosyası uygulama çalışınca otomatik oluşur ve yoklama kayıtlarını içerir.

---

## 🔑 Kullanım

1. **Kayıt Ol:** Yeni kullanıcı hesabı oluşturun
2. **Giriş Yap:** Kullanıcı adı ve şifre ile giriş yapın
3. **Yoklama Ver:** Kod ile yoklama verin
4. **Admin Paneli:** Admin hesabı ile yoklama ve kullanıcı listesini görüntüleyin, CSV dışa aktarın

---

## 🔧 API Endpoints

- `POST /api/register` - Kullanıcı kaydı
- `POST /api/login` - Kullanıcı girişi
- `POST /api/logout` - Çıkış yapma
- `POST /api/attendance` - Yoklama alma
- `GET /api/attendance` - Yoklama listesi (Admin)
- `GET /api/user` - Kullanıcı bilgileri

---

## 🛡️ Güvenlik

- Şifreler SHA256 ile hash'lenir
- Session tabanlı kimlik doğrulama
- Rol tabanlı yetkilendirme
- Tüm zaman işlemleri Türkiye saatiyle yapılır

---

## 📢 Katkı ve Lisans

Açık kaynak katkılarına açıktır.  
Lisans: MIT
