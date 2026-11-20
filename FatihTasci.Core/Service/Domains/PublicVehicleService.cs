using FatihTasci.Core.Entity.Catalog;
using FatihTasci.Core.Entity.Common;
using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Enums;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain.Public;
using FatihTasci.Core.Providers;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// Vitrin sorguları. İlanlarda yayındaki ve rezerve araçlar görünür; satılmış araç sayfası
/// "Satıldı" rozetiyle açık kalır. Taslak ve arşiv hiçbir zaman görünmez.
/// </summary>
public class PublicVehicleService : DomainService, IPublicVehicleService
{
    public const int PageSize = 12;

    private static readonly VehicleStatus[] ListedStatuses = { VehicleStatus.Published, VehicleStatus.Reserved };
    private static readonly VehicleStatus[] DetailStatuses = { VehicleStatus.Published, VehicleStatus.Reserved, VehicleStatus.Sold };
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<VehicleEntity> _vehicleRepository;
    private readonly IRepository<BrandEntity> _brandRepository;
    private readonly IRepository<SeriesEntity> _seriesRepository;
    private readonly IRepository<SlugRedirectEntity> _redirectRepository;
    private readonly IMediaStorage _storage;

    public PublicVehicleService(IUnitOfWork unitOfWork, IMediaStorage storage)
    {
        _unitOfWork = unitOfWork;
        _vehicleRepository = unitOfWork.GetRepository<VehicleEntity>();
        _brandRepository = unitOfWork.GetRepository<BrandEntity>();
        _seriesRepository = unitOfWork.GetRepository<SeriesEntity>();
        _redirectRepository = unitOfWork.GetRepository<SlugRedirectEntity>();
        _storage = storage;
    }

    /// <summary>
    /// Ana sayfadaki araç kaydırıcısının "Vitrin" sekmesindeki araç sayısı
    /// </summary>
    public const int ShowcaseSize = 8;

    /// <summary>
    /// Ana sayfadaki "Yeni Gelenler" sekmesindeki araç sayısı
    /// </summary>
    public const int LatestSize = 12;

    /// <summary>
    /// "Fırsat Araçları" sekmesindeki en fazla araç sayısı
    /// </summary>
    public const int OpportunityLimit = 30;

    public async Task<HomePageModel> GetHomeAsync()
    {
        var listed = _vehicleRepository.GetQuery().Where(x => ListedStatuses.Contains(x.Status));

        // Vitrin sekmesi: öne çıkanlar vitrin sırasıyla, eksik kalırsa en yeniler
        var showcaseIds = await listed.Where(x => x.IsFeatured)
            .OrderBy(x => x.FeaturedOrder).ThenByDescending(x => x.PublishedAt)
            .Select(x => x.Id).Take(ShowcaseSize).ToListAsync();
        if (showcaseIds.Count < ShowcaseSize)
        {
            showcaseIds.AddRange(await listed.Where(x => !showcaseIds.Contains(x.Id))
                .OrderByDescending(x => x.PublishedAt)
                .Select(x => x.Id).Take(ShowcaseSize - showcaseIds.Count).ToListAsync());
        }

        var showcase = await ToCardsAsync(listed.Where(x => showcaseIds.Contains(x.Id)));

        return new HomePageModel
        {
            Showcase = showcaseIds.Select(id => showcase.First(c => c.Id == id)).ToList(),
            Latest = await ToCardsAsync(listed.Where(x => !showcaseIds.Contains(x.Id)).OrderByDescending(x => x.PublishedAt).Take(LatestSize)),
            Opportunities = await ToCardsAsync(listed.Where(x => x.IsOpportunity)
                .OrderByDescending(x => x.PublishedAt).Take(OpportunityLimit)),
            ListingCount = await listed.CountAsync(),
            Brands = await GetListedBrandsAsync(listed),
            // Hızlı aramanın Model seçenekleri; marka seçilince site.js yalnız o markanınkileri gösterir
            Series = await listed
                .GroupBy(x => new { BrandSlug = x.Brand.Slug, x.Series.Slug, x.Series.Name })
                .Select(g => new FilterOptionModel { Parent = g.Key.BrandSlug, Value = g.Key.Slug, Label = g.Key.Name, Count = g.Count() })
                .OrderBy(x => x.Label)
                .ToListAsync()
        };
    }

    /// <summary>
    /// İlanda aracı olan markalar, araç sayıları ve logolarıyla
    /// </summary>
    private static Task<List<FilterOptionModel>> GetListedBrandsAsync(IQueryable<VehicleEntity> listed)
    {
        return listed
            .GroupBy(x => new { x.Brand.Slug, x.Brand.Name, x.Brand.LogoPath, x.Brand.SortOrder })
            .OrderBy(g => g.Key.SortOrder).ThenBy(g => g.Key.Name)
            .Select(g => new FilterOptionModel { Value = g.Key.Slug, Label = g.Key.Name, LogoUrl = g.Key.LogoPath, Count = g.Count() })
            .ToListAsync();
    }

    public async Task<CatalogPathResult> ResolveCatalogPathAsync(string brandSlug, string seriesSlug)
    {
        if (string.IsNullOrEmpty(brandSlug))
            return new CatalogPathResult { Found = true };

        var result = new CatalogPathResult();
        var brand = await _brandRepository.GetFirstOrDefaultAsync(predicate: x => x.Slug == brandSlug && x.IsActive);
        if (brand == null)
        {
            var redirect = await _redirectRepository.GetFirstOrDefaultAsync(predicate: x => x.EntityType == SlugEntityType.Brand && x.OldSlug == brandSlug);
            brand = redirect == null ? null : await _brandRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == redirect.EntityId && x.IsActive);
            if (brand == null)
                return result;
            result.RedirectPath = $"/ilanlar/{brand.Slug}";
        }

        result.BrandId = brand.Id;
        result.BrandName = brand.Name;
        result.BrandSlug = brand.Slug;

        if (!string.IsNullOrEmpty(seriesSlug))
        {
            var series = await _seriesRepository.GetFirstOrDefaultAsync(predicate: x => x.BrandId == brand.Id && x.Slug == seriesSlug && x.IsActive);
            if (series == null)
            {
                var redirect = await _redirectRepository.GetFirstOrDefaultAsync(
                    predicate: x => x.EntityType == SlugEntityType.Series && x.ParentId == brand.Id && x.OldSlug == seriesSlug);
                series = redirect == null ? null : await _seriesRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == redirect.EntityId && x.IsActive);
                if (series == null)
                    return result;
                result.RedirectPath = $"/ilanlar/{brand.Slug}/{series.Slug}";
            }
            else if (result.RedirectPath != null)
            {
                result.RedirectPath = $"/ilanlar/{brand.Slug}/{series.Slug}";
            }

            result.SeriesId = series.Id;
            result.SeriesName = series.Name;
            result.SeriesSlug = series.Slug;
        }

        result.Found = true;
        return result;
    }

    public async Task<ListingsPageModel> GetListingsAsync(ListingFilterModel filter, CatalogPathResult path)
    {
        var baseQuery = _vehicleRepository.GetQuery().Where(x => ListedStatuses.Contains(x.Status));
        var query = baseQuery;

        if (path.BrandId.HasValue)
            query = query.Where(x => x.BrandId == path.BrandId);
        if (path.SeriesId.HasValue)
            query = query.Where(x => x.SeriesId == path.SeriesId);
        if (filter.Fuel.HasValue)
            query = query.Where(x => x.FuelType == filter.Fuel);
        if (filter.Transmission.HasValue)
            query = query.Where(x => x.Transmission == filter.Transmission);
        if (filter.Body.HasValue)
            query = query.Where(x => x.BodyType == filter.Body);
        if (filter.YearMin.HasValue)
            query = query.Where(x => x.Year >= filter.YearMin);
        if (filter.YearMax.HasValue)
            query = query.Where(x => x.Year <= filter.YearMax);
        if (filter.PriceMin.HasValue)
            query = query.Where(x => x.ShowPrice && (x.CampaignPrice ?? x.Price) >= filter.PriceMin);
        if (filter.PriceMax.HasValue)
            query = query.Where(x => x.ShowPrice && (x.CampaignPrice ?? x.Price) <= filter.PriceMax);
        if (filter.KmMax.HasValue)
            query = query.Where(x => x.Kilometer <= filter.KmMax);

        query = filter.Sort switch
        {
            "fiyat-artan" => query.OrderBy(x => x.CampaignPrice ?? x.Price),
            "fiyat-azalan" => query.OrderByDescending(x => x.CampaignPrice ?? x.Price),
            "km" => query.OrderBy(x => x.Kilometer),
            "yil" => query.OrderByDescending(x => x.Year).ThenByDescending(x => x.PublishedAt),
            _ => query.OrderByDescending(x => x.PublishedAt)
        };

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        var page = Math.Clamp(filter.Page, 1, totalPages);

        var model = new ListingsPageModel
        {
            Filter = filter,
            TotalCount = total,
            Page = page,
            TotalPages = totalPages,
            BrandName = path.BrandName,
            SeriesName = path.SeriesName,
            Items = await ToCardsAsync(query.Skip((page - 1) * PageSize).Take(PageSize))
        };

        model.Brands = await GetListedBrandsAsync(baseQuery);

        if (path.BrandId.HasValue)
        {
            model.Series = await baseQuery
                .Where(x => x.BrandId == path.BrandId)
                .GroupBy(x => new { x.Series.Slug, x.Series.Name })
                .Select(g => new FilterOptionModel { Value = g.Key.Slug, Label = g.Key.Name, Count = g.Count() })
                .OrderBy(x => x.Label)
                .ToListAsync();
        }

        return model;
    }

    public async Task<PublicVehicleDetailModel> GetVehicleAsync(int listingNo)
    {
        var x = await _vehicleRepository.GetQuery()
            .Include(v => v.Brand)
            .Include(v => v.Series)
            .Include(v => v.CarModel)
            .Include(v => v.ExteriorColor)
            .Include(v => v.Advisor)
            .Include(v => v.BodyParts)
            .Include(v => v.Features).ThenInclude(f => f.Feature)
            .Include(v => v.Media)
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => v.ListingNo == listingNo && DetailStatuses.Contains(v.Status));

        if (x == null)
            return null;

        // Görüntülenme sayısı (izleme olmadan, tek UPDATE ile)
        await _vehicleRepository.GetQuery().Where(v => v.Id == x.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(v => v.ViewCount, v => v.ViewCount + 1));

        var model = new PublicVehicleDetailModel
        {
            Id = x.Id,
            ListingNo = x.ListingNo,
            Slug = x.Slug,
            Url = VehicleService.BuildPublicUrl(x.Slug, x.ListingNo),
            Title = x.Title,
            BrandName = x.Brand.Name,
            BrandSlug = x.Brand.Slug,
            BrandLogoUrl = x.Brand.LogoPath,
            SeriesName = x.Series.Name,
            SeriesSlug = x.Series.Slug,
            ModelName = x.CarModel.Name,
            Status = x.Status,
            Price = x.Price,
            CampaignPrice = x.CampaignPrice,
            ShowPrice = x.ShowPrice,
            HeavyDamageRecord = x.HeavyDamageRecord,
            TramerAmount = x.TramerAmount,
            ExpertiseReportUrl = _storage.ToUrl(x.ExpertiseReportPath),
            Description = x.Description,
            // Pasife alınan (galeriden ayrılan) danışman sitede gösterilmez, galerinin numaraları kullanılır.
            AdvisorName = x.Advisor is { IsActive: true } ? $"{x.Advisor.Name} {x.Advisor.Surname}" : null,
            AdvisorPhone = x.Advisor is { IsActive: true } ? x.Advisor.Phone : null,
            PublishedAt = x.PublishedAt,
            SoldAt = x.SoldAt,
            VideoUrl = x.VideoUrl,
            VideoEmbedUrl = ToYoutubeEmbed(x.VideoUrl),
            Video = MediaService.ToVideoModel(x, _storage),
            Summary = BuildSummary(x),
            Specs = BuildSpecs(x),
            // Galeri kapak fotoğrafıyla başlar, ardından paneldeki sırayla devam eder.
            Images = x.Media.OrderByDescending(m => m.IsCover).ThenBy(m => m.SortOrder).Select(m => new GalleryImageModel
            {
                SmallUrl = _storage.ToUrl(m.SmallPath),
                MediumUrl = _storage.ToUrl(m.MediumPath),
                LargeUrl = _storage.ToUrl(m.LargePath),
                Width = m.Width,
                Height = m.Height,
                Alt = string.IsNullOrEmpty(m.AltText) ? x.Title : m.AltText
            }).ToList(),
            BodyParts = Enum.GetValues<BodyPart>().Select(part =>
            {
                var saved = x.BodyParts.FirstOrDefault(p => p.Part == part);
                var state = saved?.State ?? BodyPartState.Original;
                return new BodyPartViewModel
                {
                    Part = part,
                    PartName = part.GetDisplayName(),
                    State = state,
                    StateName = state.GetDisplayName(),
                    Note = saved?.Note
                };
            }).ToList(),
            Features = x.Features
                .Where(f => f.Feature.IsActive)
                .GroupBy(f => f.Feature.Category)
                .OrderBy(g => g.Key)
                .Select(g => new FeatureListGroupModel
                {
                    CategoryName = g.Key.GetDisplayName(),
                    Features = g.OrderBy(f => f.Feature.SortOrder).ThenBy(f => f.Feature.Name).Select(f => f.Feature.Name).ToList()
                }).ToList()
        };

        model.Share = BuildShare(x, model.Images.FirstOrDefault()?.LargeUrl);
        model.Year = x.Year;
        model.Kilometer = x.Kilometer;
        model.FuelName = EnumHelper.GetDisplayName(x.FuelType);
        model.TransmissionName = EnumHelper.GetDisplayName(x.Transmission);
        model.BodyTypeName = EnumHelper.GetDisplayName(x.BodyType);
        model.EngineCc = x.EngineCc;
        model.PowerHp = x.PowerHp;
        model.Drivetrain = x.Drivetrain;
        model.ExteriorColorName = x.ExteriorColor?.Name;
        model.InteriorColor = x.InteriorColor;
        model.Condition = x.Condition;

        var similarQuery = _vehicleRepository.GetQuery()
            .Where(v => v.Id != x.Id && ListedStatuses.Contains(v.Status) && (v.SeriesId == x.SeriesId || v.BodyType == x.BodyType))
            .OrderByDescending(v => v.SeriesId == x.SeriesId)
            .ThenByDescending(v => v.PublishedAt)
            .Take(4);
        model.Similar = await ToCardsAsync(similarQuery);

        return model;
    }

    public async Task<VehicleShareModel> GetShareAsync(int listingNo)
    {
        var x = await _vehicleRepository.GetQuery()
            .Where(v => v.ListingNo == listingNo && DetailStatuses.Contains(v.Status))
            .Select(v => new
            {
                Vehicle = v,
                CoverPath = v.Media.OrderByDescending(m => m.IsCover).ThenBy(m => m.SortOrder).Select(m => m.LargePath).FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        return x == null ? null : BuildShare(x.Vehicle, _storage.ToUrl(x.CoverPath));
    }

    public async Task<List<SitemapEntryModel>> GetSitemapEntriesAsync()
    {
        var listed = _vehicleRepository.GetQuery().Where(x => ListedStatuses.Contains(x.Status));

        var vehicles = await listed
            .OrderByDescending(x => x.PublishedAt)
            .Select(x => new
            {
                x.Slug,
                x.ListingNo,
                LastModified = x.LastUpdateDate ?? x.PublishedAt ?? x.CreateDate,
                CoverPath = x.Media.Where(m => m.IsCover).Select(m => m.LargePath).FirstOrDefault()
            })
            .ToListAsync();

        var series = await listed
            .GroupBy(x => new { BrandSlug = x.Brand.Slug, SeriesSlug = x.Series.Slug })
            .Select(g => new { g.Key.BrandSlug, g.Key.SeriesSlug, LastModified = g.Max(v => v.LastUpdateDate ?? v.PublishedAt ?? v.CreateDate) })
            .ToListAsync();

        var entries = series
            .GroupBy(x => x.BrandSlug)
            .OrderBy(g => g.Key)
            .SelectMany(g => new[] { new SitemapEntryModel { Path = $"/ilanlar/{g.Key}", LastModified = g.Max(s => s.LastModified) } }
                .Concat(g.OrderBy(s => s.SeriesSlug).Select(s => new SitemapEntryModel { Path = $"/ilanlar/{s.BrandSlug}/{s.SeriesSlug}", LastModified = s.LastModified })))
            .ToList();

        entries.AddRange(vehicles.Select(x => new SitemapEntryModel
        {
            Path = VehicleService.BuildPublicUrl(x.Slug, x.ListingNo),
            LastModified = x.LastModified,
            ImageUrl = _storage.ToUrl(x.CoverPath)
        }));

        return entries;
    }

    private static VehicleShareModel BuildShare(VehicleEntity x, string coverUrl)
    {
        return new VehicleShareModel
        {
            ListingNo = x.ListingNo,
            Title = x.Title,
            Summary = BuildSummary(x),
            Price = x.Price,
            CampaignPrice = x.CampaignPrice,
            ShowPrice = x.ShowPrice,
            Status = x.Status,
            CoverUrl = coverUrl
        };
    }

    public Task<Dictionary<string, List<EnumOption>>> GetFilterOptionsAsync()
    {
        var options = new Dictionary<string, List<EnumOption>>
        {
            ["fuel"] = EnumHelper.GetOptions<FuelType>(),
            ["transmission"] = EnumHelper.GetOptions<TransmissionType>(),
            ["body"] = EnumHelper.GetOptions<BodyType>()
        };
        return Task.FromResult(options);
    }

    private async Task<List<VehicleCardModel>> ToCardsAsync(IQueryable<VehicleEntity> query)
    {
        var rows = await query.Select(CardProjection()).ToListAsync();
        return rows.Select(r => new VehicleCardModel
        {
            Title = r.Title,
            Url = VehicleService.BuildPublicUrl(r.Slug, r.ListingNo),
            Id = r.Id,
            CoverUrl = _storage.ToUrl(r.Cover?.MediumPath),
            CoverSmallUrl = _storage.ToUrl(r.Cover?.SmallPath),
            CoverLargeUrl = _storage.ToUrl(r.Cover?.LargePath),
            CoverAlt = r.Title,
            BrandName = r.BrandName,
            SeriesName = r.SeriesName,
            ModelName = r.ModelName,
            Year = r.Year,
            Kilometer = r.Kilometer,
            FuelName = EnumHelper.GetDisplayName(r.FuelType),
            TransmissionName = EnumHelper.GetDisplayName(r.Transmission),
            Price = r.Price,
            CampaignPrice = r.CampaignPrice,
            ShowPrice = r.ShowPrice,
            Status = r.Status,
            HasVideo = r.HasVideo
        }).ToList();
    }

    private static Expression<Func<VehicleEntity, CardRow>> CardProjection()
    {
        return x => new CardRow
        {
            Title = x.Title,
            BrandName = x.Brand.Name,
            SeriesName = x.Series.Name,
            ModelName = x.CarModel.Name,
            Slug = x.Slug,
            ListingNo = x.ListingNo,
            Year = x.Year,
            Kilometer = x.Kilometer,
            FuelType = x.FuelType,
            Transmission = x.Transmission,
            Price = x.Price,
            CampaignPrice = x.CampaignPrice,
            ShowPrice = x.ShowPrice,
            Status = x.Status,
            Id = x.Id,
            HasVideo = x.VideoPath != null,
            Cover = x.Media.Where(m => m.IsCover)
                .Select(m => new CoverRow { SmallPath = m.SmallPath, MediumPath = m.MediumPath, LargePath = m.LargePath })
                .FirstOrDefault()
        };
    }

    private static List<string> BuildSummary(VehicleEntity x)
    {
        var items = new List<string>();
        if (x.Year.HasValue) items.Add(x.Year.Value.ToString());
        if (x.Kilometer.HasValue) items.Add($"{x.Kilometer.Value.ToString("N0", Turkish)} km");
        if (x.FuelType.HasValue) items.Add(x.FuelType.Value.GetDisplayName());
        if (x.Transmission.HasValue) items.Add(x.Transmission.Value.GetDisplayName());
        if (x.BodyType.HasValue) items.Add(x.BodyType.Value.GetDisplayName());
        if (x.PowerHp.HasValue) items.Add($"{x.PowerHp} hp");
        return items;
    }

    private static List<SpecItemModel> BuildSpecs(VehicleEntity x)
    {
        var specs = new List<SpecItemModel>
        {
            Spec("İlan No", x.ListingNo.ToString()),
            Spec("Marka", x.Brand.Name),
            Spec("Model", x.Series.Name),
            Spec("Paket", x.CarModel.Name),
            Spec("Yıl", x.Year?.ToString()),
            Spec("Kilometre", x.Kilometer.HasValue ? $"{x.Kilometer.Value.ToString("N0", Turkish)} km" : null),
            Spec("Yakıt", EnumHelper.GetDisplayName(x.FuelType)),
            Spec("Vites", EnumHelper.GetDisplayName(x.Transmission)),
            Spec("Kasa Tipi", EnumHelper.GetDisplayName(x.BodyType)),
            Spec("Motor Hacmi", x.EngineCc.HasValue ? $"{x.EngineCc.Value.ToString("N0", Turkish)} cc" : null),
            Spec("Motor Gücü", x.PowerHp.HasValue ? $"{x.PowerHp} hp" : null),
            Spec("Çekiş", EnumHelper.GetDisplayName(x.Drivetrain)),
            Spec("Batarya", x.BatteryKwh.HasValue ? $"{x.BatteryKwh} kWh" : null),
            Spec("Menzil", x.RangeKm.HasValue ? $"{x.RangeKm} km" : null),
            Spec("Dış Renk", x.ExteriorColor?.Name),
            Spec("İç Renk", x.InteriorColor),
            Spec("Durumu", x.Condition.GetDisplayName()),
            Spec("Garanti", x.HasWarranty ? (x.WarrantyEndDate.HasValue ? $"Var ({x.WarrantyEndDate.Value.ToString("MMMM yyyy", Turkish)} sonuna kadar)" : "Var") : "Yok"),
            Spec("Ağır Hasar Kayıtlı", x.HeavyDamageRecord ? "Evet" : "Hayır"),
            Spec("Plaka / Uyruk", x.PlateOrigin.GetDisplayName()),
            Spec("Takas", x.AcceptsTrade ? "Takasa uygun" : "Takas yok")
        };

        return specs.Where(s => !string.IsNullOrEmpty(s.Value)).ToList();
    }

    private static SpecItemModel Spec(string label, string value) => new() { Label = label, Value = value };

    /// <summary>
    /// YouTube bağlantısını gizlilik dostu (nocookie) gömme adresine çevirir. YouTube değilse null.
    /// </summary>
    private static string ToYoutubeEmbed(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var match = Regex.Match(url, @"(?:youtube\.com/(?:watch\?(?:.*&)?v=|shorts/|embed/)|youtu\.be/)([A-Za-z0-9_-]{11})");
        return match.Success ? $"https://www.youtube-nocookie.com/embed/{match.Groups[1].Value}" : null;
    }

    private class CardRow
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string BrandName { get; set; }
        public string SeriesName { get; set; }
        public string ModelName { get; set; }
        public string Slug { get; set; }
        public int ListingNo { get; set; }
        public int? Year { get; set; }
        public int? Kilometer { get; set; }
        public FuelType? FuelType { get; set; }
        public TransmissionType? Transmission { get; set; }
        public decimal? Price { get; set; }
        public decimal? CampaignPrice { get; set; }
        public bool ShowPrice { get; set; }
        public VehicleStatus Status { get; set; }
        public bool HasVideo { get; set; }
        public CoverRow Cover { get; set; }
    }

    private class CoverRow
    {
        public string SmallPath { get; set; }
        public string MediumPath { get; set; }
        public string LargePath { get; set; }
    }
}
