using FatihTasci.Core.Entity.Crm;
using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Enums;
using FatihTasci.Core.Extensions.Data;
using FatihTasci.Core.Helpers;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Lead;
using FatihTasci.Core.Providers;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FatihTasci.Core.Service.Domains;

public class LeadService : DomainService, ILeadService
{
    private const string SuccessMessage = "Talebiniz alındı. En kısa sürede sizinle iletişime geçeceğiz.";
    private const string SellSuccessMessage = "Talebiniz alındı. Aracınızı inceleyip en kısa sürede teklifimizle size dönelim.";

    // Talep fotoğrafları: uzun kenar en çok 2000 px, EXIF (konum dahil) temizlenmiş WebP. Yalnız özel klasörde
    // durur, sitede sunulmaz. Çok büyük çözünürlüklü dosya (60 MP üstü) bellek için reddedilir.
    private const int PhotoMaxSize = 2000;
    private const int PhotoQuality = 82;

    private static readonly VehicleStatus[] VisibleStatuses = { VehicleStatus.Published, VehicleStatus.Reserved, VehicleStatus.Sold };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<LeadEntity> _leadRepository;
    private readonly IRepository<LeadMediaEntity> _mediaRepository;
    private readonly IRepository<VehicleEntity> _vehicleRepository;
    private readonly IMediaStorage _storage;
    private readonly ILogger<LeadService> _logger;

    public LeadService(IUnitOfWork unitOfWork, IMediaStorage storage, ILogger<LeadService> logger)
    {
        _unitOfWork = unitOfWork;
        _leadRepository = unitOfWork.GetRepository<LeadEntity>();
        _mediaRepository = unitOfWork.GetRepository<LeadMediaEntity>();
        _vehicleRepository = unitOfWork.GetRepository<VehicleEntity>();
        _storage = storage;
        _logger = logger;
    }

    /// <summary>
    /// Panelin talep fotoğrafını aldığı uç.
    /// </summary>
    public static string MediaUrl(Guid leadId, Guid mediaId) => $"/api/leads/{leadId}/media/{mediaId}";

    public async Task<ServiceResultModel> CreateAsync(CreateLeadRequestModel request, string ipAddress)
    {
        // Bot tuzağı doluysa kayıt yapılmaz ama bota başarılı görünür.
        if (!string.IsNullOrEmpty(request.Website))
        {
            _logger.LogInformation("Bot tuzağına takılan form isteği yok sayıldı. IP: {Ip}", ipAddress);
            return Result(message: SuccessMessage);
        }

        Guid? vehicleId = null;
        if (request.ListingNo.HasValue)
        {
            vehicleId = await _vehicleRepository.GetFirstOrDefaultAsync(
                selector: x => (Guid?)x.Id,
                predicate: x => x.ListingNo == request.ListingNo && VisibleStatuses.Contains(x.Status));
        }

        _leadRepository.Insert(new LeadEntity
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            VehicleId = vehicleId,
            FullName = request.FullName.Trim(),
            Phone = request.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim(),
            PreferredDate = request.Type == LeadType.TestDrive ? request.PreferredDate : null,
            KvkkConsentAt = DateTime.Now,
            Status = LeadStatus.New,
            IpAddress = ipAddress
        });
        await _unitOfWork.SaveChangesAsync();

        return Result(message: SuccessMessage);
    }

    public async Task<ServiceResultModel> CreateSellAsync(CreateSellLeadRequestModel request, string ipAddress)
    {
        if (!string.IsNullOrEmpty(request.Website))
        {
            _logger.LogInformation("Bot tuzağına takılan teklif formu yok sayıldı. IP: {Ip}", ipAddress);
            return Result(message: SellSuccessMessage);
        }

        var lead = new LeadEntity
        {
            Id = Guid.NewGuid(),
            Type = LeadType.SellOrTrade,
            FullName = request.FullName.Trim(),
            Phone = request.Phone.Trim(),
            Message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim(),
            SellBrand = request.SellBrand.Trim(),
            SellModel = request.SellModel.Trim(),
            SellPackage = string.IsNullOrWhiteSpace(request.SellPackage) ? null : request.SellPackage.Trim(),
            SellYear = request.SellYear,
            SellKm = request.SellKmValue,
            SellExpectedPrice = request.SellExpectedPriceValue,
            SellDamage = request.SellDamage,
            KvkkConsentAt = DateTime.Now,
            Status = LeadStatus.New,
            IpAddress = ipAddress
        };

        var written = new List<string>();
        try
        {
            foreach (var photo in request.Photos ?? new List<IFormFile>())
            {
                var mediaId = Guid.NewGuid();
                var path = await SavePhotoAsync(lead.Id, mediaId, photo);
                if (path == null)
                {
                    Cleanup(lead.Id, written);
                    return Result(message: $"{Path.GetFileName(photo.FileName)}: dosya okunamadı, geçerli bir fotoğraf seçin.", code: 400);
                }

                written.Add(path);
                lead.Media.Add(new LeadMediaEntity { Id = mediaId, LeadId = lead.Id, Path = path, SortOrder = lead.Media.Count });
            }

            _leadRepository.Insert(lead);
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            Cleanup(lead.Id, written);
            throw;
        }

        return Result(message: SellSuccessMessage);
    }

    public async Task<ServiceResultModel<string>> GetMediaFileAsync(Guid leadId, Guid mediaId)
    {
        var path = await _mediaRepository.GetFirstOrDefaultAsync(
            selector: x => x.Path,
            predicate: x => x.Id == mediaId && x.LeadId == leadId);
        if (path == null)
            return Result<string>(null, "Fotoğraf bulunamadı.", 404);

        var fullPath = _storage.GetFullPrivatePath(path);
        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("Talep fotoğrafı diskte bulunamadı: {Path}", path);
            return Result<string>(null, "Fotoğraf bulunamadı.", 404);
        }

        return Result(fullPath);
    }

    public async Task<ServiceResultModel<IPagedList<LeadModel>>> GetListAsync(LeadListRequestModel request)
    {
        var query = _leadRepository.GetQuery();

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);
        if (request.Type.HasValue)
            query = query.Where(x => x.Type == request.Type.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.FullName.Contains(search) || x.Phone.Contains(search) || x.Email.Contains(search));
        }

        var page = await query
            .OrderByDescending(x => x.CreateDate)
            .Select(ToModelExpression())
            .ToPagedListAsync(request.PageIndex, request.PageSize);

        foreach (var item in page.Items)
            FillMediaUrls(item);

        return Result(page);
    }

    public async Task<ServiceResultModel<LeadModel>> UpdateAsync(Guid id, UpdateLeadRequestModel request)
    {
        var lead = await _leadRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == id, disableTracking: false);
        if (lead == null)
            return Result<LeadModel>(null, "Talep bulunamadı.", 404);

        lead.Status = request.Status;
        lead.InternalNote = string.IsNullOrWhiteSpace(request.InternalNote) ? null : request.InternalNote.Trim();
        await _unitOfWork.SaveChangesAsync();

        var model = await _leadRepository.GetFirstOrDefaultAsync(selector: ToModelExpression(), predicate: x => x.Id == id);
        FillMediaUrls(model);
        return Result(model, "Talep güncellendi.");
    }

    public async Task<ServiceResultModel<int>> GetNewCountAsync()
    {
        return Result(await _leadRepository.CountAsync(x => x.Status == LeadStatus.New));
    }

    /// <summary>
    /// Fotoğrafı okur, yönünü düzeltir, küçültür ve WebP olarak özel klasöre yazar (yeniden kodlanan dosyada
    /// EXIF konumu dahil metadata kalmaz). Dosya geçerli bir fotoğraf değilse ya da çok büyükse null döner.
    /// </summary>
    private async Task<string> SavePhotoAsync(Guid leadId, Guid mediaId, IFormFile file)
    {
        // JPEG / WebP kutunun altına inmeden küçültülerek çözülür, telefon fotoğrafında bellek kullanımı düşer.
        await using var stream = file.OpenReadStream();
        using var image = ImageTools.Decode(stream, PhotoMaxSize, PhotoMaxSize);
        if (image == null)
        {
            _logger.LogWarning("Talep fotoğrafı okunamadı: {FileName}", file.FileName);
            return null;
        }

        using var resized = ImageTools.Fit(image, PhotoMaxSize, PhotoMaxSize);
        var path = $"leads/{leadId:N}/{mediaId:N}.webp";
        await using var output = ImageTools.ToWebp(resized, PhotoQuality);
        await _storage.SavePrivateAsync(path, output);
        return path;
    }

    /// <summary>
    /// Kaydedilemeyen talebin yazılmış fotoğraflarını ve boş kalan klasörünü siler.
    /// </summary>
    private void Cleanup(Guid leadId, IEnumerable<string> paths)
    {
        try
        {
            foreach (var path in paths)
                _storage.DeletePrivate(path);

            var folder = _storage.GetFullPrivatePath($"leads/{leadId:N}");
            if (Directory.Exists(folder) && !Directory.EnumerateFileSystemEntries(folder).Any())
                Directory.Delete(folder);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Talep fotoğrafları temizlenemedi: {LeadId}", leadId);
        }
    }

    private static void FillMediaUrls(LeadModel model)
    {
        if (model == null)
            return;

        foreach (var media in model.Media)
            media.Url = MediaUrl(model.Id, media.Id);
    }

    private static Expression<Func<LeadEntity, LeadModel>> ToModelExpression()
    {
        return x => new LeadModel
        {
            Id = x.Id,
            Type = x.Type,
            Status = x.Status,
            FullName = x.FullName,
            Phone = x.Phone,
            Email = x.Email,
            Message = x.Message,
            PreferredDate = x.PreferredDate,
            InternalNote = x.InternalNote,
            VehicleId = x.VehicleId,
            VehicleListingNo = x.Vehicle != null ? x.Vehicle.ListingNo : null,
            VehicleTitle = x.Vehicle != null ? x.Vehicle.Title : null,
            SellBrand = x.SellBrand,
            SellModel = x.SellModel,
            SellPackage = x.SellPackage,
            SellYear = x.SellYear,
            SellKm = x.SellKm,
            SellExpectedPrice = x.SellExpectedPrice,
            SellDamage = x.SellDamage,
            Media = x.Media.OrderBy(m => m.SortOrder).Select(m => new LeadMediaModel { Id = m.Id }).ToList(),
            KvkkConsentAt = x.KvkkConsentAt,
            CreateDate = x.CreateDate
        };
    }
}
