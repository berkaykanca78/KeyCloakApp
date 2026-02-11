# KeyCloakApp

<p align="center">
  <img src="https://upload.wikimedia.org/wikipedia/commons/2/29/Keycloak_Logo.png" alt="Keycloak Logo" width="120" />
</p>

---

## 🔐 Keycloak Nedir?

**Keycloak**, uygulamalarınız için **kimlik doğrulama (authentication)** ve **yetkilendirme (authorization)** sağlayan, **açık kaynak** bir **Identity and Access Management (IAM)** çözümüdür. Red Hat tarafından desteklenir ve Java tabanlıdır.

---

## 🎯 Ne İşe Yarar?

| Özellik | Açıklama |
|--------|----------|
| **SSO (Tek Oturum)** | Kullanıcı bir kez giriş yapar; aynı realm’deki tüm uygulamalara otomatik erişir. |
| **Merkezi Kimlik** | Kullanıcıları, rolleri ve grupları tek yerden yönetirsiniz. |
| **Sosyal / Kurumsal Giriş** | Google, GitHub, LDAP, Active Directory vb. ile giriş entegrasyonu. |
| **OAuth 2.0 / OpenID Connect** | Modern protokollerle güvenli API ve web uygulaması koruması. |

---

## 🏗️ Neden Kullanılır?

- **Güvenlik:** Şifreleri ve oturumları merkezi ve güvenli yönetir.
- **Geliştirme Kolaylığı:** Uygulamanız sadece Keycloak ile konuşur; kimlik mantığını kendiniz yazmazsınız.
- **Ölçeklenebilirlik:** Birden fazla uygulama ve mikroservis için tek kimlik katmanı.

---

## 📌 Bu Proje

Bu depo, Keycloak ile entegre **AuthApi**, **OrderApi** ve **InventoryApi** örnek uygulamalarını içerir. Keycloak kurulumu ve kullanımı için `KEYCLOAK_KURULUM.md` dosyasına bakabilirsiniz.

---

## 👤 Roller ve Kullanıcılar

Projede iki **realm rolü** tanımlıdır:

| Rol   | Açıklama |
|-------|----------|
| **Admin** | Tüm yetkili endpoint’lere erişir. |
| **User**  | Sadece kendisi için ayrılan endpoint’lere erişir. |

### Örnek kullanıcılar (Keycloak’ta oluşturulur)

| Kullanıcı adı | Örnek şifre | Atanmış rol | Ne yapabilir? |
|---------------|-------------|-------------|-------------------------------|
| `admin`       | `admin`     | Admin       | Tüm korumalı API’lere erişir (sipariş/stok tam yetki). |
| `user`        | `user`      | User        | Kendi siparişleri, sipariş oluşturma, stok sorgulama. |

---

## 🔗 Endpoint’ler ve Erişim

**OrderApi (Sipariş):** `GET /orders/public` (herkes), `GET /orders` (Admin), `GET /orders/my` ve `POST /orders` (Admin veya User).

**InventoryApi (Stok):** `GET /inventory/public` (herkes), `GET /inventory` ve `PUT /inventory/{id}` (Admin), `GET /inventory/{id}` (Admin veya User).

Giriş **AuthApi** üzerinden yapılır; dönen **access_token** ile isteklerde `Authorization: Bearer <token>` kullanılır. User bilgisi, token’daki `preferred_username` claim’inden okunur; yanıtta hangi kullanıcıyla giriş yapıldıysa o kullanıcı adı döner.

---

## Ocelot API Gateway

Tüm API'ler tek giriş noktasından (**http://localhost:5000**) erişilebilir. Gateway (GatewayApi) istekleri arka plandaki servislere yönlendirir.

| Gateway (tek adres) | Yönlendirme |
|---------------------|-------------|
| `http://localhost:5000/api/auth/*` | → AuthApi (5200) |
| `http://localhost:5000/orders/*`   | → OrderApi (5198) |
| `http://localhost:5000/inventory/*` | → InventoryApi (5131) |

### Gateway Swagger (hepsine tek UI'dan istek)

Gateway'de **tek bir Swagger UI** var; Auth, Order ve Inventory API'leri açılır menüden seçilir, tüm istekler **Gateway (5000)** üzerinden gider, Ocelot ilgili servise yönlendirir.

- **Adres:** http://localhost:5000/swagger  
- Üstteki dropdown'dan **Auth API**, **Order API** veya **Inventory API** seç; "Try it out" ile denediğin istekler otomatik olarak `http://localhost:5000/...` adresine gider.

**Gereksinim:** Auth, Order ve Inventory API'leri çalışır olmalı (5200, 5198, 5131); Gateway başlarken her birinin `/swagger/v1/swagger.json` adresinden dokümanı çeker.

### Ne yapman gerekiyor?

1. **Keycloak + veritabanlarını çalıştır**
   ```bash
   docker-compose up -d
   ```

2. **Migration'ları uygula** (henüz yapmadıysan)
   ```bash
   dotnet ef database update --project InventoryApi --startup-project InventoryApi
   dotnet ef database update --project OrderApi --startup-project OrderApi
   ```

3. **Üç mikroservisi ayağa kaldır** (her biri ayrı terminalde veya IDE ile çoklu startup)
   - **AuthApi:** `dotnet run --project AuthApi` → http://localhost:5200
   - **OrderApi:** `dotnet run --project OrderApi` → http://localhost:5198
   - **InventoryApi:** `dotnet run --project InventoryApi` → http://localhost:5131

4. **Gateway'i çalıştır**
   ```bash
   dotnet run --project GatewayApi
   ```
   Gateway http://localhost:5000 üzerinde dinler.

5. **İstekleri Gateway üzerinden gönder**
   - Giriş: `POST http://localhost:5000/api/auth/login`
   - Siparişler: `GET http://localhost:5000/orders/public`
   - Stok: `GET http://localhost:5000/inventory/public`
   - Token ile: `Authorization: Bearer <access_token>` header'ı aynen kullanılır.

**Not:** Portları değiştirirsen `GatewayApi/ocelot.json` içindeki `DownstreamHostAndPorts` değerlerini (5200, 5198, 5131) güncelle. Geliştirme ortamı için `ocelot.Development.json` ile override da yapabilirsin.

---

## Veritabanları ve Migration

- **InventoryApi** → **PostgreSQL** (veritabanı adı: `Inventory`, localhost:5432).
- **OrderApi** → **MSSQL / T-SQL** (veritabanı adı: `Order`, localhost:1433).

### 1. Veritabanlarını ayağa kaldırma

```bash
docker-compose up -d
```

Keycloak (8080), PostgreSQL (5432) ve MSSQL (1433) container'ları çalışır.

### 2. Migration'ları çalıştırma

Veritabanları çalışırken, proje kökünden:

```bash
# Inventory (PostgreSQL)
dotnet ef database update --project InventoryApi --startup-project InventoryApi

# Order (MSSQL)
dotnet ef database update --project OrderApi --startup-project OrderApi
```

İlk migration'dan sonra Inventory tablosuna örnek 3 stok kaydı (seed) eklenir.

---

<p align="center">
  <sub><i>Keycloak — Açık kaynak kimlik ve erişim yönetimi</i></sub>
</p>
