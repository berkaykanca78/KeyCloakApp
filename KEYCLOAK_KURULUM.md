# KeyCloak ile Ortak JWT Giriş Rehberi

Bu projede **FirstApp** ve **SecondApp** aynı KeyCloak realm'inden alınan JWT token ile korunuyor. Tek token ile her iki API'ye de erişebilirsiniz.

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
2. **Realm name:** `KeyCloackApp` (projedeki `Authority` ile aynı olmalı).
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

### 3.4 Test Kullanıcısı Oluşturma

1. Sol menü **Users** → **Create user**.
2. **Username:** `testuser` (veya istediğiniz).
3. **Email** ve **First/Last name** isteğe bağlı.
4. **Create**.
5. **Credentials** sekmesi → **Set password** (örn. `testuser`) → **Save** (Temporary: OFF yapabilirsiniz).

---

## 4. Token Alma (JWT)

### Yöntem A: Direct Access (kullanıcı adı/şifre) – Test için

PowerShell’de (tek satırda veya satır sonunda `\` ile bölerek):

/// BURAYA_CLIENT_SECRET_YAPIŞTIRIN -> kznXHoEkkbfzCOJe3RzupRCDkSXMD0E1

```powershell
$body = @{
    client_id     = "backend-api"
    client_secret = "kznXHoEkkbfzCOJe3RzupRCDkSXMD0E1"
    username      = "testuser"
    password      = "testuser"
    grant_type    = "password"
}
Invoke-RestMethod -Uri "http://localhost:8080/realms/KeyCloackApp/protocol/openid-connect/token" -Method Post -Body $body -ContentType "application/x-www-form-urlencoded"
```

Çıktıda `access_token` alanındaki uzun string **JWT**’nizdir. Bunu API isteklerinde kullanacaksınız.

### Yöntem B: curl (Windows’ta Git Bash veya WSL)

```bash
curl -X POST "http://localhost:8080/realms/KeyCloackApp/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=backend-api" \
  -d "client_secret=BURAYA_CLIENT_SECRET" \
  -d "username=testuser" \
  -d "password=testuser" \
  -d "grant_type=password"
```

Yanıttaki `access_token` değerini kopyalayın.

---

## 5. API’leri Çalıştırma ve Token ile Çağırma

### 5.1 FirstApp ve SecondApp’i Çalıştırma

- Visual Studio’dan her iki projeyi ayrı ayrı çalıştırabilirsiniz (F5 veya “Run”).
- Veya terminalde:
  - `FirstApp` klasöründe: `dotnet run`
  - `SecondApp` klasöründe: başka bir terminalde `dotnet run`

Varsayılan portlar `launchSettings.json` içinde (genelde 5000/5001 veya 7xxx). Doğru URL’leri oradan alın.

### 5.2 Token Gerektirmeyen Endpoint (test)

- FirstApp: `GET https://localhost:PORT/WeatherForecast/public`
- SecondApp: `GET https://localhost:PORT/WeatherForecast/public`

Tarayıcı veya Postman ile doğrudan açılır; token gerekmez.

### 5.3 Token Gerektiren Endpoint’ler

- FirstApp: `GET https://localhost:PORT/WeatherForecast`
- SecondApp: `GET https://localhost:PORT/WeatherForecast`

Bu isteklerde **Authorization** header’ı gerekir:

- **Header adı:** `Authorization`
- **Değer:** `Bearer BURAYA_ACCESS_TOKEN_YAPIŞTIRIN` 

Postman’de: **Authorization** sekmesi → Type: **Bearer Token** → Token alanına yapıştırın.

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
| 2 | KeyCloak’ta `KeyCloackApp` realm’i ve `backend-api` client’ı oluşturduk. |
| 3 | Client’a audience mapper ekleyip `audience = backend-api` yaptık. |
| 4 | Test kullanıcısı ve şifre ile token aldık. |
| 5 | Bu JWT’yi `Authorization: Bearer <token>` ile FirstApp ve SecondApp’e gönderdik. |

Her iki API de aynı **Authority** ve **Audience** ile JWT doğruladığı için tek token yeterli.

---

## 7. Sorun Giderme

- **401 Unauthorized:** Token süresi dolmuş olabilir; yeni token alın. Audience’ın `backend-api` ve Issuer’ın `http://localhost:8080/realms/KeyCloackApp` olduğundan emin olun.
- **KeyCloak açılmıyor:** `docker compose ps` ile container’ın “Up” olduğunu kontrol edin; gerekirse `docker compose logs keycloak` ile loglara bakın.
- **API “Authority” bulamıyor:** KeyCloak’ın 8080’de çalıştığından ve realm adının `KeyCloackApp` olduğundan emin olun. API’leri Docker dışında çalıştırıyorsanız Authority olarak `http://localhost:8080/realms/KeyCloackApp` kullanın (zaten `appsettings.json`’da bu var).

İsterseniz bir sonraki adımda Postman koleksiyonu veya basit bir test script’i de ekleyebiliriz.
