using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Vehicle;

// Kurallar panelin zod şemalarıyla (FatihTasci.Admin/src/pages/vehicles/wizard/schemas.ts) aynıdır.
// Adımlar arası kurallar (sıfır araçta km, yayın için fiyat) VehicleService'te kontrol edilir.

/// <summary>
/// Adım 6: Video bağlantısı
/// </summary>
public class VehicleVideoRequestModel : IRequestModel
{
    public string VideoUrl { get; set; }
}

public class VehicleVideoRequestModelValidator : AbstractValidator<VehicleVideoRequestModel>
{
    public VehicleVideoRequestModelValidator()
    {
        RuleFor(x => x.VideoUrl)
            .MaximumLength(300).WithMessage("Video bağlantısı en fazla 300 karakter olabilir.")
            .Must(BeHttpsUrl).When(x => !string.IsNullOrWhiteSpace(x.VideoUrl))
            .WithMessage("Geçerli bir https bağlantısı girin.");
    }

    private static bool BeHttpsUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
    }
}
