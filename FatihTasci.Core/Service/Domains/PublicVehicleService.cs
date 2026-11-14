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
/// Vitrin sorguları. Stokta yayındaki ve rezerve araçlar görünür; satılmış araç sayfası
/// "Satıldı" rozetiyle açık kalır. Taslak ve arşiv hiçbir zaman görünmez.
/// </summary>
public class PublicVehicleService : DomainService, IPublicVehicleService
{
    public const int PageSize = 12;

    private static readonly VehicleStatus[] StockStatuses = { VehicleStatus.Published, VehicleStatus.Reserved };
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

    public async Task<HomePageModel> GetHomeAsync()
    {
        var stock = _vehicleRepository.GetQuery().Where(x => StockStatuses.Contains(x.Status));

        return new HomePageModel
        {
            Featured = await ToCardsAsync(stock.Where(x => x.IsFeatured).OrderBy(x => x.FeaturedOrder).ThenByDescending(x => x.PublishedAt).Take(6)),
            Latest = await ToCardsAsync(stock.OrderByDescending(x => x.PublishedAt).Take(6)),
            StockCount = await stock.CountAsync(),
            Brands = await GetStockBrandsAsync(stock)
        };
    }

    /// <summary>
    /// Stokta aracı olan markalar, araç sayıları ve logolarıyla
    /// </summary>
    private static Task<List<FilterOptionModel>> GetStockBrandsAsync(IQueryable<VehicleEntity> stock)
    {
        return stock
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
            result.RedirectPath = $"/stok/{brand.Slug}";
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
                result.RedirectPath = $"/stok/{brand.Slug}/{series.Slug}";
            }
            else if (result.RedirectPath != null)
            {
                result.RedirectPath = $"/stok/{brand.Slug}/{series.Slug}";
            }

            result.SeriesId = series.Id;
            result.SeriesName = series.Name;
            result.SeriesSlug = series.Slug;
        }

        result.Found = true;
        return result;
    }

    public async Task<StockPageModel> GetStockAsync(StockFilterModel filter, CatalogPathResult path)
    {
        var baseQuery = _vehicleRepository.GetQuery().Where(x => StockStatuses.Contains(x.Status));
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

        var model = new StockPageModel
        {
            Filter = filter,
            TotalCount = total,
            Page = page,
            TotalPages = totalPages,
            BrandName = path.BrandName,
            SeriesName = path.SeriesName,
            Items = await ToCardsAsync(query.Skip((page - 1) * PageSize).Take(PageSize))
        };

        model.Brands = await GetStockBrandsAsync(baseQuery);

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
            VideoUrl = x.VideoUrl,
            VideoEmbedUrl = ToYoutubeEmbed(x.VideoUrl),
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

        var similarQuery = _vehicleRepository.GetQuery()
            .Where(v => v.Id != x.Id && StockStatuses.Contains(v.Status) && (v.SeriesId == x.SeriesId || v.BodyType == x.BodyType))
            .OrderByDescending(v => v.SeriesId == x.SeriesId)
            .ThenByDescending(v => v.PublishedAt)
            .Take(4);
        model.Similar = await ToCardsAsync(similarQuery);

        return model;
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
            CoverUrl = _storage.ToUrl(r.CoverPath),
            CoverAlt = r.Title,
            Year = r.Year,
            Kilometer = r.Kilometer,
            FuelName = EnumHelper.GetDisplayName(r.FuelType),
            TransmissionName = EnumHelper.GetDisplayName(r.Transmission),
            Price = r.Price,
            CampaignPrice = r.CampaignPrice,
            ShowPrice = r.ShowPrice,
            Status = r.Status
        }).ToList();
    }

    private static Expression<Func<VehicleEntity, CardRow>> CardProjection()
    {
        return x => new CardRow
        {
            Title = x.Title,
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
            CoverPath = x.Media.Where(m => m.IsCover).Select(m => m.MediumPath).FirstOrDefault()
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
            Spec("Seri", x.Series.Name),
            Spec("Model", x.CarModel.Name),
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
        public string Title { get; set; }
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
        public string CoverPath { get; set; }
    }
}
