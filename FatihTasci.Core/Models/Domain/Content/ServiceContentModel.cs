using FatihTasci.Core.Enums;
using FatihTasci.Core.Helpers;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Content;

/// <summary>
/// Bir hizmetin vitrin yazıları. Başlık ve adres hizmet türünden gelir, panelde değişmez.
/// </summary>
public class ServiceContentModel
{
    public const int MaxSummaryLength = 300;
    public const int MaxBodyLength = 3000;

    public SiteServiceType Type { get; set; }

    /// <summary>
    /// Ekrandaki ad ([Display(Name)]). Salt okunur.
    /// </summary>
    public string Title => Type.GetDisplayName();

    /// <summary>
    /// Sayfa adresi: /hizmetler/{slug}, slug başlıktan SlugHelper ile üretilir. Salt okunur.
    /// </summary>
    public string Path => $"/hizmetler/{SlugHelper.Generate(Title)}";

    /// <summary>
    /// Kartlarda ve hizmet sayfasının girişinde görünen kısa yazı.
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// Hizmet sayfasının metni. Boş satırla ayrılan bloklar paragraf olur.
    /// </summary>
    public string Body { get; set; }
}
public class ServiceContentModelValidator : AbstractValidator<ServiceContentModel>
{
    public ServiceContentModelValidator()
    {
        RuleFor(x => x.Type).IsInEnum().WithMessage("Hizmet türü geçersiz.");
        RuleFor(x => x.Summary)
            .NotEmpty().WithMessage("Kısa yazı zorunludur.")
            .MaximumLength(ServiceContentModel.MaxSummaryLength)
            .WithMessage($"Kısa yazı en fazla {ServiceContentModel.MaxSummaryLength} karakter olabilir.");
        RuleFor(x => x.Body)
            .MaximumLength(ServiceContentModel.MaxBodyLength)
            .WithMessage($"Sayfa metni en fazla {ServiceContentModel.MaxBodyLength} karakter olabilir.");
    }
}
