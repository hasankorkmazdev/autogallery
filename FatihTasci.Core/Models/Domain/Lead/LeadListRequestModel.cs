using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Lead;

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
