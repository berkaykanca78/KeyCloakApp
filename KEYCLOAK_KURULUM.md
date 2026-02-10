# KeyCloak ile Ortak JWT Giriş Rehberi

Bu projede **AuthApi** (giriş API’si), **FirstApp** ve **SecondApp** birlikte kullanılıyor. Kullanıcı **AuthApi** üzerinden giriş yapar, Keycloak’tan alınan **access_token** döner; bu token **FirstApp** ve **SecondApp** isteklerinde **Authorization** header’ında taşınır. Tek token ile her iki API’ye de erişilir.

---

## 1. Gereksinimler

- **Docker Desktop** (Windows’ta Docker Compose için)
- **.NET 10 SDK**
- İsteğe bağlı: **Postman** veya **curl** (token alıp API’leri test etmek için)

---

## 2. KeyCloak’ı Docker ile Çalıştırma

Proje kök dizininde (`docker-compose.yml` dosyasının olduğu yerde) PowerShell’de:

```powershell
docker compose up -d
```

KeyCloak ilk açılışta 30–60 saniye sürebilir. Hazır olduğunda:

- **Admin Console:** http://localhost:8080
- **Kullanıcı adı:** `admin`
- **Şifre:** `admin`

Tarayıcıda açıp giriş yapın.

---

## 3. KeyCloak’ta Realm ve Client Ayarları

### 3.1 Yeni Realm Oluşturma

1. Sol üstte **Keycloak** yazan yere tıklayın → **Create realm**.
2. **Realm name:** `KeyCloakApp` (projedeki `Authority` ile aynı olmalı).
3. **Create** ile kaydedin.

### 3.2 Client (backend-api) Oluşturma

1. Sol menüden **Clients** → **Create client**.
2. **General settings**
   - **Client type:** OpenID Connect  
   - **Client ID:** `backend-api`  
   - **Next**.
3. **Capability config**
   - **Client authentication:** ON (confidential client).
   - **Authorization:** Kapalı.
   - **Authentication flow:**  
     - **Standard flow** (Authorization Code) ve **Direct access grants** (Resource Owner Password) işaretli olsun (test için).
   - **Next** → **Save**.
4. **Credentials** sekmesine gidin.
   - **Client authenticator:** Client Id and Secret.
   - **Secret** değerini kopyalayın (token alırken `client_secret` olarak kullanacaksınız).

### 3.3 Client’e Audience (Mapper) Eklemek

**Neden gerekli?**  
JWT token içinde **aud** (audience) adında bir alan vardır. FirstApp ve SecondApp, `appsettings.json` içinde `"Audience": "backend-api"` ile çalışıyor; yani gelen token’da **aud = backend-api** olmalı ki API isteği kabul etsin. KeyCloak varsayılan olarak access token’a her zaman bu değeri koymaz. Bu yüzden “token’a audience olarak backend-api ekle” kuralını (mapper) biz tanımlıyoruz.

**Ne yapıyoruz?**  
KeyCloak’a diyoruz: “Bu client için ürettiğin token’ların içine **aud** alanında **backend-api** yaz.” Böylece API’lerimiz token’ı doğrulayabiliyor.

**Adımlar:**

1. Sol menüden **Clients** → listeden **backend-api** client’ına tıklayın.
2. Üstteki sekmelerden **Client scopes** sekmesine geçin.
3. **Dedicated scope** bölümünde tek bir scope görürsünüz (adı genelde **backend-api-dedicated** veya sadece **Dedicated**). Bu satıra tıklayın (scope’un adına tıklayın, açılan sayfada mapper’lar listelenir).
4. **Add mapper** butonuna tıklayın → **By configuration** → listeden **Audience** seçin.
5. Açılan formda:
   - **Name:** `audience` (veya istediğiniz bir isim)
   - **Included Client Audience:** `backend-api` yazın (açılır listeden seçebilirsiniz)
   - **Add to ID token:** Açık (ON)
   - **Add to access token:** Açık (ON) — **önemli:** API’ler access token kullandığı için bu mutlaka ON olmalı
6. **Save** ile kaydedin.

Bu işlemden sonra bu client ile alacağınız token’ların içinde `"aud": "backend-api"` olur ve her iki API de token’ı kabul eder.

### 3.4 Realm Rolleri (Admin, User) ve JWT’de Rol Claim’i

API’lerde **Admin** ve **User** rolleri kullanılıyor. Admin tüm yetkili endpoint’lere, User sadece kendine ayrılan endpoint’lere erişir. Rollerin JWT access token içinde **role** claim’i olarak gelmesi gerekir.

#### 3.4.1 Realm rolleri oluşturma

1. Sol menü **Realm roles** → **Create role**.
2. **Role name:** `Admin` → **Save**.
3. Tekrar **Create role** → **Role name:** `User` → **Save**.

#### 3.4.2 backend-api client’ında rol claim mapper’ı

Token’da realm rolleri varsayılan olarak sadece `realm_access.roles` içinde gelir. ASP.NET Core’un `[Authorize(Roles = "Admin")]` kullanabilmesi için rollerin **role** adlı claim’de gelmesi gerekir.

1. **Clients** → **backend-api** → **Client scopes** sekmesi.
2. **Dedicated** (veya backend-api-dedicated) scope satırına tıklayın (scope adına tıklayın).
3. **Add mapper** → **By configuration** → **User Realm Role** seçin.
4. Ayarlar:
   - **Name:** `realm-roles`
   - **Token Claim Name:** `role`
   - **Multivalued:** ON (her rol ayrı claim olur)
   - **Add to ID token:** İsterseniz ON
   - **Add to access token:** **ON** (zorunlu)
5. **Save**.

Bundan sonra giriş yapan kullanıcının realm rolleri access token’da `role` claim’i olarak yer alır (örn. `"role": ["Admin"]`).

### 3.5 Admin ve User kullanıcıları oluşturma

#### Admin kullanıcısı

1. Sol menü **Users** → **Create user**.
2. **Username:** `admin` (veya istediğiniz).
3. **Email** / **First name** / **Last name** isteğe bağlı.
4. **Create**.
5. **Credentials** sekmesi → **Set password** (örn. `admin`) → **Save** (Temporary: OFF).
6. **Role mapping** sekmesi → **Assign role** → **Filter by realm roles** → **Admin**’i seçin → **Assign**.

#### User kullanıcısı

1. **Users** → **Create user**.
2. **Username:** `user`.
3. **Create**.
4. **Credentials** → **Set password** (örn. `user`) → **Save** (Temporary: OFF).
5. **Role mapping** → **Assign role** → **User** rolünü seçin → **Assign**.

**Özet:**

| Kullanıcı | Şifre (örnek) | Rol  | Erişebildiği endpoint’ler                          |
|-----------|----------------|------|---------------------------------------------------|
| admin     | admin          | Admin | Tüm yetkili (GET /WeatherForecast, GET /WeatherForecast/user) |
| user      | user           | User  | Sadece GET /WeatherForecast/user                  |

İsterseniz mevcut **testuser** kullanıcısına da **User** rolü atayabilirsiniz.

### 3.6 Eski test kullanıcısı (isteğe bağlı)

1. Sol menü **Users** → **Create user**.
2. **Username:** `testuser` (veya istediğiniz).
3. **Email** ve **First/Last name** isteğe bağlı.
4. **Create**.
5. **Credentials** sekmesi → **Set password** (örn. `testuser`) → **Save** (Temporary: OFF yapabilirsiniz).

---

## 4. Token Alma (JWT)

### Yöntem A: Direct Access (kullanıcı adı/şifre) – Test için

PowerShell’de (tek satırda veya satır sonunda `\` ile bölerek):

/// BURAYA_CLIENT_SECRET_YAPIŞTIRIN -> 8UnOB25jLwfU4fQpIgQjgt0xIVcXBMqo

```powershell
$body = @{
    client_id     = "backend-api"
    client_secret = "8UnOB25jLwfU4fQpIgQjgt0xIVcXBMqo"
    username      = "testuser"
    password      = "testuser"
    grant_type    = "password"
}
Invoke-RestMethod -Uri "http://localhost:8080/realms/KeyCloakApp/protocol/openid-connect/token" -Method Post -Body $body -ContentType "application/x-www-form-urlencoded"
```

Çıktıda `access_token` alanındaki uzun string **JWT**’nizdir. Bunu API isteklerinde kullanacaksınız.

### Yöntem B: curl (Windows’ta Git Bash veya WSL)

```bash
curl -X POST "http://localhost:8080/realms/KeyCloakApp/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=backend-api" \
  -d "client_secret=BURAYA_CLIENT_SECRET" \
  -d "username=testuser" \
  -d "password=testuser" \
  -d "grant_type=password"
```

Yanıttaki `access_token` değerini kopyalayın.

### Yöntem C: AuthApi ile login (önerilen)

**AuthApi**, Keycloak’a sizin yerinize istek atıp **access_token** döndüren ayrı bir API’dir. Tüm girişler bu API üzerinden yapılır; dönen token FirstApp ve SecondApp’te **Authorization: Bearer &lt;token&gt;** ile kullanılır.

1. **AuthApi**’yi çalıştırın (bkz. 5.1).
2. **POST** isteği atın:
   - **URL:** `http://localhost:5200/api/auth/login` (veya `https://localhost:7225/api/auth/login`)
   - **Body (JSON):** `{ "username": "testuser", "password": "testuser" }`
   - **Content-Type:** `application/json`
3. Yanıtta `access_token` ve isteğe bağlı `refresh_token` gelir. Bu **access_token**’ı FirstApp ve SecondApp isteklerinde **Authorization** header’ında kullanın.

Refresh token ile yeni access token almak için: **POST** `http://localhost:5200/api/auth/refresh` — Body: `{ "refreshToken": "BURAYA_REFRESH_TOKEN" }`.

AuthApi ayarları (`AuthApi/appsettings.json`): **Keycloak:Authority**, **Keycloak:ClientId**, **Keycloak:ClientSecret**. Client secret Keycloak’taki **backend-api** client’ının secret’i ile aynı olmalı.

---

## 5. API’leri Çalıştırma ve Token ile Çağırma

### 5.1 AuthApi, FirstApp ve SecondApp’i Çalıştırma

- Visual Studio’dan projeleri ayrı ayrı çalıştırabilirsiniz (F5 veya “Run”).
- Veya terminalde:
  - **AuthApi:** `AuthApi` klasöründe `dotnet run` → `http://localhost:5200` / `https://localhost:7225`
  - **FirstApp:** `FirstApp` klasöründe `dotnet run` → `http://localhost:5198` / `https://localhost:7223`
  - **SecondApp:** `SecondApp` klasöründe `dotnet run` → `http://localhost:5131` / `https://localhost:7067`

**Akış:** Önce **AuthApi**’de login olun → dönen **access_token**’ı alın → FirstApp / SecondApp isteklerinde **Authorization: Bearer &lt;access_token&gt;** header’ı ile gönderin.

### 5.2 Token Gerektirmeyen Endpoint (test)

- FirstApp: `GET https://localhost:PORT/WeatherForecast/public`
- SecondApp: `GET https://localhost:PORT/WeatherForecast/public`

Tarayıcı veya Postman ile doğrudan açılır; token gerekmez.

### 5.3 Token ve Rol Gerektiren Endpoint’ler

| Endpoint | İzin verilen roller | Açıklama |
|----------|----------------------|----------|
| `GET …/WeatherForecast` | **Admin** | Sadece Admin rolü erişir. |
| `GET …/WeatherForecast/user` | **Admin, User** | Admin ve User rolleri erişir (User için ayrı metod). |

- FirstApp: `GET https://localhost:PORT/WeatherForecast` (Admin), `GET https://localhost:PORT/WeatherForecast/user` (Admin veya User)
- SecondApp: aynı yapı.

Bu isteklerde **Authorization** header’ı gerekir. Token’ı **AuthApi**’nin `/api/auth/login` endpoint’inden alın (örn. `admin`/`admin` veya `user`/`user`); aynı token’ı burada kullanın.

- **Header adı:** `Authorization`
- **Değer:** `Bearer BURAYA_ACCESS_TOKEN_YAPIŞTIRIN` (access_token, AuthApi login yanıtındaki `access_token` alanı)

Postman’de: **Authorization** sekmesi → Type: **Bearer Token** → Token alanına **sadece** access_token’ı yapıştırın.

PowerShell örneği (PORT’u kendi değerinizle değiştirin):

```powershell
$token = "BURAYA_ACCESS_TOKEN"
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "https://localhost:7XXX/WeatherForecast" -Headers $headers
```

Aynı token ile hem FirstApp hem SecondApp’e istek atabilirsiniz; ortak giriş bu şekilde çalışır.

---

## 6. Kısa Özet

| Adım | Ne yaptık? |
|------|------------|
| 1 | `docker compose up -d` ile KeyCloak’ı ayağa kaldırdık. |
| 2 | KeyCloak’ta `KeyCloakApp` realm’i ve `backend-api` client’ı oluşturduk. |
| 3 | Client’a audience mapper ekleyip `audience = backend-api` yaptık. |
| 4 | **AuthApi** ile login olup token aldık (veya doğrudan Keycloak token endpoint’i kullandık). |
| 5 | Dönen JWT’yi `Authorization: Bearer <token>` ile FirstApp ve SecondApp’e gönderdik. |

**AuthApi** tüm girişleri toplar; Keycloak’tan alınan access_token FirstApp ve SecondApp’te aynı şekilde kullanılır. Her iki API de aynı **Authority** ve **Audience** ile JWT doğruladığı için tek token yeterli.

---

## 7. Sorun Giderme

- **401 Unauthorized:** Token süresi dolmuş olabilir; yeni token alın. Audience’ın `backend-api` ve Issuer’ın `http://localhost:8080/realms/KeyCloakApp` olduğundan emin olun.
- **KeyCloak açılmıyor:** `docker compose ps` ile container’ın “Up” olduğunu kontrol edin; gerekirse `docker compose logs keycloak` ile loglara bakın.
- **API “Authority” bulamıyor:** KeyCloak’ın 8080’de çalıştığından ve realm adının `KeyCloakApp` olduğundan emin olun. API’leri Docker dışında çalıştırıyorsanız Authority olarak `http://localhost:8080/realms/KeyCloakApp` kullanın (zaten `appsettings.json`’da bu var).

İsterseniz bir sonraki adımda Postman koleksiyonu veya basit bir test script’i de ekleyebiliriz.
