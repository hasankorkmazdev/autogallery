using FatihTasci.Core.Entity.Crm;
using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Enums;
using FatihTasci.Core.Extensions.Data;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Lead;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace FatihTasci.Core.Service.Domains;

public class LeadService : DomainService, ILeadService
{
    private static readonly VehicleStatus[] VisibleStatuses = { VehicleStatus.Published, VehicleStatus.Reserved, VehicleStatus.Sold };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<LeadEntity> _leadRepository;
    private readonly IRepository<VehicleEntity> _vehicleRepository;
    private readonly ILogger<LeadService> _logger;

    public LeadService(IUnitOfWork unitOfWork, ILogger<LeadService> logger)
    {
        _unitOfWork = unitOfWork;
        _leadRepository = unitOfWork.GetRepository<LeadEntity>();
        _vehicleRepository = unitOfWork.GetRepository<VehicleEntity>();
        _logger = logger;
    }

    public async Task<ServiceResultModel> CreateAsync(CreateLeadRequestModel request, string ipAddress)
    {
        const string successMessage = "Talebiniz alındı. En kısa sürede sizinle iletişime geçeceğiz.";

        // Bot tuzağı doluysa kayıt yapılmaz ama bota başarılı görünür.
        if (!string.IsNullOrEmpty(request.Website))
        {
            _logger.LogInformation("Bot tuzağına takılan form isteği yok sayıldı. IP: {Ip}", ipAddress);
            return Result(message: successMessage);
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

        return Result(message: successMessage);
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
        return Result(model, "Talep güncellendi.");
    }

    public async Task<ServiceResultModel<int>> GetNewCountAsync()
    {
        return Result(await _leadRepository.CountAsync(x => x.Status == LeadStatus.New));
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
            KvkkConsentAt = x.KvkkConsentAt,
            CreateDate = x.CreateDate
        };
    }
}
