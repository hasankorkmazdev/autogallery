using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Lead;

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
