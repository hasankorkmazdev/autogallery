using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Account;

/// <summary>
/// Kullanıcı ekleme ve düzenleme. Şifre yalnız eklerken zorunludur; düzenlemede şifre ayrı uçtan sıfırlanır.
/// </summary>
public class SaveUserRequestModel : IRequestModel
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string EMail { get; set; }

    /// <summary>
    /// Sitede danışmanı olduğu ilanlarda görünen iletişim telefonu (isteğe bağlı)
    /// </summary>
    public string Phone { get; set; }

    public UserRole Role { get; set; } = UserRole.Staff;
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Yalnız yeni kullanıcıda kullanılır
    /// </summary>
    public string Password { get; set; }
}

public class SaveUserRequestModelValidator : AbstractValidator<SaveUserRequestModel>
{
    public SaveUserRequestModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ad zorunludur.")
            .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.");
        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage("Soyad zorunludur.")
            .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.");
        RuleFor(x => x.EMail)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin.")
            .MaximumLength(150).WithMessage("E-posta en fazla 150 karakter olabilir.");
        RuleFor(x => x.Phone)
            .Matches(@"^[0-9 +()\-]{10,20}$").When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Geçerli bir telefon numarası girin.");
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Geçerli bir rol seçin.");
        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Şifre en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Password));
    }
}
