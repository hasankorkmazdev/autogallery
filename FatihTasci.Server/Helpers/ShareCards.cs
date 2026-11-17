using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Public;
using FatihTasci.Core.Models.Domain.Setting;

namespace FatihTasci.Server.Helpers;

/// <summary>
/// Paylaşım görseli (og:image) kartlarının içeriği. Araç sayfası ve görsel ucu (ShareImageController) aynı kartı
/// üretir; böylece sayfadaki ?v= sürümü ile üretilen dosya aynı olur.
/// </summary>
public static class ShareCards
{
    public const string SiteImagePath = "/paylasim/galeri.jpg";
    public const string SiteTagline = "Seçkin ikinci el ve sıfır araçlar";

    public static ShareCardModel ForVehicle(VehicleShareModel vehicle, SiteSettingsModel settings)
    {
        var sold = vehicle.Status == VehicleStatus.Sold;
        var campaign = !sold && SiteFormat.HasCampaign(vehicle.Price, vehicle.CampaignPrice, vehicle.ShowPrice);

        return new ShareCardModel
        {
            CacheName = $"vehicle-{vehicle.ListingNo}",
            Title = vehicle.Title,
            Subtitle = string.Join(" · ", vehicle.Summary.Take(4)),
            PriceText = sold ? null : SiteFormat.EffectivePrice(vehicle.Price, vehicle.CampaignPrice, vehicle.ShowPrice),
            OldPriceText = campaign ? SiteFormat.Money(vehicle.Price.Value) : null,
            Badge = SiteFormat.StatusBadge(vehicle.Status) ?? (campaign ? "Kampanya" : null),
            BackgroundUrl = vehicle.CoverUrl,
            LogoUrl = settings.LogoUrl,
            GalleryName = SiteFormat.GalleryName(settings)
        };
    }

    public static ShareCardModel ForSite(SiteSettingsModel settings, string backgroundUrl)
    {
        return new ShareCardModel
        {
            CacheName = "site",
            Title = SiteFormat.GalleryName(settings),
            Subtitle = SiteTagline,
            BackgroundUrl = backgroundUrl,
            LogoUrl = settings.LogoUrl,
            GalleryName = SiteFormat.GalleryName(settings)
        };
    }

    public static string VehicleImagePath(int listingNo, string version) => $"/paylasim/arac/{listingNo}.jpg?v={version}";
}
