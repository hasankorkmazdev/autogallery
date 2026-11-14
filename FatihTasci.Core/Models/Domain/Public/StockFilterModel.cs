using FatihTasci.Core.Enums;

namespace FatihTasci.Core.Models.Domain.Public;

// Vitrin (Razor) sayfalarının modelleri. Değerler ekranda gösterilecek şekilde hazırlanır.

/// <summary>
/// /stok filtreleri. Marka ve seri adresten (slug), diğerleri query string'den gelir.
/// </summary>
public class StockFilterModel
{
    public string Brand { get; set; }
    public string Series { get; set; }
    public FuelType? Fuel { get; set; }
    public TransmissionType? Transmission { get; set; }
    public BodyType? Body { get; set; }
    public int? YearMin { get; set; }
    public int? YearMax { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public int? KmMax { get; set; }

    /// <summary>
    /// yeni (varsayılan), fiyat-artan, fiyat-azalan, km, yil
    /// </summary>
    public string Sort { get; set; }

    public int Page { get; set; } = 1;
}
