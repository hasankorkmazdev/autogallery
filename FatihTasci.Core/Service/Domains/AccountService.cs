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
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FatihTasci.Core.Service.Domains;

public class AccountService : DomainService, IAccountService
{
    private const string InvalidCredentialsMessage = "E-posta ya da şifre hatalı.";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher<UserEntity> _passwordHasher;
    private readonly IMemoryCache _cache;

    public AccountService(IUnitOfWork unitOfWork, IConfiguration configuration, IPasswordHasher<UserEntity> passwordHasher, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _userRepository = unitOfWork.GetRepository<UserEntity>();
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _cache = cache;
    }

    public async Task<ServiceResultModel<LoginResponseModel>> LoginAsync(LoginRequestModel request)
    {
        var normalizedEmail = request.EMail.Trim().ToLowerInvariant();
        var user = await _userRepository.GetFirstOrDefaultAsync(
            predicate: x => x.NormalizedEMail == normalizedEmail && x.IsActive,
            disableTracking: false);

        if (user == null)
            return Result<LoginResponseModel>(null, InvalidCredentialsMessage, 400);

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
            return Result<LoginResponseModel>(null, InvalidCredentialsMessage, 400);

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        user.LastLoginDate = DateTime.Now;
        await _unitOfWork.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue("JwtSettings:ExpiryInMinutes", 720));
        var response = new LoginResponseModel
        {
            User = ToModel(user),
            Token = GenerateJwtToken(user, expiresAt),
            ExpiresAt = expiresAt
        };

        return Result(response, "Giriş başarılı.");
    }

    public async Task<ServiceResultModel<UserModel>> GetUserAsync(Guid userId)
    {
        var user = await _userRepository.GetFirstOrDefaultAsync(predicate: x => x.Id == userId && x.IsActive);
        if (user == null)
            return Result<UserModel>(null, "Kullanıcı bulunamadı.", 404);

        return Result(ToModel(user));
    }

    public static string SessionCacheKey(Guid userId) => $"user-session:{userId:N}";

    public async Task<bool> IsSessionValidAsync(Guid userId, string role)
    {
        // Her istekte veritabanına gitmemek için kısa süre önbelleğe alınır; UserService güncellemede temizler.
        var current = await _cache.GetOrCreateAsync(SessionCacheKey(userId), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return await _userRepository.GetQuery()
                .Where(x => x.Id == userId && x.IsActive)
                .Select(x => (UserRole?)x.Role)
                .FirstOrDefaultAsync();
        });

        return current.HasValue && current.Value.ToString() == role;
    }

    private static UserModel ToModel(UserEntity user)
    {
        return new UserModel
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            EMail = user.EMail,
            Role = user.Role
        };
    }

    private string GenerateJwtToken(UserEntity user, DateTime expiresAt)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(HttpContextCurrentUserProvider.UserIdClaim, user.Id.ToString()),
            new(ClaimTypes.Email, user.EMail),
            new(ClaimTypes.Name, $"{user.Name} {user.Surname}"),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
