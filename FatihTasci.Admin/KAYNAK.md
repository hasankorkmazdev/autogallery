# Harita: kütüphane ve karo kaynağı (panel)

Panelde **Ayarlar → Konum**'daki konum seçme haritası (`src/pages/settings/components/location-map.tsx`). Vitrin bunu
kullanmaz; vitrindeki harita Google Haritalar gömmesidir ve ziyaretçi "Haritayı göster"e basınca yüklenir (KVKK).

| Ne | Lisans / koşul | Not |
|---|---|---|
| [Leaflet](https://leafletjs.com) 1.9.x (npm `leaflet`) | BSD-2-Clause | Lisans metni `public/licenses/leaflet.txt`, panel paketiyle `/panel/licenses/leaflet.txt` olarak yayımlanır (ikili dağıtımda lisansın yeniden verilmesi şartı). |
| `@types/leaflet` | MIT | Yalnız geliştirme (tip tanımları), pakete girmez. |
| [OpenStreetMap](https://www.openstreetmap.org/copyright) karoları, `https://tile.openstreetmap.org/{z}/{x}/{y}.png` | Veri ODbL; [OSMF karo kullanım politikası](https://operations.osmfoundation.org/policies/tiles/) | Atıf haritanın üstünde görünür ("© OpenStreetMap katkıda bulunanlar", telif sayfasına bağlantı), gizlenmez. |

OSMF karo politikasına göre uyulan kurallar (kontrol: 2026-11-21):

- Karo adresi yalnız `tile.openstreetmap.org` (a/b/c alt alan adı yok, yalnız https). Panel CSP'sinde `img-src`'de bu
  adres izinlidir (`FatihTasci.Core/Middleware/SecurityMiddleware.cs`).
- Yalnız kullanıcının baktığı karolar yüklenir: önden yükleme, toplu indirme, çevrimdışı saklama, sunucuda vekil
  (proxy) ya da karolardan statik görsel üretme yoktur.
- Tarayıcı geçerli bir Referer gönderir (site geneli `Referrer-Policy: strict-origin-when-cross-origin`, kaynak adresi
  gider); bu ayar `no-referrer` yapılmaz.
- Trafik yalnız panel kullanıcılarınındır (birkaç kişi), politika ağır kullanımda erişimi kesebilir. Ücretli /
  yoğun kullanım gerekirse başka karo sağlayıcısına geçilir ve CSP güncellenir.

`react-leaflet` kullanılmaz: lisansı Hippocratic-2.1 (OSI onaylı değil, kullanım koşulu içerir). Leaflet doğrudan,
küçük bir React bileşeniyle kullanılır.
