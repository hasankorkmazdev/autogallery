using FatihTasci.Core.Entity.Identity;
using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Account;
using FatihTasci.Core.Providers;
using FatihTasci.Core.RepositoryBase;
using FatihTasci.Core.Service.Domains.Abstracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FatihTasci.Core.Service.Domains;

/// <summary>
/// Panel kullanıcılarının yönetimi (yalnız Yönetici). Kullanıcı silinmez, pasife alınır.
/// En az bir aktif Yönetici her zaman kalır; kişi kendi rolünü ve aktifliğini değiştiremez.
/// </summary>
public class UserService : DomainService, IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IPasswordHasher<UserEntity> _passwordHasher;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IMemoryCache _cache;

    public UserService(
        IUnitOfWork unitOfWork,
        IPasswordHasher<UserEntity> passwordHasher,
        ICurrentUserProvider currentUserProvider,
        IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _userRepository = unitOfWork.GetRepository<UserEntity>();
        _passwordHasher = passwordHasher;
        _currentUserProvider = currentUserProvider;
        _cache = cache;
    }

    public async Task<ServiceResultModel<List<UserListItemModel>>> GetListAsync()
    {
        var users = await _userRepository.GetQuery()
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Surname)
            .Select(x => new UserListItemModel
            {
                Id = x.Id,
                Name = x.Name,
                Surname = x.Surname,
                EMail = x.EMail,
                Phone = x.Phone,
                Role = x.Role,
                IsActive = x.IsActive,
                LastLoginDate = x.LastLoginDate,
                CreateDate = x.CreateDate
            })
            .ToListAsync();

        return Result(users);
    }

    public async Task<ServiceResultModel<List<AdvisorModel>>> GetAdvisorsAsync()
    {
        var advisors = await _userRepository.GetQuery()
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Surname)
            .Select(x => new AdvisorModel
            {
                Id = x.Id,
                FullName = x.Name + " " + x.Surname,
                Phone = x.Phone,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return Result(advisors);
    }

    public async Task<ServiceResultModel<UserListItemModel>> CreateAsync(SaveUserRequestModel request)
    {
        if (string.IsNullOrEmpty(request.Password))
            return Result<UserListItemModel>(null, "Yeni kullanıcı için şifre zorunludur.", 400);

        var normalizedEmail = Normalize(request.EMail);
        if (await _userRepository.ExistsAsync(x => x.NormalizedEMail == normalizedEmail))
            return Result<UserListItemModel>(null, "Bu e-posta adresiyle kayıtlı bir kullanıcı var.", 400);

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Surname = request.Surname.Trim(),
            EMail = request.EMail.Trim(),
            NormalizedEMail = normalizedEmail,
            Phone = NormalizePhone(request.Phone),
            Role = request.Role,
            IsActive = request.IsActive
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _userRepository.Insert(user);
        await _unitOfWork.SaveChangesAsync();

        return Result(ToModel(user), "Kullanıcı eklendi.");
    }

    public async Task<ServiceResultModel<UserListItemModel>> UpdateAsync(Guid id, SaveUserRequestModel request)
    {
        var user = await _userRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == id, disableTracking: false);
        if (user == null)
            return Result<UserListItemModel>(null, "Kullanıcı bulunamadı.", 404);

        var normalizedEmail = Normalize(request.EMail);
        if (await _userRepository.ExistsAsync(x => x.NormalizedEMail == normalizedEmail && x.Id != id))
            return Result<UserListItemModel>(null, "Bu e-posta adresiyle kayıtlı bir kullanıcı var.", 400);

        var isSelf = _currentUserProvider.GetUserId() == id;
        if (isSelf && (request.Role != user.Role || !request.IsActive))
            return Result<UserListItemModel>(null, "Kendi rolünüzü değiştiremez ya da kendinizi pasife alamazsınız.", 400);

        var losesAdmin = user.Role == UserRole.Admin && user.IsActive && (request.Role != UserRole.Admin || !request.IsActive);
        if (losesAdmin && !await _userRepository.ExistsAsync(x => x.Id != id && x.Role == UserRole.Admin && x.IsActive))
            return Result<UserListItemModel>(null, "En az bir aktif yönetici kalmalıdır.", 400);

        user.Name = request.Name.Trim();
        user.Surname = request.Surname.Trim();
        user.EMail = request.EMail.Trim();
        user.NormalizedEMail = normalizedEmail;
        user.Phone = NormalizePhone(request.Phone);
        user.Role = request.Role;
        user.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync();

        // Rol ya da aktiflik değiştiyse açık oturumlar bir sonraki istekte yeniden doğrulanır.
        _cache.Remove(AccountService.SessionCacheKey(id));

        return Result(ToModel(user), "Kullanıcı güncellendi.");
    }

    public async Task<ServiceResultModel> ResetPasswordAsync(Guid id, ResetUserPasswordRequestModel request)
    {
        var user = await _userRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == id, disableTracking: false);
        if (user == null)
            return Result("Kullanıcı bulunamadı.", 404);

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        await _unitOfWork.SaveChangesAsync();

        return Result(message: "Şifre değiştirildi.");
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();

    private static string NormalizePhone(string phone) => string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();

    private static UserListItemModel ToModel(UserEntity x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Surname = x.Surname,
        EMail = x.EMail,
        Phone = x.Phone,
        Role = x.Role,
        IsActive = x.IsActive,
        LastLoginDate = x.LastLoginDate,
        CreateDate = x.CreateDate
    };
}
