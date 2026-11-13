using FatihTasci.Core.Enums;
using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Catalog;

// ---------------------------------------------------------------- Request

public class SaveBrandRequestModel : IRequestModel
{
    public string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
public class SaveBrandRequestModelValidator : AbstractValidator<SaveBrandRequestModel>
{
    public SaveBrandRequestModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Marka adı zorunludur.")
            .MaximumLength(60).WithMessage("Marka adı en fazla 60 karakter olabilir.");
        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sıra 0 ya da daha büyük olmalıdır.");
    }
}

public class SaveSeriesRequestModel : IRequestModel
{
    public int BrandId { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
public class SaveSeriesRequestModelValidator : AbstractValidator<SaveSeriesRequestModel>
{
    public SaveSeriesRequestModelValidator()
    {
        RuleFor(x => x.BrandId)
            .GreaterThan(0).WithMessage("Marka seçin.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Seri adı zorunludur.")
            .MaximumLength(80).WithMessage("Seri adı en fazla 80 karakter olabilir.");
        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sıra 0 ya da daha büyük olmalıdır.");
    }
}

public class SaveCarModelRequestModel : IRequestModel
{
    public int SeriesId { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; } = true;
}
public class SaveCarModelRequestModelValidator : AbstractValidator<SaveCarModelRequestModel>
{
    public SaveCarModelRequestModelValidator()
    {
        RuleFor(x => x.SeriesId)
            .GreaterThan(0).WithMessage("Seri seçin.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Model adı zorunludur.")
            .MaximumLength(120).WithMessage("Model adı en fazla 120 karakter olabilir.");
    }
}

public class SaveFeatureRequestModel : IRequestModel
{
    public FeatureCategory Category { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
public class SaveFeatureRequestModelValidator : AbstractValidator<SaveFeatureRequestModel>
{
    public SaveFeatureRequestModelValidator()
    {
        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Kategori seçin.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Donanım adı zorunludur.")
            .MaximumLength(80).WithMessage("Donanım adı en fazla 80 karakter olabilir.");
        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sıra 0 ya da daha büyük olmalıdır.");
    }
}

// ---------------------------------------------------------------- Response

public class BrandModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int SeriesCount { get; set; }
}

public class SeriesModel
{
    public int Id { get; set; }
    public int BrandId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int ModelCount { get; set; }
}

public class CarModelModel
{
    public int Id { get; set; }
    public int SeriesId { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public bool IsActive { get; set; }
}

public class ColorModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string HexCode { get; set; }
}

public class FeatureModel
{
    public int Id { get; set; }
    public FeatureCategory Category { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class FeatureGroupModel
{
    public FeatureCategory Category { get; set; }
    public string CategoryName { get; set; }
    public List<FeatureModel> Features { get; set; } = new();
}
