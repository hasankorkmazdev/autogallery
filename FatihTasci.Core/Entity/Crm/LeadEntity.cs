using FatihTasci.Core.Entity.Base;
using FatihTasci.Core.Entity.Inventory;
using FatihTasci.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace FatihTasci.Core.Entity.Crm;

/// <summary>
/// Vitrin formlarından gelen talep (bilgi, test sürüşü, aracını sat, iletişim).
/// </summary>
[Table(name: "Leads", Schema = "Crm")]
public class LeadEntity : BaseAuditableEntity<Guid>
{
    public LeadType Type { get; set; }

    /// <summary>
    /// Talep bir araç sayfasından geldiyse
    /// </summary>
    public Guid? VehicleId { get; set; }

    public string FullName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Message { get; set; }

    /// <summary>
    /// Test sürüşünde tercih edilen tarih
    /// </summary>
    public DateTime? PreferredDate { get; set; }

    // --- Yalnızca "Aracını Sat / Takas" talebinde (/hizmetler/aracimi-satmak-istiyorum teklif formu).
    // Ekrandaki adlarıyla Marka / Model / Paket; ziyaretçi serbest yazar, katalogla eşlenmez.
    public string SellBrand { get; set; }
    public string SellModel { get; set; }
    public string SellPackage { get; set; }
    public int? SellYear { get; set; }
    public int? SellKm { get; set; }
    public decimal? SellExpectedPrice { get; set; }
    public SellDamageState? SellDamage { get; set; }

    /// <summary>
    /// KVKK açık rızasının verildiği zaman
    /// </summary>
    public DateTime KvkkConsentAt { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.New;
    public string InternalNote { get; set; }

    /// <summary>
    /// Formu gönderen IP (kötüye kullanım takibi için)
    /// </summary>
    public string IpAddress { get; set; }

    public virtual VehicleEntity Vehicle { get; set; }
    public virtual ICollection<LeadMediaEntity> Media { get; set; } = new List<LeadMediaEntity>();
}
