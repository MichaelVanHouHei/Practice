using System.Net;
using Hangfire;
using IssuranceConsumer;
using IssuranceConsumer.Infrastructure.DbBackgroundServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Providers;

#if DEBUG
Console.WriteLine("The services is running in DEBUG which is slow in performance, Are you sure to continue?[y/yes]");
var isRunDebug = Console.ReadLine().ToLower() is  "yes" or "y";
if (!isRunDebug) return;
#endif
Console.WriteLine("We will disable logging to console STThread in release mode");
ServicePointManager.Expect100Continue      = false;
ServicePointManager.DefaultConnectionLimit = int.MaxValue;
var builder = Host.CreateDefaultBuilder();

builder.ConfigureServices((hostContext, services) =>
        {
            var connectionStr = hostContext.Configuration.GetConnectionString("IssuranceDB");
            if (string.IsNullOrEmpty(connectionStr)) return;

            #region using redis is faster for hangfire

            var hangfireConnectionStr = hostContext.Configuration.GetConnectionString("Hangfire");
            services.AddHangFireServices(hangfireConnectionStr)
                    .ConfigDb(connectionStr)
                    .AddSingleton(TimeProvider.System)
                    .AddTransient<PrizeBackroundServices>() // make this to transient for hangfire use
                    .ConfigHttpClients()
                    .AddChannels();

            #endregion

            // services.AddHostedService<PrizeBackroundServices>();
            //services.AddSingleton<IFlurlClientCache>(sp => new FlurlClientCache()
            //                                            .Add("produceEndpoint", "https://10.0.10.100/"));
        })
       .ConfigureLogging(logging =>
        {
            //TODO i am lazy for logging here , we should use LoggerFactory to separate different "services" log
            logging.ClearProviders().AddZLoggerRollingFile(options =>
            {
                // File name determined by parameters to be rotated
                options.FilePathSelector = (timestamp, sequenceNumber) =>
                    $"logs/{timestamp.ToLocalTime():yyyy-MM-dd}_{sequenceNumber:000}.log";

                // The period of time for which you want to rotate files at time intervals.
                options.RollingInterval = RollingInterval.Day;

                // Limit of size if you want to rotate by file size. (KB)
                // options.RollingSizeKB = 1024;
            });
#if DEBUG
            logging.AddZLoggerConsole(options =>
            {
                //  options.UseJsonFormatter();
            });
#endif
        });
var                     app = builder.Build();
CancellationTokenSource cts = new();

#if DEBUG
var backgroundJob = app.Services.GetRequiredService<IBackgroundJobClient>();
backgroundJob.Enqueue<PrizeBackroundServices>(x => x.StartAsync(cts.Token));
#else
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate<PrizeBackroundServices>($"{Guid.NewGuid()}-{TimeProvider.System.GetUtcNow().DateTime:MM-dd-yyyy}", x => x.StartAsync(CancellationToken.None), Cron.Daily(7));

#endif

await app.RunAsync(cts.Token);

//Console.ReadLine();