using FatihTasci.Core.Models.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FatihTasci.Core.Middleware;

public class ServiceResultResponseProccesor : IMiddleware
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

    public ServiceResultResponseProccesor(
        IConfiguration configuration,
        ILogger<GlobalExceptionHandler> logger,
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
    {
        _configuration = configuration;
        _logger = logger;
        _env = env;
    }



    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var isApi = context.Request.Path.StartsWithSegments("/api");

        if (context.Items.ContainsKey("__SkipResponseProcessor"))
        {
            await next(context);
            return;
        }

        if (isApi)
        {
            var originalBodyStream = context.Response.Body;
            await using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await next(context);

            memStream.Position = 0;

            // Check if it's a file result or non-JSON
            var contentType = context.Response.ContentType ?? "";
            if (contentType.Contains("json") || string.IsNullOrEmpty(contentType))
            {
                string responseBody;
                using (var sr = new StreamReader(memStream))
                {
                    responseBody = await sr.ReadToEndAsync();
                }

                var generatedResponse = RunServiceResultProccesor(context, responseBody);

                context.Response.Body = originalBodyStream;
                context.Response.ContentLength = null;

                await using var generatedResponseStream = new MemoryStream();
                using (var sw = new StreamWriter(generatedResponseStream, leaveOpen: true))
                {
                    await sw.WriteAsync(generatedResponse);
                    await sw.FlushAsync();
                    generatedResponseStream.Position = 0;

                    await generatedResponseStream.CopyToAsync(originalBodyStream);
                    await originalBodyStream.FlushAsync();
                }
            }
            else
            {
                // It's a file or other binary, just copy the memStream back to original
                context.Response.Body = originalBodyStream;
                await memStream.CopyToAsync(originalBodyStream);
            }
        }
        else
        {
            await next(context);
        }
    }

    public string RunServiceResultProccesor(HttpContext context, string originalResponse)
    {
        var version = _configuration.GetSection("Application:Version").Value;
        var provider = _configuration.GetSection("Application:Name").Value;
        var environmentName = _env.EnvironmentName;
        ServiceResultModel<object> responseContent = null;

        var serializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        if (context.Response.ContentType != null && context.Response.ContentType.Contains("json") && !string.IsNullOrEmpty(originalResponse) && !context.Request.Path.Value.StartsWith("/src"))
        {
            try
            {
                using var doc = JsonDocument.Parse(originalResponse);

                // Check if it's already a ServiceResultModel by checking for 'code' property
                if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                    (doc.RootElement.TryGetProperty("code", out _) || doc.RootElement.TryGetProperty("Code", out _)))
                {
                    responseContent = JsonSerializer.Deserialize<ServiceResultModel<object>>(originalResponse, serializerOptions);
                }
                else
                {
                    // It's a raw object, an array, or a ValidationProblemDetails. Wrap it.
                    responseContent = new ServiceResultModel<object>
                    {
                        Code = context.Response.StatusCode,
                        Message = context.Response.StatusCode >= 400 ? "İşlem başarısız." : "İşlem başarılı.",
                        Data = JsonSerializer.Deserialize<object>(originalResponse, serializerOptions)
                    };
                }
            }
            catch
            {
                // Fallback for non-JSON or malformed JSON
                responseContent = new ServiceResultModel<object>
                {
                    Code = context.Response.StatusCode,
                    Message = context.Response.StatusCode >= 400 ? "İşlem başarısız." : "İşlem başarılı.",
                    Data = originalResponse
                };
            }
        }
        else
        {
            return originalResponse;
        }

        if (responseContent != null)
        {
            responseContent.Provider = provider;
            responseContent.Version = version;
            responseContent.Environment = environmentName;

            context.Response.StatusCode = responseContent.Code;
            context.Response.ContentType = "application/json; charset=utf-8";

            return JsonSerializer.Serialize(responseContent, serializerOptions);
        }

        return originalResponse;
    }
}





public static class ServiceResultResponseProccesorExtension
{
    public static IServiceCollection AddServiceResultResponseProccesor(this IServiceCollection services)
    {
        return services.AddScoped<ServiceResultResponseProccesor>();
    }
    public static IApplicationBuilder UseServiceResultResponseProccesor(this IApplicationBuilder builder)
    {

        return builder.UseMiddleware<ServiceResultResponseProccesor>();
    }
}

