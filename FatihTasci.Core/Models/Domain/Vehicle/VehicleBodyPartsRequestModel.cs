using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Vehicle;

// Kurallar panelin zod şemalarıyla (FatihTasci.Admin/src/pages/vehicles/wizard/schemas.ts) aynıdır.
// Adımlar arası kurallar (sıfır araçta km, yayın için fiyat) VehicleService'te kontrol edilir.

/// <summary>
/// Adım 4: Boya / değişen. Gönderilmeyen parça Orijinal kabul edilir.
/// </summary>
public class VehicleBodyPartsRequestModel : IRequestModel
{
    public List<VehicleBodyPartItemModel> Parts { get; set; } = new();
}

public class VehicleBodyPartsRequestModelValidator : AbstractValidator<VehicleBodyPartsRequestModel>
{
    public VehicleBodyPartsRequestModelValidator()
    {
        RuleFor(x => x.Parts)
            .NotNull().WithMessage("Parça listesi gönderilmelidir.")
            .Must(x => x.Select(p => p.Part).Distinct().Count() == x.Count)
            .WithMessage("Aynı parça birden fazla kez gönderilemez.");
        RuleForEach(x => x.Parts).ChildRules(part =>
        {
            part.RuleFor(p => p.Part).IsInEnum().WithMessage("Parça geçersiz.");
            part.RuleFor(p => p.State).IsInEnum().WithMessage("Parça durumu geçersiz.");
            part.RuleFor(p => p.Note).MaximumLength(200).WithMessage("Not en fazla 200 karakter olabilir.");
        });
    }
}
