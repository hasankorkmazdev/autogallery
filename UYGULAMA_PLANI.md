# Uygulama Planı — Faz 1 (Temel)

> 13 Kasım 2026 · Kapsam ve gerekçeler için bkz. [PROJE_DOKUMANI.md](PROJE_DOKUMANI.md)

Bu plan, Faz 1'in hangi adımlarla ve hangi sırayla kodlanacağını anlatır. Faz 1 bittiğinde araçlar panelden eksiksiz girilebilecek ve sitede görünecek.

## Durum

| # | Adım | Durum |
|---|---|---|
| 1 | Ortamı ve Signage altyapısını incele | Tamamlandı |
| 2 | Solution ve projeleri oluştur | Tamamlandı |
| 3 | Core: altyapı, entity'ler, slug | Tamamlandı |
| 4 | Data: DbContext, seed, migration | Tamamlandı |
| 5 | Server: auth, admin API, medya | Tamamlandı |
| 6 | Admin panel: giriş, liste, sihirbaz | Tamamlandı |
| 7 | Vitrin: stok ve detay sayfaları | Tamamlandı |
| 8 | Derle ve çalıştırarak doğrula | Tamamlandı (13 Kasım 2026) |

### Uygulamada plandan sapmalar

- **Repository / UnitOfWork aynen kopyalanmadı.** Signage'daki `Repository.cs` (1353 satır) her metotta `ignoreTenant` parametresi ve EFCore.BulkExtensions bağımlılığı taşıyordu. Aynı metot adları ve isimli parametrelerle tenant'sız, sade bir sürüm yazıldı. `PagedList`, sayfalama extension'ları, `ServiceResultModel`, `DomainService`, middleware'ler ve converter'lar aynen alındı.
- **AutoMapper kullanılmadı.** Sürüm 15'ten itibaren ticari lisans istiyor; eşlemeler elle yazıldı.
- **CORS yerine Vite proxy'si.** Panel geliştirmede `/api` ve `/media`'yı Server'a yönlendirir; derlenmiş panel zaten aynı origin'den sunulur.
- **SixLabors.ImageSharp 3.2.** 3.1.x sürümlerinde yüksek önemli güvenlik açıkları var; 3.2.0 yamalı ve lisans koşulu Signage'daki ile aynı. 4.x'e geçilmedi.
- **Ek olarak yapılanlar:** `/stok/{marka}/{seri}` adresleri ve eski slug ile 301, ana sayfa (sade), `/kvkk` taslak metni, araç sayfasında bilgi / test sürüşü formu, panelde Katalog, Donanımlar ve Ayarlar ekranları.
- **Yazı tipleri** (Cormorant Garamond / Manrope) Faz 2'ye kaldı; vitrin şimdilik sistem yazı tiplerini kullanıyor.

### Doğrulama sonucu

- `dotnet build` hatasız; panelde `npm run build` ve `npm run lint` hatasız.
- LocalDB'de veritabanı oluştu, seed çalıştı (yönetici, 16 renk, 80 donanım, 13 marka).
- API uçtan uca betikle denendi: giriş, taslak, 7 adım, 9 fotoğraf yükleme (her biri için 3 WebP varyantı), sıralama, kapak, yayın kontrolü, yayınlama, kopyalama. Hatalı girişler (uyumsuz katalog, elektrikte motor hacmi, sıfır araçta km, geçmiş garanti tarihi, bozuk dosya, fotoğrafsız yayın) reddedildi.
- Tarayıcıda: panel girişi, araç listesi, yeni araç (zincirleme seçim ve başlık önerisi), adım doğrulaması, hasar şeması, yayın kontrol listesi; vitrinde `/stok`, araç detayı, eski slug ile 301, iletişim formu ve talebin panelde görünmesi.
- Panelden tarayıcıyla fotoğraf yükleme (dosya seçme penceresi) elle denenmedi; aynı uç nokta API betiğiyle doğrulandı.

---

## Ortam (incelendi)

| Araç | Sürüm |
|---|---|
| .NET SDK | 10.0.401 |
| .NET runtime | 8.0.31 ve 10.0.12 |
| dotnet-ef | 10.0.5 |
| Node / npm | 24.14.0 / 11.9.0 |
| SQL Server | LocalDB (`MSSQLLocalDB`) |
| Git | 2.53 (proje klasörü henüz repo değil) |

## Teknik kararlar

- **Hedef çatı: .NET 10 (`net10.0`).** Signage .NET 8 kullanıyor ama .NET 8'in desteği 10 Kasım 2026'da sona erdi. .NET 10 güncel LTS sürümü.
- **Şifreler: `PasswordHasher<T>` (PBKDF2).** Signage şifreleri MD5 ile saklıyor, bu güvenli değil. Bu projede ASP.NET Core'un kendi hasher'ı kullanılacak.
- **Vitrin: Razor.** Server projesinin içinde, sunucuda render. PROJE_DOKUMANI §2.4'teki öneri. Kesin onay bekleniyor; aksi söylenmezse bununla ilerlenir.
- **Çıkarılanlar:** Tenant katmanı, i18n (yalnızca Türkçe), Node / WebSocket / Device auth, kayıt ve Google ile giriş, e-posta doğrulama.
- **Görsel işleme:** İlk sürümde yükleme isteği içinde yapılır. Yük artarsa Hangfire'a taşınır.
- **Veritabanı (geliştirme):** `Server=(localdb)\MSSQLLocalDB;Database=FatihTasci;Trusted_Connection=True;TrustServerCertificate=True`
- **Roller:** Başlangıçta tek rol (Yönetici). Personel rolü PROJE_DOKUMANI §9'daki soruya bağlı.

## Signage'dan alınacak dosyalar

Kaynak kök: `C:\Users\hasan\Desktop\Project\GoraDigitalSignage\GoraDigitalSignage\`. Kopyalanınca namespace `GoraDigitalSignage.*` → `FatihTasci.*` olarak değiştirilir.

| Signage dosyası | Ne yapılacak |
|---|---|
| `Core/RepositoryBase/*` (8 dosya, ~2500 satır) | Aynen kopyalanır: IRepository, Repository, UnitOfWork, PagedList |
| `Core/Extensions/Data/UnitOfWorkServiceCollectionExtensions.cs` | Aynen kopyalanır |
| `Core/Extensions/Data/IQueryablePageListExtensions.cs`, `IEnumerablePagedListExtensions.cs` | Aynen kopyalanır |
| `Core/Models/Domain/ServiceResultModel.cs` | Aynen kopyalanır |
| `Core/Service/Domains/Abstracts/DomainService.cs` | Aynen kopyalanır (`Result(...)` yardımcıları) |
| `Core/Middleware/GlobalExceptionHandler.cs`, `ServiceResultResponseProccesor.cs` | Aynen kopyalanır |
| `Core/Entity/Base/*` | Tenant'lı türevler (`BaseEntityWithTenant`, `ITenantEntity`, `BaseAuditableAndTenantEntity`) çıkarılarak kopyalanır |
| `Core/Entity/Base/Identity/BaseUserEntity.cs`, `Entity/Identity/UserEntity.cs` | `CompanyHeadUser` ve e-posta doğrulama alanları çıkarılır, `Role` eklenir |
| `Core/Converters/*` | `FlexibleEnumConverterFactory`, `BoolConverter` |
| `Core/Logging/SerilogExtensions.cs` | `AddGoraSerilog` → `AddAppSerilog` |
| `Core/Helpers/ImageHelper.cs` | ImageSharp yardımcıları, WebP varyant üretimi için temel |
| `Server/Program.cs` | Yapısı alınır: `ConfigureServices` / `ConfigureMiddleware` / `MigrateDatabase` + JWT. Tenant, WebSocket, transcode, harita ve localization kısımları çıkarılır |
| `Server/Controllers/Api/BaseApiController.cs` | Aynen kopyalanır |
| `Server/Controllers/Api/AuthController.cs` | Yalnızca `Login` kalır |
| `AccountService.CreateLoginResult` / `GenerateJwtToken` | Tenant listesi çıkarılır, MD5 yerine `PasswordHasher` |
| `Web/` (package.json, vite.config.ts, tailwind.config, components.json, `src/components/ui`, `src/api/axios.ts`, auth-store, layout) | Panel iskeleti için. i18next, tenant-gate, `X-Tenant-Id` interceptor'ı ve Signage'a özel sayfalar çıkarılır |

---

## Adım 2 — Solution ve projeler

1. `git init`, Signage'dan `.gitignore` ve `.editorconfig`.
2. `dotnet new sln -n FatihTasci`
3. Projeler (`net10.0`):
    - `FatihTasci.Core` (classlib)
    - `FatihTasci.Data` (classlib, Core'a referans)
    - `FatihTasci.Server` (web, Core ve Data'ya referans)
4. NuGet paketleri: EF Core SqlServer + Design, FluentValidation (+ AspNetCore, DI), AutoMapper, SixLabors.ImageSharp, Serilog.AspNetCore + File sink, JwtBearer, Microsoft.Extensions.Identity.Core (PasswordHasher).
5. `FatihTasci.Admin`: Vite + React 19 + TypeScript. Tailwind 3.4 (v4 değil), shadcn/ui, react-hook-form + zod, Zustand, axios, react-router-dom v7, TanStack Table, dnd-kit, sonner.
6. Proje `CLAUDE.md` dosyası: Signage kuralları uyarlanır. Tenant ve i18n kuralları (8. kural) çıkarılır, "metinler doğrudan Türkçe yazılır" eklenir.

**Bitti sayılır:** `dotnet build` ve `npm run build` hatasız geçer.

## Adım 3 — Core

1. Signage'dan altyapı dosyaları (yukarıdaki tablo).
2. **Enum'lar:** VehicleStatus, FuelType, TransmissionType, BodyType, Drivetrain, VehicleCondition, PlateOrigin, BodyPart (13), BodyPartState, FeatureCategory, LeadType, LeadStatus, UserRole.
3. **Entity'ler** (PROJE_DOKUMANI §3): Brand, Series, CarModel, Vehicle, VehicleBodyPart, Feature, VehicleFeature, VehicleMedia, Lead, LeadMedia, SiteSetting, SlugRedirect, Color (lookup).
4. **SlugHelper** (PROJE_DOKUMANI §3.8):
    - `Generate(string)`: Türkçe karakter dönüşümü (ç→c, ğ→g, ı→i, İ→i, ö→o, ş→s, ü→u), küçük harf, harf/rakam dışı → tire, ardışık tire → tek, baş/son tire silinir.
    - `EnsureUnique(...)`: Çakışmada `-2`, `-3` ekler.
    - Ad değişince eski slug `SlugRedirect`'e yazılır.
5. **DTO + validator'lar:** Her request modelinin validator'ı aynı dosyada, hemen altında. `record` yok.
6. **Servisler:** AccountService, CatalogService, FeatureService, VehicleService, MediaService, LeadService, SiteSettingService, PublicVehicleService (vitrin sorguları).

## Adım 4 — Data

1. `AppDbContext`. Şemalar: `Identity`, `Catalog`, `Inventory`, `Crm`, `Common`.
2. Yapılandırmalar:
    - `Vehicle.ListingNo` benzersiz, 100000'den başlayan sequence.
    - Benzersiz index'ler: `Brand.Slug`; `(BrandId, Slug)` seri için; `(SeriesId, Slug)` model için; `(VehicleId, Part)` hasar parçası için.
    - Fiyat ve Tramer için `decimal(18,2)`.
    - Soft delete için global query filter (`IsDelete`).
3. Seed (yalnızca yeni veritabanında, Signage'daki gibi):
    - Yönetici kullanıcı. E-posta ve şifre `appsettings.Development.json`'dan okunur, koda yazılmaz.
    - Donanım listesi (PROJE_DOKUMANI §3.4).
    - Renkler.
    - Başlangıç marka / seri / model listesi (kapsamı §9'daki soruya bağlı; ilk etapta yaygın premium markalar).
4. İlk migration:

    ```bash
    dotnet ef migrations add Initial --project FatihTasci.Data --startup-project FatihTasci.Server -o Migrations
    ```

5. Uygulama açılışında `Database.Migrate()`, yeni veritabanıysa ardından `Seed()`.

## Adım 5 — Server

1. `Program.cs`: Serilog, DbContext + UnitOfWork, JWT, FluentValidation auto-validation, ServiceResult middleware, GlobalExceptionHandler, CORS (panel dev adresi), rate limit (formlar için), static files.
2. **Admin API** (hepsi `[Authorize]`, yanıtlar `ServiceResultModel<T>`):

    | Uç nokta | Görevi |
    |---|---|
    | `POST /api/auth/login` | Giriş, JWT döner |
    | `GET/POST/PUT /api/brands` | Marka listesi, ekleme, güncelleme |
    | `GET /api/brands/{id}/series`, `POST /api/series` | Seriler |
    | `GET /api/series/{id}/models`, `POST /api/models` | Modeller |
    | `GET /api/features`, `POST/PUT /api/features` | Kategorilere göre gruplu donanım listesi |
    | `GET /api/colors` | Renk listesi |
    | `GET /api/vehicles` | Sayfalı liste; filtre: durum, marka, arama |
    | `GET /api/vehicles/{id}` | Sihirbaz için tam kayıt |
    | `POST /api/vehicles` | Taslak oluşturur, id ve ilan no döner |
    | `PUT /api/vehicles/{id}/identity` | Adım 1 |
    | `PUT /api/vehicles/{id}/technical` | Adım 2 |
    | `PUT /api/vehicles/{id}/condition` | Adım 3 |
    | `PUT /api/vehicles/{id}/body-parts` | Adım 4 |
    | `PUT /api/vehicles/{id}/features` | Adım 5 |
    | `POST /api/vehicles/{id}/media` | Adım 6: çoklu fotoğraf yükleme (multipart) |
    | `PUT /api/vehicles/{id}/media/order`, `PUT .../media/{mediaId}/cover`, `DELETE .../media/{mediaId}` | Sıralama, kapak seçimi, silme |
    | `PUT /api/vehicles/{id}/pricing` | Adım 7: fiyat, vitrin |
    | `GET /api/vehicles/{id}/publish-check` | Yayın kontrol listesi |
    | `PUT /api/vehicles/{id}/status` | Yayınla / Rezerve / Satıldı / Arşiv. Yayınlarken kontrol listesi zorunlu |
    | `POST /api/vehicles/{id}/copy` | Benzer ilandan kopyala |
    | `GET /api/leads`, `PUT /api/leads/{id}` | Talepler, durum ve not |
    | `GET/PUT /api/settings` | Site ayarları |

3. **Medya:** ImageSharp ile WebP varyantları (480 / 1280 / 2400 px), orijinal dosya saklanır. Kök klasör `Storage:Root` ayarından okunur, `/media` altında sunulur.
4. Panelin derlenmiş hali `/panel` altında sunulur. Geliştirmede Vite dev sunucusu ve CORS kullanılır.

**Bitti sayılır:** Giriş yapılıp API ile baştan sona bir araç oluşturulabiliyor.

## Adım 6 — Admin panel

1. Signage Web iskeleti: axios instance (Bearer token, 401'de çıkış), auth-store, giriş sayfası, yan menülü layout.
2. Menü: Araçlar, Talepler, Katalog, Donanımlar, Ayarlar.
3. **Araç listesi:** TanStack Table. Kapak küçük resmi, başlık, yıl, km, fiyat, durum. Tek tıkla Rezerve / Satıldı.
4. **Araç sihirbazı (7 adım):** Her adımda otomatik taslak kaydı. Adımlar arasında serbest geçiş. zod şemaları sunucudaki validator'larla aynı kurallarda.
    - Marka / seri / model zincirleme seçimi ve listede yoksa anında ekleme penceresi.
    - **Hasar şeması bileşeni:** SVG araç silüeti, 13 tıklanabilir parça, tıklandıkça durum sırayla değişir, "Tümü orijinal" kısayolu.
    - **Medya yükleyici:** Sürükle-bırak toplu yükleme, dnd-kit ile sıralama, kapak seçimi, ilerleme göstergesi.
    - Yayın kontrol listesi ve Yayınla butonu.
5. Signage frontend kuralları geçerli (shadcn primitive'leri, her alana doğrulama ve açıklama, küçük bileşenler, iç içe ternary yok, `autoComplete="off"`). Tek fark: metinler `t()` olmadan doğrudan Türkçe.

**Bitti sayılır:** Panelden bir araç 7 adımda girilip yayınlanabiliyor.

Not: API sözleşmesi (Adım 5'teki tablo) sabitlendikten sonra bu adım backend'den bağımsız yürütülebilir. İstenirse ayrı bir oturuma verilebilir.

## Adım 7 — Vitrin (Razor, sade tasarım)

1. `_Layout`: Tasarım token'ları (PROJE_DOKUMANI §6), header, footer, mobil alt çubuk (Ara / WhatsApp).
2. `/stok`: Filtreler query string ile, sunucuda filtreleme ve sayfalama.
3. `/arac/{slug}-{ilanNo}`: Araç ilan no ile bulunur. Adresteki slug güncel değilse 301 ile doğru adrese yönlendirilir. Galeri, özet çubuğu, teknik tablo, donanım listesi, hasar şeması (salt okunur SVG), Tramer, WhatsApp / Ara butonları.
4. `/iletisim`: Form ve talep kaydı (rate limit + KVKK onayı).
5. Satılmış araç sayfası "Satıldı" rozetiyle açık kalır.

Premium görünüm, animasyonlar ve ana sayfa Faz 2'de yapılacak.

## Adım 8 — Doğrulama

1. `dotnet build` ve `npm run build` / `npm run lint` hatasız.
2. Server açılınca LocalDB'de veritabanı oluşuyor, seed çalışıyor.
3. Seed edilen yöneticiyle panele giriş yapılıyor.
4. Bir araç 7 adımda giriliyor: fotoğraflar yükleniyor ve WebP varyantları oluşuyor, yayınlanıyor.
5. Araç `/stok`'ta görünüyor. Detay sayfası açılıyor. Eski slug ile gelinince 301 ile yönlendiriliyor.
6. İletişim formu gönderiliyor ve talep panelde görünüyor.
