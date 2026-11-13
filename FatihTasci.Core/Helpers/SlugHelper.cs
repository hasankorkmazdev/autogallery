using System.Text;

namespace FatihTasci.Core.Helpers;

/// <summary>
/// Slug üretimi (PROJE_DOKUMANI §3.8). Slug hiçbir yerde elle girilmez.
/// </summary>
public static class SlugHelper
{
    /// <summary>
    /// 1) Türkçe karakterler dönüştürülür (ç→c, ğ→g, ı→i, İ→i, ö→o, ş→s, ü→u)
    /// 2) Tümü küçük harfe çevrilir
    /// 3) Harf ve rakam dışındaki her karakter tireye dönüşür; ardışık tireler teke iner,
    ///    baştaki ve sondaki tireler silinir.
    /// </summary>
    public static string Generate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var builder = new StringBuilder(value.Length);
        var lastWasDash = false;

        foreach (var ch in value)
        {
            var mapped = MapChar(ch);

            if (mapped != '\0')
            {
                builder.Append(mapped);
                lastWasDash = false;
            }
            else if (!lastWasDash && builder.Length > 0)
            {
                builder.Append('-');
                lastWasDash = true;
            }
        }

        return builder.ToString().Trim('-');
    }

    /// <summary>
    /// Slug kullanılıyorsa sonuna -2, -3 ... ekleyerek boşta olan ilk değeri döner.
    /// </summary>
    public static async Task<string> EnsureUniqueAsync(string baseSlug, Func<string, Task<bool>> isTaken)
    {
        var slug = string.IsNullOrEmpty(baseSlug) ? "kayit" : baseSlug;
        var candidate = slug;
        var counter = 2;

        while (await isTaken(candidate))
        {
            candidate = $"{slug}-{counter}";
            counter++;
        }

        return candidate;
    }

    /// <summary>
    /// Araç slug'ı: yıl + marka + model, örn. 2022-mercedes-benz-c-200-amg
    /// </summary>
    public static string ForVehicle(int? year, string brandName, string modelName)
    {
        var parts = new List<string>();
        if (year.HasValue)
            parts.Add(year.Value.ToString());
        parts.Add(brandName);
        parts.Add(modelName);
        return Generate(string.Join(" ", parts));
    }

    /// <summary>
    /// "/arac/{slug}-{ilanNo}" adresinin son parçasından ilan no'yu ayırır.
    /// </summary>
    public static bool TryParseVehiclePath(string slugAndNo, out string slug, out int listingNo)
    {
        slug = null;
        listingNo = 0;

        if (string.IsNullOrWhiteSpace(slugAndNo))
            return false;

        var index = slugAndNo.LastIndexOf('-');
        var numberPart = index >= 0 ? slugAndNo[(index + 1)..] : slugAndNo;

        if (!int.TryParse(numberPart, out listingNo))
            return false;

        slug = index >= 0 ? slugAndNo[..index] : string.Empty;
        return true;
    }

    private static char MapChar(char ch)
    {
        switch (ch)
        {
            case 'ç': case 'Ç': return 'c';
            case 'ğ': case 'Ğ': return 'g';
            case 'ı': case 'I': case 'İ': case 'i': return 'i';
            case 'ö': case 'Ö': return 'o';
            case 'ş': case 'Ş': return 's';
            case 'ü': case 'Ü': return 'u';
        }

        if (ch >= 'a' && ch <= 'z')
            return ch;
        if (ch >= 'A' && ch <= 'Z')
            return (char)(ch + 32);
        if (ch >= '0' && ch <= '9')
            return ch;

        return '\0';
    }
}
