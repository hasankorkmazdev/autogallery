namespace FatihTasci.Core.Models.Domain.Common;

public class PagedListRequestModel : IRequestModel
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 20;
}
