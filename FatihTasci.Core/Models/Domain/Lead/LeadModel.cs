using FatihTasci.Core.Enums;

namespace FatihTasci.Core.Models.Domain.Lead;

public class LeadModel
{
    public Guid Id { get; set; }
    public LeadType Type { get; set; }
    public LeadStatus Status { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Message { get; set; }
    public DateTime? PreferredDate { get; set; }
    public string InternalNote { get; set; }
    public Guid? VehicleId { get; set; }
    public int? VehicleListingNo { get; set; }
    public string VehicleTitle { get; set; }

    // --- Yalnızca "Aracını Sat / Takas" talebinde
    public string SellBrand { get; set; }
    public string SellModel { get; set; }
    public string SellPackage { get; set; }
    public int? SellYear { get; set; }
    public int? SellKm { get; set; }
    public decimal? SellExpectedPrice { get; set; }
    public SellDamageState? SellDamage { get; set; }
    public List<LeadMediaModel> Media { get; set; } = new();

    public DateTime KvkkConsentAt { get; set; }
    public DateTime CreateDate { get; set; }
}
