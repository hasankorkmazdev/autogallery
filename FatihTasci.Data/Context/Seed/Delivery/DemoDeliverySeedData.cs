namespace FatihTasci.Data.Context.Seed.Delivery;

/// <summary>
/// Örnek teslimatlar (DemoDeliverySeeder): Id, Server/Assets/Seed/Deliveries'teki fotoğraf, araç, müşteri, şehir ve kaç
/// gün önce teslim edildiği. Pexels / Unsplash stok fotoğraflar, kaynakları Deliveries/KAYNAK.md'de.
/// </summary>
internal static class DemoDeliverySeedData
{
    public static readonly (int Id, string File, string Vehicle, string Customer, string City, int DaysAgo)[] Samples =
    [
        (1, "01-rav4-cift.jpg", "2022 Toyota RAV4 2.5 Hybrid Passion", "Seda ve Onur K.", "İstanbul", 3),
        (2, "02-jimny-alanya.jpg", "2021 Suzuki Jimny 1.5 GLX AllGrip", "Derya S.", "Antalya", 7),
        (3, "03-audi-a3-ankara.jpg", "2023 Audi A3 Sedan 35 TFSI S line", "Burak E.", "Ankara", 12),
        (4, "04-anahtar-alan-musteri.jpg", "2023 Toyota C-HR 1.8 Hybrid Passion X", "Serkan B.", "Kocaeli", 18),
        (5, "05-arabasina-sarilan.jpg", "2022 Peugeot 3008 1.5 BlueHDi GT", "Okan Y.", "Bursa", 25),
        (6, "06-golf-zonguldak.jpg", "2017 Volkswagen Golf 1.4 TSI Highline", "Mehmet A.", "Zonguldak", 33),
        (7, "07-aile-anahtar.jpg", "2022 Volvo XC90 B5 Inscription", "Murat K.", "Eskişehir", 44),
        (8, "08-civic-izmir.jpg", "2000 Honda Civic 1.6 VTi", "Emre T.", "İzmir", 56)
    ];
}
