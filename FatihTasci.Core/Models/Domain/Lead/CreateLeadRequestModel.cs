using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Lead;

/// <summary>
/// Vitrindeki formlardan gelen talep (iletişim, araç sayfasından bilgi / test sürüşü).
/// </summary>
public class CreateLeadRequestModel : IRequestModel
{
    public LeadType Type { get; set; } = LeadType.Contact;
    public int? ListingNo { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Message { get; set; }
    public DateTime? PreferredDate { get; set; }
    public bool KvkkConsent { get; set; }

    /// <summary>
    /// Bot tuzağı: gizli alan, insanlar boş bırakır.
    /// </summary>
    public string Website { get; set; }
}

public class CreateLeadRequestModelValidator : AbstractValidator<CreateLeadRequestModel>
{
    public CreateLeadRequestModelValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Talep türü geçersiz.")
            .NotEqual(LeadType.SellOrTrade).WithMessage("Bu form ile araç satış talebi gönderilemez.");
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad zorunludur.")
            .MaximumLength(100).WithMessage("Ad soyad en fazla 100 karakter olabilir.");
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon zorunludur.")
            .Matches(@"^[0-9 +()\-]{10,20}$").WithMessage("Geçerli bir telefon numarası girin.");
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Geçerli bir e-posta adresi girin.")
            .MaximumLength(150).WithMessage("E-posta en fazla 150 karakter olabilir.");
        RuleFor(x => x.Message)
            .MaximumLength(2000).WithMessage("Mesaj en fazla 2000 karakter olabilir.");
        RuleFor(x => x.Message)
            .NotEmpty().When(x => x.Type == LeadType.Contact).WithMessage("Mesaj zorunludur.");
        RuleFor(x => x.PreferredDate)
            .Must(x => x.Value.Date >= DateTime.Today).When(x => x.PreferredDate.HasValue)
            .WithMessage("Tercih edilen tarih bugünden önce olamaz.");
        RuleFor(x => x.KvkkConsent)
            .Equal(true).WithMessage("Devam etmek için aydınlatma metnini onaylayın.");
    }
}
