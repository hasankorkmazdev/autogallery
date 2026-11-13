## Proje

Tek galeriye ait premium oto galeri sitesi. Kapsam ve kararlar: [PROJE_DOKUMANI.md](PROJE_DOKUMANI.md), uygulama adımları: [UYGULAMA_PLANI.md](UYGULAMA_PLANI.md). Altyapı GoraDigitalSignage'dan alındı; **tenant yok, çoklu dil yok (yalnız Türkçe)**.

| Proje | Görevi |
|---|---|
| `FatihTasci.Core` | Entity, enum, DTO + validator, servisler, repository/UnitOfWork, middleware |
| `FatihTasci.Data` | `AppDbContext`, migration'lar, `DbSeeder` |
| `FatihTasci.Server` | Admin JSON API (`/api/*`), vitrin Razor sayfaları, medya (`/media`), panelin derlenmiş hali (`/panel`) |
| `FatihTasci.Admin` | Yönetim paneli: React 19 + TS + Vite + shadcn/ui |

## Komutlar

- Server: `dotnet run --project FatihTasci.Server` → http://localhost:5080 (vitrin), http://localhost:5080/panel (panelin derlenmiş hali)
- Panel geliştirme: `cd FatihTasci.Admin && npm run dev` → http://localhost:5173/panel/ (`/api` ve `/media` Vite proxy'si ile 5080'e gider, CORS yok)
- Panel derleme: `npm run build` (çıktı `FatihTasci.Server/wwwroot/panel`), `npm run lint`
- Migration: `dotnet ef migrations add <Ad> --project FatihTasci.Data --startup-project FatihTasci.Server -o Migrations`

## Ortam ve ayarlar

- .NET 10 (`net10.0`), SQL Server (geliştirmede LocalDB). Açılışta `Database.Migrate()` ve `DbSeeder.SeedAsync` çalışır; seed her bölümü yalnız tablo boşsa yazar.
- `JwtSettings:Secret` en az 32 karakter olmalı, yoksa uygulama açılmaz. Yönetici kullanıcı `SeedAdmin` ayarından oluşturulur; şifre koda yazılmaz.
- `Storage:Root` (varsayılan `Storage`): `public/` `/media` altında sunulur (WebP varyantları, ekspertiz PDF), `private/` sunulmaz (orijinal fotoğraflar).
- Şifreler `PasswordHasher<UserEntity>` (PBKDF2) ile saklanır. Signage'daki MD5 kullanılmaz.

## Kurallar

Signage'ın CLAUDE.md kuralları geçerlidir, şu farklarla:

- **Tenant yok:** `ITenantProvider`, `X-Tenant-Id`, `ignoreTenant` parametreleri yoktur. Denetim alanları `ICurrentUserProvider` ile doldurulur.
- **Dil:** i18next yoktur, Signage kural 8 (no magic strings) geçersizdir. **Metinler doğrudan Türkçe yazılır.** Enum'ların ekranda görünen adları `[Display(Name)]` ile verilir; panel bunları `GET /api/lookups`'tan alır.

### Backend

1. `record` yok, her model `class`.
2. FluentValidation validator'ı aynı dosyada, request modelinin hemen altında (`<Model>Validator`).
3. `Result.Message` metinleri Türkçe ve kullanıcıya gösterilebilir (panel toast'ları bunları gösterir).
4. Yanıtlar `ServiceResultModel<T>` zarfıyla döner; controller her zaman `Ok(result)` döndürür, HTTP durum kodunu `ServiceResultResponseProccesor` `Code`'dan ayarlar. Doğrulama hataları da aynı zarfla gelir (`Data` = alan → mesajlar).
5. `DomainService.Result(...)` ile yalnız mesaj dönerken isimli argüman kullanın: `Result(message: "...")`. Aksi halde generic `Result<T>(T data)` seçilir.
6. Tam nitelikli tip adı yerine `using`.
7. `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json` anahtarları senkron tutulur.
8. Model değişince migration eklenir ve commit'e dahil edilir.
9. Slug hiçbir yerde elle girilmez: `SlugHelper`. Marka / seri / model adı değişince eski slug `SlugRedirect`'e yazılır (301).
10. Araç sihirbazı kuralları iki tarafta aynıdır: `Core/Models/Domain/Vehicle/VehicleRequestModels.cs` ↔ `Admin/src/pages/vehicles/wizard/schemas.ts`. Biri değişirse diğeri de güncellenir.
11. Hasar şeması parça yerleşimi iki yerde aynıdır: `Server/Views/Shared/_DamageDiagram.cshtml` ↔ `Admin/src/components/damage-diagram/parts.ts`.

### Panel (Signage kural 9-18 aynen)

- shadcn primitive'leri (ham `<button>`/`<input>` yok), `components/ui` bileşenlerine görünüm class'ı yok (ESLint `gora/no-design-classes`).
- Her alan doğrulanır (react-hook-form + zod) ve altında açıklama/hata gösterilir: `InputField`, `components/form/form-fields.tsx` (`SelectField`, `NumberField`, `TextareaField`, `SwitchField`).
- `pages/<feature>/index.tsx` + `components/`, iç içe ternary yok, `autoComplete="off"`.
- Effect içinde senkron `setState` yok (`react-hooks/set-state-in-effect`); veri yükleme için `hooks/use-query.ts`.
- `eslint-suppressions.json` yalnız Signage'dan gelen `components/ui` ihlallerini içerir; yeni ihlal eklenmez.

### Vitrin (Razor)

- Faz 1 sade tasarım: `wwwroot/css/site.css` (renk token'ları PROJE_DOKUMANI §6), `wwwroot/js/site.js` (çerçevesiz küçük etkileşimler).
- Dış servislerden yazı tipi / script yüklenmez (KVKK). Formlarda antiforgery, rate limit (`forms`), KVKK onayı ve bot tuzağı (`Website` alanı) vardır.
