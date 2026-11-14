using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Showroom;

public class SaveShowroomSceneRequestModel : IRequestModel
{
    public string Tag { get; set; }
    public string Title { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SaveShowroomSceneRequestModelValidator : AbstractValidator<SaveShowroomSceneRequestModel>
{
    public SaveShowroomSceneRequestModelValidator()
    {
        RuleFor(x => x.Tag)
            .NotEmpty().WithMessage("Etiket zorunludur.")
            .MaximumLength(40).WithMessage("Etiket en fazla 40 karakter olabilir.");
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık zorunludur.")
            .MaximumLength(80).WithMessage("Başlık en fazla 80 karakter olabilir.");
    }
}
