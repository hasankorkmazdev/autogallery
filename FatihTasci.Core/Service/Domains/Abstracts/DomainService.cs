using FatihTasci.Core.Models.Domain;

namespace FatihTasci.Core.Service.Domains.Abstracts;

public class DomainService : IDomainService
{
    protected ServiceResultModel<T> Result<T>(T data)
    {
        return new ServiceResultModel<T>()
        {
            Data = data,
            Code = 200,
            Message = "İşlem başarılı."
        };
    }
    protected ServiceResultModel Result(string message = "", int code = 200)
    {
        return new ServiceResultModel()
        {
            Code = code,
            Message = message
        };
    }
    protected ServiceResultModel<T> Result<T>(T data, string message = "", int code = 200)
    {
        return new ServiceResultModel<T>()
        {
            Data = data,
            Code = code,
            Message = message
        };
    }
}
