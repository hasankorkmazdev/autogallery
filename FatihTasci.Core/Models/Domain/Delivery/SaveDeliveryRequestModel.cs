using FatihTasci.Core.Models.Domain.Common;
using FluentValidation;

namespace FatihTasci.Core.Models.Domain.Delivery;

/// <summary>
/// Teslimat ekleme / düzenleme (multipart: alanlar + "photo", isteğe bağlı "video" + "videoPoster"). Fotoğraf eklemede
/// zorunlu, düzenlemede yalnız değişecekse gönderilir; video da yalnız eklenecek / değişecekse gönderilir.
/// </summary>
public class SaveDeliveryRequestModel : IRequestModel
{
    public string VehicleTitle { get; set; }
    public string CustomerName { get; set; }
    public string City { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Düzenlemede mevcut videoyu kaldırır (yeni video gönderilmediyse).
    /// </summary>
    public bool RemoveVideo { get; set; }
}

public class SaveDeliveryRequestModelValidator : AbstractValidator<SaveDeliveryRequestModel>
{
    public SaveDeliveryRequestModelValidator()
    {
        RuleFor(x => x.VehicleTitle)
            .NotEmpty().WithMessage("Araç adı zorunludur.")
            .MaximumLength(80).WithMessage("Araç adı en fazla 80 karakter olabilir.");
        RuleFor(x => x.CustomerName)
            .MaximumLength(60).WithMessage("Müşteri adı en fazla 60 karakter olabilir.");
        RuleFor(x => x.City)
            .MaximumLength(40).WithMessage("Şehir en fazla 40 karakter olabilir.");
        RuleFor(x => x.DeliveredAt)
            .NotNull().WithMessage("Teslim tarihi zorunludur.")
            .Must(x => x == null || x.Value.Date <= DateTime.Today).WithMessage("Teslim tarihi bugünden sonra olamaz.");
    }
}
