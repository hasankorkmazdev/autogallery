using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Vehicle;

// Kurallar panelin zod şemalarıyla (FatihTasci.Admin/src/pages/vehicles/wizard/schemas.ts) aynıdır.
// Adımlar arası kurallar (sıfır araçta km, yayın için fiyat) VehicleService'te kontrol edilir.

public class VehicleListRequestModel : PagedListRequestModel
{
    public VehicleStatus? Status { get; set; }
    public int? BrandId { get; set; }
    public string Search { get; set; }
}

public class VehicleListRequestModelValidator : AbstractValidator<VehicleListRequestModel>
{
    public VehicleListRequestModelValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0).WithMessage("Sayfa numarası geçersiz.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır.");
        RuleFor(x => x.Search).MaximumLength(100).WithMessage("Arama en fazla 100 karakter olabilir.");
    }
}
