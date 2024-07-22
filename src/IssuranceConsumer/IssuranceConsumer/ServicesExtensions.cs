using System.Threading.Channels;
using Hangfire;
using IssuranceConsumer.Infrastructure.DbBackgroundServices;
using IssuranceConsumer.Infrastructure.Models;
using IssuranceConsumer.Infrastructure.PrizeProcessor.Impl;
using IssuranceConsumer.Infrastructure.PrizeProcessor.Interfaces;
using IssuranceConsumer.Model;
using IssuranceConsumer.Model.Model.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Timeout;

namespace IssuranceConsumer;

public static class ServicesExtensions
{
    public static IServiceCollection AddChannels(this IServiceCollection services)
    {
        // TODO inject as keyed services , since it so misunderstanding after a period of time
        services.AddSingleton(Channel.CreateUnbounded<PrizeConfig>(new UnboundedChannelOptions
                                                                       { SingleWriter = true, SingleReader = false }));
        services.AddSingleton(Channel.CreateUnbounded<List<IssueModel>>(new UnboundedChannelOptions
        {
            SingleWriter = false, SingleReader = false
        }));
        for (var i = 0; i < 3; i++) // should be in IConfigure appsettings
            services
               .AddSingleton<IHostedService,
                    BatchingMemberBackgroundServices>(); // dont use addHostServices here , will bomb with lower .net version
        for (var i = 0; i < 100; i++) // should be in IConfigure appsettings
            services
               .AddSingleton<IHostedService,
                    RequestProducerBackgroundServices>(); // dont use addHostServices here , will bomb with lower .net version

        services.AddSingleton<IPrizeConfigProvider, PrizeConfigProvider>();
        services.AddScoped<IPrizeProcessor, GeneralBatchProcessor>();

        return services;
    }

    public static IServiceCollection AddHangFireServices(this IServiceCollection services, string connectionStr)
    {
        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseColouredConsoleLogProvider()
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UseSqlServerStorage(connectionStr);
        });
        services.AddHangfireServer();
        return services;
    }

    public static IServiceCollection ConfigHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient("productEndpoints", client =>
                 {
                     client.DefaultRequestHeaders.Add("Keep-Alive", "false");
#if DEBUG
                     client.BaseAddress = new Uri("https://localhost:7132/");
#else
                    client.BaseAddress = new Uri("https://10.0.10.100/");
#endif
                 })
                .AddStandardResilienceHandler(options =>
                 {
                     // Customize retry
                     options.Retry.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                                 .Handle<TimeoutRejectedException>()
                                                 .Handle<HttpRequestException>();
                     //       .HandleResult(response => response.IsSuccessStatusCode == false);
                     options.Retry.MaxRetryAttempts = 5;
                 });
        return services;
    }

    public static IServiceCollection ConfigDb(this IServiceCollection services, string connectionStr)
    {
        services.AddPooledDbContextFactory<InsurancesContext>(options =>
                 {
                     options.UseSqlServer(connectionStr,
                                          serviceOptions =>
                                              serviceOptions
                                                 .EnableRetryOnFailure(10));
                 })
                .AddScoped(p => p
                               .GetRequiredService<
                                    IDbContextFactory<InsurancesContext>>()
                               .CreateDbContext()); //add a default scoped in case ppl get it wrong
        return services;
    }
}