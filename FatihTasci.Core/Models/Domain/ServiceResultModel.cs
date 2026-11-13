using System.Text.Json.Serialization;

namespace FatihTasci.Core.Models.Domain;

public class ServiceResultModel
{
    [JsonPropertyOrder(0)]
    public string Provider { get; set; }

    [JsonPropertyOrder(1)]
    public string Environment { get; set; }

    [JsonPropertyOrder(2)]
    public string Version { get; set; }

    [JsonPropertyOrder(3)]
    public int Code { get; set; } = 200;

    [JsonPropertyOrder(99)]
    public string Message { get; set; } = "Ok";

    [JsonPropertyOrder(98)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object Exception { get; set; }
}
public class ServiceResultModel<T> : ServiceResultModel
{

    [ JsonPropertyOrder(4)]
    public T Data { get; set; }
}
