namespace FatihTasci.Core.Models.Domain.Common;

/// <summary>
/// Validator'ların bulunduğu assembly'yi işaretler (AddValidatorsFromAssemblyContaining&lt;IRequestModel&gt;).
/// </summary>
public interface IRequestModel
{
}

public class IdNameModel
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class PagedListRequestModel : IRequestModel
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 20;
}
