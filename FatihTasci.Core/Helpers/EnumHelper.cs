using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FatihTasci.Core.Helpers;

public static class EnumHelper
{
    /// <summary>
    /// [Display(Name)] varsa onu, yoksa enum adını döner.
    /// </summary>
    public static string GetDisplayName(this Enum value)
    {
        if (value == null)
            return string.Empty;

        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var display = member?.GetCustomAttribute<DisplayAttribute>();
        return display?.GetName() ?? value.ToString();
    }

    public static string GetDisplayName<T>(T? value) where T : struct, Enum
    {
        return value.HasValue ? value.Value.GetDisplayName() : string.Empty;
    }

    public static List<EnumOption> GetOptions<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>()
            .Select(x => new EnumOption { Value = Convert.ToInt32(x), Label = x.GetDisplayName() })
            .ToList();
    }
}

public class EnumOption
{
    public int Value { get; set; }
    public string Label { get; set; }
}
