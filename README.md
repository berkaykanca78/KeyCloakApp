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

Bu depo, Keycloak ile entegre **AuthApi**, **FirstApi** ve **SecondApi** örnek uygulamalarını içerir. Keycloak kurulumu ve kullanımı için `KEYCLOAK_KURULUM.md` dosyasına bakabilirsiniz.

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
| `admin`       | `admin`     | Admin       | Tüm korumalı API’lere erişir. |
| `user`        | `user`      | User        | Sadece `/WeatherForecast/user` endpoint’ine erişir. |

---

## 🔗 Endpoint’ler ve Erişim

Her iki API’de (FirstApi, SecondApi) aynı yapı kullanılır:

| Endpoint | Kim erişir? | Açıklama |
|----------|--------------|----------|
| `GET /WeatherForecast/public` | Herkes (token gerekmez) | Test için herkese açık. |
| `GET /WeatherForecast` | **Sadece Admin** | Hava tahmini listesi. |
| `GET /WeatherForecast/user` | **Admin veya User** | Giriş yapan kullanıcı bilgisi döner (`user`, `time`). |

Giriş **AuthApi** üzerinden yapılır; dönen **access_token** ile isteklerde `Authorization: Bearer <token>` kullanılır. User bilgisi, token’daki `preferred_username` claim’inden okunur; yanıtta hangi kullanıcıyla giriş yapıldıysa o kullanıcı adı döner.

---

<p align="center">
  <sub><i>Keycloak — Açık kaynak kimlik ve erişim yönetimi</i></sub>
</p>
