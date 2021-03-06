﻿using System;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NeoMonitor;
using NeoMonitor.Abstractions.Caches;
using NeoMonitor.Abstractions.Services;
using NeoMonitor.Caches;
using NeoMonitor.Configs;
using NeoMonitor.DbContexts;
using NeoMonitor.Profiles;
using NeoMonitor.Services;
using NeoMonitor.Services.Data;
using NeoMonitor.Services.Seeds;
using NeoMonitor.Shared.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static INeoMonitorModuleBuilder AddBasicServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddCors(options =>
            {
                options.AddPolicy("DEV",
                    builder =>
                    {
                        builder
                            .WithOrigins("http://localhost:8111", "http://localhost:4200", "http://localhost:9876")
                            .AllowCredentials()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });
            services.AddControllers();
            services.AddSignalR()
                .AddMessagePackProtocol();
            return new NeoMonitorModuleBuilder(services);
        }

        public static INeoMonitorModuleBuilder AddThirdPartyServices(this INeoMonitorModuleBuilder builder)
        {
            var services = builder.Services;
            services.AddAutoMapper(typeof(AutoMapperProfile));
            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "NeoMonitor APIs";
                    document.Info.Description = "APIs of NeoMonitor-Server";
                    document.Info.TermsOfService = "None";
                    document.Info.Contact = new NSwag.OpenApiContact
                    {
                        Name = "Github Repository",
                        Email = string.Empty,
                        Url = "https://github.com/alienworks/NeoMonitor-Server"
                    };
                    //document.Info.License = new NSwag.OpenApiLicense
                    //{
                    //};
                };
            });
            return builder;
        }

        public static INeoMonitorModuleBuilder AddInternalServices(this INeoMonitorModuleBuilder builder, IConfiguration configuration)
        {
            var services = builder.Services;

            services
                .AddInternalOptions(configuration)
                .AddInternalDbContexts(configuration)
                .AddInternalCaches();

            services
                .AddNeoRpcHttpClient(c => c.ApiVersion = new Version(2, 0))
                .AddNeoJsonRpcAPIs();

            services
                .AddSingleton<ScopedDbContextFactory>()
                .AddSingleton<NodeSynchronizer>();

            services
                .AddTransient<INodeSeedsLoaderFactory, NodeSeedsLoaderFactory>()
                .AddTransient<IStartupFilter, NodeSeedsStartupFilter>();

            services.AddInternalHostedServices();

            return builder;
        }

        public static INeoMonitorModuleBuilder AddOtherModules(this INeoMonitorModuleBuilder builder, IConfiguration config)
        {
            var services = builder.Services;
            services
                .AddNeoCommonModule(config.GetSection("CommonModuleSettings"))
                .AddNeoAnalysisWebModule(dbContextOptionsAction: options =>
                {
                    options.UseMySql(config.GetConnectionString("AnalysisDevConnection"));
                });
            return builder;
        }

        private static IServiceCollection AddInternalCaches(this IServiceCollection services)
        {
            return services
                .AddSingleton<INodeDataCache, NodeDataMemoryCache>()
                .AddSingleton<IRawMemPoolDataCache, RawMemPoolDataMemoryCache>()
                .AddSingleton<IMatrixDataCache, MatrixDateMemoryCache>();
        }

        private static IServiceCollection AddInternalOptions(this IServiceCollection services, IConfiguration config)
        {
            return services.Configure<NodeSyncSettings>(config.GetSection(nameof(NodeSyncSettings)));
        }

        private static IServiceCollection AddInternalDbContexts(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<NeoMonitorContext>(options =>
            {
                options.UseMySql(config.GetConnectionString("DefaultConnection"));
            }, ServiceLifetime.Scoped, ServiceLifetime.Scoped);
            services.AddDbContext<NeoMatrixDbContext>(options =>
            {
                options.UseMySql(config.GetConnectionString("MatrixConnection"));
            }, ServiceLifetime.Scoped, ServiceLifetime.Scoped);
            return services;
        }

        private static IServiceCollection AddInternalHostedServices(this IServiceCollection services)
        {
            return services
                .AddHostedService<NodeSyncHostService>()
                .AddHostedService<RawMemPoolBroadcastHostService>()
                .AddHostedService<MatrixSyncHostService>();
        }
    }
}