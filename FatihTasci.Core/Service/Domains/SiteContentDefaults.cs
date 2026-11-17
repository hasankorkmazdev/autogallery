using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Content;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// Site İçeriği'nde hiç kaydedilmemiş alanların başlangıç metinleri. Panelden değiştirilir;
/// firma adı burada geçmez (ayarlardan gelir). Rakamlar galeri sahibinin verdiği bilgilerdir.
/// </summary>
public static class SiteContentDefaults
{
    /// <summary>
    /// Galerinin drone videosu (wwwroot/video, ffmpeg ile 1080p ve 720p'ye sıkıştırıldı; afiş ilk kare).
    /// </summary>
    public static HeroVideoModel HeroVideo() => new()
    {
        VideoHdUrl = "/video/galeri-1080.mp4",
        VideoSdUrl = "/video/galeri-720.mp4",
        PosterUrl = "/video/galeri-poster-1920.webp",
        PosterSmallUrl = "/video/galeri-poster-960.webp",
        IsCustom = false
    };

    public static string ServiceSummary(SiteServiceType type) => type switch
    {
        SiteServiceType.BuyVehicle =>
            "İlanlarımızdaki ekspertizli araçları inceleyin ya da aradığınız aracı bize söyleyin; bütçenize ve beklentinize uygun aracı birlikte bulalım.",
        SiteServiceType.Consignment =>
            "Aracınızı galerimizde sergileyelim, alıcısını biz bulalım. Satış sürecinin tamamını sizin adınıza güvenle yürütüyoruz.",
        SiteServiceType.SellVehicle =>
            "Aracınızı değerinde ve hızlıca alalım. Bilgilerini gönderin, ekspertizin ardından size net bir teklif sunalım.",
        _ => string.Empty
    };

    public static string ServiceBody(SiteServiceType type) => type switch
    {
        SiteServiceType.BuyVehicle =>
            "İlanlarımızdaki her araç ekspertiz raporu, boya ve değişen bilgisiyle şeffaf biçimde sunulur. Beğendiğiniz aracı showroomumuzda yakından görebilir, test sürüşüne çıkabilirsiniz.\n\n" +
            "Aradığınız araç ilanlarımızda yoksa marka, model, yıl ve bütçe tercihlerinizi bize iletin. Size uygun aracı bulduğumuzda sizinle iletişime geçelim.\n\n" +
            "Takasa uygun araçlarda mevcut aracınızı da değerlendirebiliriz. Noter ve evrak işlemlerinde süreç boyunca yanınızdayız.",
        SiteServiceType.Consignment =>
            "Konsinye satışta aracınız satılana kadar sizin adınıza galerimizde sergilenir. Satış fiyatını piyasa koşullarına göre birlikte belirleriz.\n\n" +
            "Aracınız ekspertizden geçer, fotoğrafları çekilir ve sitemizde ilan olarak yayınlanır. Alıcılarla görüşme, test sürüşü ve pazarlık süreçlerini biz yürütürüz.\n\n" +
            "Satış gerçekleştiğinde noter işlemleri tamamlanır ve ödemeniz yapılır.",
        SiteServiceType.SellVehicle =>
            "Aracınızın marka, model, yıl, kilometre ve hasar bilgilerini WhatsApp üzerinden ya da iletişim formundan bize iletin. Fotoğraf eklemeniz teklifimizi hızlandırır.\n\n" +
            "Aracınızı galerimize getirdiğinizde ekspertiz yapılır ve piyasa değerine göre size net bir teklif sunulur.\n\n" +
            "Anlaştığımızda noter işlemleri ve ödeme hızlıca tamamlanır.",
        _ => string.Empty
    };

    public const string CorporateIntro =
        "40 yıllık tecrübemizle araç alım satımında güveni, şeffaflığı ve müşteri memnuniyetini her şeyin önünde tutuyoruz.";

    public const string CorporateAbout =
        "Otomotiv sektöründeki 40 yıllık tecrübemizle bugüne kadar 5.000'den fazla araç satışı gerçekleştirdik ve binlerce müşterimizi aradıkları araçla buluşturduk.\n\n" +
        "Her aracı satışa sunmadan önce titizlikle inceliyor, ekspertiz raporu ve hasar bilgisiyle şeffaf biçimde paylaşıyoruz. Showroomumuzda araçlarımızı yakından görebilir, ekibimizden bilgi alabilirsiniz.\n\n" +
        "Araç almak, aracınızı satmak ya da konsinye olarak değerlendirmek istediğinizde süreç boyunca yanınızdayız.";

    public const string CorporateVision =
        "Araç alım satımında güven, kalite ve şeffaflıkla örnek gösterilen; müşterilerinin ilk tercih ettiği galeri olmak.";

    public const string CorporateMission =
        "Her müşterimize ihtiyacına en uygun aracı doğru fiyat ve şeffaf bilgiyle sunmak; satış öncesinde ve sonrasında güvenilir bir çözüm ortağı olmak.";

    public static List<StatItemModel> Stats() => new()
    {
        new StatItemModel { Value = "40", Label = "Yıllık Tecrübe" },
        new StatItemModel { Value = "5.000+", Label = "Araç Satışı" },
        new StatItemModel { Value = "20.000+", Label = "Ziyaret" }
    };
}
