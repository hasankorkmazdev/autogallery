using FatihTasci.Core.Models.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FatihTasci.Core.Middleware;

public class GlobalExceptionHandler : IMiddleware
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandler(
        IConfiguration configuration,
        ILogger<GlobalExceptionHandler> logger,
        IWebHostEnvironment env)
    {
        _configuration = configuration;
        _logger = logger;
        _env = env;
    }



    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (_env.IsDevelopment())
                throw;
            else
                await HandleExceptionAsync(context, ex);
        }
    }

    public Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var version = _configuration.GetSection("Application:Version").Value;
        var provider = _configuration.GetSection("Application:Name").Value;
        var environmentName = _env.EnvironmentName;
        _logger.LogCritical(
            "Something went wrong: {@Exception}, InnerException: {@InnerException}",
            exception,
            exception.InnerException);
        var isApi = context.Request.Path.StartsWithSegments("/api");
        if (isApi)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var exposeExceptionDetails = _configuration.GetValue<bool>("Diagnostics:ExposeExceptionDetails");

            return context.Response.WriteAsync(JsonSerializer.Serialize(new ServiceResultModel()
            {
                Code = context.Response.StatusCode,
                Message = "Beklenmeyen bir hata oluştu.",
                Version = version,
                Provider = provider,
                Environment = environmentName,
                Exception = exposeExceptionDetails ? BuildExceptionDetails(exception) : null
            }));

        }
        return Task.CompletedTask;
    }

    private static object BuildExceptionDetails(Exception exception)
    {
        if (exception == null) return null;

        return new
        {
            Type = exception.GetType().FullName,
            exception.Message,
            exception.StackTrace,
            InnerException = BuildExceptionDetails(exception.InnerException)
        };
    }
}

public static class GlobalExceptionHandlerExtension
{
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        return services.AddScoped<GlobalExceptionHandler>();
    }
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {

        return builder.UseMiddleware<GlobalExceptionHandler>();
    }
}
