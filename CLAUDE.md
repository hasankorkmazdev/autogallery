## Proje

Tek galeriye ait premium oto galeri sitesi: Razor vitrin + araçların Sahibinden ayrıntısında girildiği yönetim paneli. Altyapı GoraDigitalSignage'dan alındı; **tenant yok, çoklu dil yok (yalnız Türkçe)**. Ürün kararları aşağıda "Kararlar" bölümündedir.

| Proje | Görevi |
|---|---|
| `FatihTasci.Core` | Entity, enum, DTO + validator, servisler, repository/UnitOfWork, middleware |
| `FatihTasci.Data` | `AppDbContext`, migration'lar, `DbSeeder` |
| `FatihTasci.Server` | Admin JSON API (`/api/*`), vitrin Razor sayfaları, medya (`/media`), panelin derlenmiş hali (`/panel`) |
| `FatihTasci.Admin` | Yönetim paneli: React 19 + TS + Vite + shadcn/ui |

## Komutlar

- Server: `dotnet run --project FatihTasci.Server` → http://localhost:5080 (vitrin), http://localhost:5080/panel (panelin derlenmiş hali)
- Panel geliştirme: `cd FatihTasci.Admin && npm run dev` → http://localhost:5173/panel/ (`/api`, `/media` ve `/images` Vite proxy'si ile 5080'e gider, CORS yok)
- **Portlar sabittir:** Server 5080 (`launchSettings.json`), panel 5173 (`vite.config.ts`, `strictPort`); başka porta kaymazlar (`.claude/launch.json`'da `autoPort: false`).
- **Başlatırken eski süreç kapatılır:** `scripts/start-server.ps1` 5080'i dinleyen ve `bin\Debug`'ı kilitleyen eski sunucuyu kapatıp `dotnet run` yapar; panelde `npm run dev` öncesi `predev` 5173'ü boşaltır (`scripts/free-port.ps1`). Sunucuyu terminalden `powershell -File scripts/start-server.ps1` ile başlatın.
- **Visual Studio:** `FatihTasci.Server` başlangıç projesi, profiller `launchSettings.json`'da: **Vitrin** (5080'i açar) ve **Panel** (Vite'ı da başlatır, `http://localhost:5173/panel/`'i açar; `DevTools:StartViteDevServer`, `Helpers/ViteDevServer.cs`). Debug derlemeden önce `StopPreviousServer` MSBuild adımı eski sunucuyu kapatır (`FatihTasci.Server.csproj`). `server-verify` (5081, `-c DebugVerify`) yalnız 5080'deki sunucu açıkken doğrulama içindir; `bin/Debug` kilitliyken ayrı klasöre derler.
- Panel derleme: `npm run build` (çıktı `FatihTasci.Server/wwwroot/panel`), `npm run lint`
- Migration: `dotnet ef migrations add <Ad> --project FatihTasci.Data --startup-project FatihTasci.Server -o Migrations`

## Ortam ve ayarlar

- .NET 10 (`net10.0`), SQL Server (geliştirmede LocalDB). Açılışta `Database.Migrate()` ve `DbSeeder.SeedAsync` çalışır; seed her bölümü yalnız tablo boşsa yazar. Katalog seed'i (`Data/Context/Seed/CatalogSeedData.cs`, SpareParts'tan) var olan kayıtlarla birleşir ve `SiteSettings`'teki `__CatalogSeedVersion` ile bir kez çalışır; katalog verisi değişince `DbSeeder.CatalogSeedVersion` artırılır.
- Marka logoları `Server/wwwroot/images/brands/{slug}.png` (256×256 PNG, koyu renkli olduğu için açık zeminde gösterilir).
- `JwtSettings:Secret` en az 32 karakter olmalı, yoksa uygulama açılmaz. Yönetici kullanıcı `SeedAdmin` ayarından oluşturulur; şifre koda yazılmaz.
- `Storage:Root` (varsayılan `Storage`): `public/` `/media` altında sunulur (WebP varyantları, ekspertiz PDF), `private/` sunulmaz (orijinal fotoğraflar).
- Şifreler `PasswordHasher<UserEntity>` (PBKDF2) ile saklanır. Signage'daki MD5 kullanılmaz.

## Kararlar

- **Kapsam dışı:** üyelik / bireysel ilan, online ödeme / kapora. İlan portallarına (Sahibinden, arabam.com) aktarım Faz 3'te araştırılacak; Sahibinden'den veri çekilmez.
- **Roller:** Panele birden fazla kişi girer, kayıt ekranı yok (kullanıcıları Yönetici ekler; silinmez, pasife alınır). **Personel:** araçlar (ekleme, düzenleme, fotoğraf, yayın, durum), talepler, sihirbazda listede olmayan marka / seri / model ekleme. **Yalnız Yönetici:** katalog sayfası (ad değiştirme, pasife alma), donanım listesi, site ayarları, showroom, kullanıcılar. En az bir aktif Yönetici kalır; kişi kendi rolünü / aktifliğini değiştiremez. Token her istekte `IAccountService.IsSessionValidAsync` ile kontrol edilir (1 dk önbellek), pasife alınan ya da rolü değişen kullanıcının oturumu kapanır.
- **Firma adı ve logo** kodda sabit yazılmaz: `SiteSettings` (`GalleryName`, `LogoPath`). Vitrin başlığı/alt bilgisi, panel başlığı ve filigran buradan okur. Logo `POST /api/settings/logo` ile yüklenir, `PUT /api/settings` logoyu değiştirmez.
- **Fotoğraflar:** araç başına sınırsız (yalnız dosya 25 MB ve istek boyutu sınırı). Orijinal `private/`'ta EXIF'li ve filigransız saklanır; 480 / 1280 / 2400 WebP varyantları üretilir. İşleme yükleme isteği içinde yapılır (yük artarsa Hangfire'a taşınır).
- **Filigran:** `WatermarkEnabled` açık ve logo varsa logo 1280 ve 2400 varyantlarının sağ altına yarı saydam basılır (genişliğin %16'sı); 480'e basılmaz. Logo / ayar değişince Ayarlar → "Filigranı yeniden uygula" tüm varyantları orijinallerden arka planda yeniden üretir (`MediaRegenerationRunner`). `/media` bir yıl `immutable` önbelleğe alındığı için yeniden üretilen dosya hep **yeni adla** yazılır, eski silinir.
- **Fiyat:** araç bazında `ShowPrice` (kapalıysa "Fiyat için arayınız", fiyat filtresinde çıkmaz; fiyat yine de panelde zorunlu). `CampaignPrice` doluysa eski fiyat üstü çizilir, sıralama ve filtre kampanyalı fiyatı kullanır.
- **Slug:** marka / seri / model adından, araç yıl + marka + model'den (`/arac/{slug}-{ilanNo}`); araç ilan no ile bulunur, slug güncel değilse 301. Türkçe harfler dönüştürülür, benzersizlik çakışmada `-2`, `-3`.
- **Tasarım dili (vitrin):** lüks showroom; koyu zemin, büyük fotoğraf, bol boşluk, ince çizgi, yavaş hareket. Renk token'ları `site.css` `:root`'ta (zemin `#0B0B0C`, vurgu şampanya altın `#C8A96A`); hasar şeması renkleri (gri / sarı / turuncu / kırmızı) başka yerde kullanılmaz. Başlık Cormorant Garamond, metin Manrope, kendi sunucumuzdan. "Hareketi azalt" açıksa animasyon yok. Panel Signage'ın shadcn tasarımını kullanır.
- **Signage'dan sapmalar:** Repository / UnitOfWork tenant'sız sade sürüm; AutoMapper yok (ticari lisans, eşleme elle); CORS yerine Vite proxy'si; ImageSharp 3.2 (3.1 güvenlik açığı).
- **Yol haritası:** Faz 1 tamam. **Faz 2:** premium vitrin tasarımı, yazı tipleri, ana sayfa ve animasyonlar (GSAP + Lenis), tam ekran galeri, satılanlar arşivi (`/satilanlar`), talep e-posta bildirimleri, schema.org `Car`+`Offer`, sitemap, 1200×630 paylaşım görseli. **Faz 3:** "Aracınızı alalım" formu (fotoğraflı), kredi hesaplayıcı, istatistik ekranı (görüntülenme, talep, WhatsApp / arama tıklaması), Instagram görseli, portal aktarımı araştırması.
- **Açık sorular:** galerinin gerçek adı, logosu ve kurumsal renkleri; ortalama stok sayısı (depolama planı); sunucu (IIS?) ve alan adı.

## Kurallar

Signage'ın CLAUDE.md kuralları geçerlidir, şu farklarla:

- **Tenant yok:** `ITenantProvider`, `X-Tenant-Id`, `ignoreTenant` parametreleri yoktur. Denetim alanları `ICurrentUserProvider` ile doldurulur.
- **Dil:** i18next yoktur, Signage kural 8 (no magic strings) geçersizdir. **Metinler doğrudan Türkçe yazılır.** Enum'ların ekranda görünen adları `[Display(Name)]` ile verilir; panel bunları `GET /api/lookups`'tan alır.

### Backend

1. `record` yok, her model `class`.
2. FluentValidation validator'ı aynı dosyada, request modelinin hemen altında (`<Model>Validator`).
3. **Dosya başına tek tip:** her controller, model (request/response DTO) ve entity kendi dosyasında, dosya adı tip adıyla aynı (`BrandsController.cs`, `SaveBrandRequestModel.cs`). Tek istisna validator'dır (kural 2). İlgili olanlar aynı klasörde durur (`Controllers/Api`, `Models/Domain/Catalog`, `Entity/Inventory`); birden çok tip tek dosyada toplanmaz.
4. `Result.Message` metinleri Türkçe ve kullanıcıya gösterilebilir (panel toast'ları bunları gösterir).
5. Yanıtlar `ServiceResultModel<T>` zarfıyla döner; controller her zaman `Ok(result)` döndürür, HTTP durum kodunu `ServiceResultResponseProccesor` `Code`'dan ayarlar. Doğrulama hataları da aynı zarfla gelir (`Data` = alan → mesajlar).
6. `DomainService.Result(...)` ile yalnız mesaj dönerken isimli argüman kullanın: `Result(message: "...")`. Aksi halde generic `Result<T>(T data)` seçilir.
7. Tam nitelikli tip adı yerine `using`.
8. `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json` anahtarları senkron tutulur.
9. Model değişince migration eklenir ve commit'e dahil edilir.
10. Slug hiçbir yerde elle girilmez: `SlugHelper`. Marka / seri / model adı değişince eski slug `SlugRedirect`'e yazılır (301).
11. Araç sihirbazı kuralları iki tarafta aynıdır: `Core/Models/Domain/Vehicle/Vehicle*RequestModel.cs` ↔ `Admin/src/pages/vehicles/wizard/schemas.ts`. Biri değişirse diğeri de güncellenir.
12. **Roller (bkz. Kararlar):** Yalnız Yönetici'nin yapabileceği uçlara `[Authorize(Roles = RoleNames.Admin)]` eklenir; panelde ilgili menü `adminOnly`, sayfa `AdminRoute` altında olur. Yetki her zaman sunucuda kontrol edilir.
13. Hasar şeması parça yerleşimi iki yerde aynıdır: `Server/Views/Shared/_DamageDiagram.cshtml` ↔ `Admin/src/components/damage-diagram/parts.ts`.

### Panel (Signage kural 9-18 aynen)

- shadcn primitive'leri (ham `<button>`/`<input>` yok), `components/ui` bileşenlerine görünüm class'ı yok (ESLint `gora/no-design-classes`).
- Seçim kutuları aranabilir olur: `components/ui/combobox.tsx` (`Combobox`, formda `SelectField`). `ui/select` doğrudan kullanılmaz. Arama Türkçe harf ve aksan farkını yok sayar.
- Her alan doğrulanır (react-hook-form + zod) ve altında açıklama/hata gösterilir: `InputField`, `components/form/form-fields.tsx` (`SelectField`, `NumberField`, `TextareaField`, `SwitchField`).
- `pages/<feature>/index.tsx` + `components/`, iç içe ternary yok, `autoComplete="off"`.
- Effect içinde senkron `setState` yok (`react-hooks/set-state-in-effect`); veri yükleme için `hooks/use-query.ts`.
- `eslint-suppressions.json` yalnız Signage'dan gelen `components/ui` ihlallerini içerir; yeni ihlal eklenmez.

### Vitrin (Razor)

- Faz 1 sade tasarım: `wwwroot/css/site.css` (renk token'ları `:root`'ta), `wwwroot/js/site.js` (çerçevesiz küçük etkileşimler).
- Dış servislerden yazı tipi / script yüklenmez (KVKK). Formlarda antiforgery, rate limit (`forms`), KVKK onayı ve bot tuzağı (`Website` alanı) vardır.
