using ClaimsModule.Application.Interfaces;
using ClaimsModule.Infrastructure.Jobs;
using ClaimsModule.Infrastructure.Services;
using ClaimsModule.Infrastructure.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IHangfireEnqueuer, HangfireEnqueuer>();

        // Storage provider selection
        var provider = configuration["StorageProvider"] ?? "LocalFileSystem";
        if (provider == "AzureBlob")
            services.AddScoped<IStorageService, AzureBlobStorageService>();
        else
            services.AddScoped<IStorageService, LocalFileSystemStorageService>();

        // Register Hangfire jobs as scoped so they get DI
        services.AddScoped<PostGLReserveChangeJob>();
        services.AddScoped<SlaMonitoringJob>();

        return services;
    }
}
