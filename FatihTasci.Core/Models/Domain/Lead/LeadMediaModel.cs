namespace FatihTasci.Core.Models.Domain.Lead;

/// <summary>
/// Talep fotoğrafı. Url panelin oturumla eriştiği uçtur (/api/leads/{id}/media/{mediaId}); dosya sitede sunulmaz.
/// </summary>
public class LeadMediaModel
{
    public Guid Id { get; set; }
    public string Url { get; set; }
}
