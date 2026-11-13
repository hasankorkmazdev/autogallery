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

public class LeadListRequestModel : PagedListRequestModel
{
    public LeadStatus? Status { get; set; }
    public LeadType? Type { get; set; }
    public string Search { get; set; }
}
public class LeadListRequestModelValidator : AbstractValidator<LeadListRequestModel>
{
    public LeadListRequestModelValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0).WithMessage("Sayfa numarası geçersiz.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır.");
        RuleFor(x => x.Search).MaximumLength(100).WithMessage("Arama en fazla 100 karakter olabilir.");
    }
}

public class UpdateLeadRequestModel : IRequestModel
{
    public LeadStatus Status { get; set; }
    public string InternalNote { get; set; }
}
public class UpdateLeadRequestModelValidator : AbstractValidator<UpdateLeadRequestModel>
{
    public UpdateLeadRequestModelValidator()
    {
        RuleFor(x => x.Status).IsInEnum().WithMessage("Durum geçersiz.");
        RuleFor(x => x.InternalNote).MaximumLength(2000).WithMessage("Not en fazla 2000 karakter olabilir.");
    }
}

public class LeadModel
{
    public Guid Id { get; set; }
    public LeadType Type { get; set; }
    public LeadStatus Status { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Message { get; set; }
    public DateTime? PreferredDate { get; set; }
    public string InternalNote { get; set; }
    public Guid? VehicleId { get; set; }
    public int? VehicleListingNo { get; set; }
    public string VehicleTitle { get; set; }
    public DateTime KvkkConsentAt { get; set; }
    public DateTime CreateDate { get; set; }
}
