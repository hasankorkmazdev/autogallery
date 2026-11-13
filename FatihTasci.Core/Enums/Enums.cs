using System.ComponentModel.DataAnnotations;

namespace FatihTasci.Core.Enums;

// Tüm enum'ların ekranda görünen adları [Display(Name)] ile verilir.
// Panel bu adları GET /api/lookups'tan, vitrin EnumHelper.GetDisplayName ile alır.

public enum UserRole
{
    [Display(Name = "Yönetici")]
    Admin = 1,

    [Display(Name = "Personel")]
    Staff = 2
}

public enum VehicleStatus
{
    [Display(Name = "Taslak")]
    Draft = 0,

    [Display(Name = "Yayında")]
    Published = 1,

    [Display(Name = "Rezerve")]
    Reserved = 2,

    [Display(Name = "Satıldı")]
    Sold = 3,

    [Display(Name = "Arşiv")]
    Archived = 4
}

public enum FuelType
{
    [Display(Name = "Benzin")]
    Gasoline = 1,

    [Display(Name = "Dizel")]
    Diesel = 2,

    [Display(Name = "LPG & Benzin")]
    LpgGasoline = 3,

    [Display(Name = "Hibrit")]
    Hybrid = 4,

    [Display(Name = "Plug-in Hibrit")]
    PlugInHybrid = 5,

    [Display(Name = "Elektrik")]
    Electric = 6
}

public enum TransmissionType
{
    [Display(Name = "Manuel")]
    Manual = 1,

    [Display(Name = "Otomatik")]
    Automatic = 2,

    [Display(Name = "Yarı Otomatik")]
    SemiAutomatic = 3
}

public enum BodyType
{
    [Display(Name = "Sedan")]
    Sedan = 1,

    [Display(Name = "Hatchback 3 kapı")]
    Hatchback3 = 2,

    [Display(Name = "Hatchback 5 kapı")]
    Hatchback5 = 3,

    [Display(Name = "Station Wagon")]
    StationWagon = 4,

    [Display(Name = "SUV")]
    Suv = 5,

    [Display(Name = "Crossover")]
    Crossover = 6,

    [Display(Name = "Coupe")]
    Coupe = 7,

    [Display(Name = "Cabrio")]
    Cabrio = 8,

    [Display(Name = "Roadster")]
    Roadster = 9,

    [Display(Name = "MPV")]
    Mpv = 10,

    [Display(Name = "Pick-up")]
    Pickup = 11
}

public enum Drivetrain
{
    [Display(Name = "Önden Çekiş")]
    FrontWheel = 1,

    [Display(Name = "Arkadan İtiş")]
    RearWheel = 2,

    [Display(Name = "4WD (Sürekli)")]
    FourWheel = 3,

    [Display(Name = "AWD (Elektronik)")]
    AllWheel = 4
}

public enum VehicleCondition
{
    [Display(Name = "İkinci El")]
    Used = 1,

    [Display(Name = "Sıfır")]
    New = 2
}

public enum PlateOrigin
{
    [Display(Name = "TR Plakalı")]
    Turkish = 1,

    [Display(Name = "Yabancı Plakalı")]
    Foreign = 2
}

/// <summary>
/// Boya / değişen şemasındaki 13 parça. Sayısal değerler panel ve vitrindeki SVG şemasıyla eşleşir.
/// </summary>
public enum BodyPart
{
    [Display(Name = "Ön Tampon")]
    FrontBumper = 1,

    [Display(Name = "Motor Kaputu")]
    Hood = 2,

    [Display(Name = "Tavan")]
    Roof = 3,

    [Display(Name = "Bagaj Kapağı")]
    TrunkLid = 4,

    [Display(Name = "Arka Tampon")]
    RearBumper = 5,

    [Display(Name = "Sol Ön Çamurluk")]
    LeftFrontFender = 6,

    [Display(Name = "Sol Ön Kapı")]
    LeftFrontDoor = 7,

    [Display(Name = "Sol Arka Kapı")]
    LeftRearDoor = 8,

    [Display(Name = "Sol Arka Çamurluk")]
    LeftRearFender = 9,

    [Display(Name = "Sağ Ön Çamurluk")]
    RightFrontFender = 10,

    [Display(Name = "Sağ Ön Kapı")]
    RightFrontDoor = 11,

    [Display(Name = "Sağ Arka Kapı")]
    RightRearDoor = 12,

    [Display(Name = "Sağ Arka Çamurluk")]
    RightRearFender = 13
}

public enum BodyPartState
{
    [Display(Name = "Orijinal")]
    Original = 0,

    [Display(Name = "Lokal Boyalı")]
    LocalPainted = 1,

    [Display(Name = "Boyalı")]
    Painted = 2,

    [Display(Name = "Değişen")]
    Replaced = 3
}

public enum FeatureCategory
{
    [Display(Name = "Güvenlik")]
    Safety = 1,

    [Display(Name = "İç Donanım")]
    Interior = 2,

    [Display(Name = "Dış Donanım")]
    Exterior = 3,

    [Display(Name = "Multimedya")]
    Multimedia = 4
}

public enum LeadType
{
    [Display(Name = "Bilgi")]
    Information = 1,

    [Display(Name = "Test Sürüşü")]
    TestDrive = 2,

    [Display(Name = "Aracını Sat / Takas")]
    SellOrTrade = 3,

    [Display(Name = "Genel İletişim")]
    Contact = 4
}

public enum LeadStatus
{
    [Display(Name = "Yeni")]
    New = 0,

    [Display(Name = "Arandı")]
    Called = 1,

    [Display(Name = "Teklif Verildi")]
    OfferMade = 2,

    [Display(Name = "Kapandı")]
    Closed = 3
}

public enum SlugEntityType
{
    Brand = 1,
    Series = 2,
    CarModel = 3
}
