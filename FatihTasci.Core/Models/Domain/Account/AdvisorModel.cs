namespace FatihTasci.Core.Models.Domain.Account;

/// <summary>
/// Araca danışman olarak atanabilecek panel kullanıcısı (sihirbazın Fiyat adımındaki seçim kutusu).
/// Pasif kullanıcılar da gelir: araca önceden atanmış pasif danışmanın adı gösterilebilsin diye.
/// </summary>
public class AdvisorModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public bool IsActive { get; set; }
}
