# Örnek araç fotoğrafları

Yalnız **geliştirme ortamında** `DbSeeder.SeedDemoVehiclesAsync` örnek (yayında) araç oluşturmak için kullanır; yayın
paketine kopyalanmaz (`FatihTasci.Server.csproj`). Galerinin aracı değildirler, canlı sitede kullanılmaz: fotoğraflar
başka bir galerinin Sahibinden ilanından, üzerlerinde o galerinin logosu, portal filigranı ve plaka var.

Her klasör bir araçtır; klasör adı `FatihTasci.Data/Context/Seed/DemoVehicleSeedData.cs`'teki örnek listesinde geçer.
Fotoğraflar dosya adı sırasıyla eklenir, ilki kapaktır. Her örnek bir kez eklenir (`__DemoVehiclesSeeded`); panelden
silinen örnek geri gelmez. Fiyatlar tahminidir.

| Klasör | Araç | Kaynak |
|---|---|---|
| 2025-mg7-red-edition | Boyasız 2025 MG MG7 1.5 Excellence Red Edition, 19.000 km | Sahibinden ilan no 1350723239 (BD Otomotiv) |
| 2025-mercedes-sl43-amg | Bayi 2025 Mercedes-AMG SL 43, 6.100 km | Sahibinden ilan no 1348316881 (Ekrem Otonomi); bayinin dış cephe fotoğrafı (araç yok) alınmadı |
