using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Showroom;

public class ShowroomSceneOrderRequestModel : IRequestModel
{
    public List<int> SceneIds { get; set; } = new();
}

public class ShowroomSceneOrderRequestModelValidator : AbstractValidator<ShowroomSceneOrderRequestModel>
{
    public ShowroomSceneOrderRequestModelValidator()
    {
        RuleFor(x => x.SceneIds)
            .NotEmpty().WithMessage("Sıralanacak sahne yok.")
            .Must(x => x.Distinct().Count() == x.Count).WithMessage("Aynı sahne birden fazla kez gönderilemez.");
    }
}
