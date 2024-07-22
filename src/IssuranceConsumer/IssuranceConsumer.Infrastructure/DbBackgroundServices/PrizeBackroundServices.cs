using System.Threading.Channels;
using IssuranceConsumer.Infrastructure.Extensions;
using IssuranceConsumer.Infrastructure.Models;
using IssuranceConsumer.Infrastructure.PrizeProcessor.Interfaces;
using IssuranceConsumer.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IssuranceConsumer.Infrastructure.DbBackgroundServices;

/// <summary>
///     the first begin services to get all prize from db
///     then get the strategies by prize code
/// </summary>
/// <param name="channel"></param>
/// <param name="sp"></param>
/// <param name="prizeConfigProvider"></param>
/// <param name="logger"></param>
public class PrizeBackroundServices(
    Channel<PrizeConfig> channel,
    IServiceProvider                  sp,
    IPrizeConfigProvider              prizeConfigProvider,
    ILogger<PrizeBackroundServices>   logger) //: IHostedService
{
    private static readonly string InstanceCode = Guid.NewGuid().ToString();
    private static readonly string TypeName     = nameof(PrizeBackroundServices);


    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        logger.LogBeginServices(TypeName, InstanceCode);
        await using var scope     = sp.CreateAsyncScope();
        var             dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<InsurancesContext>>();
        await using var context   = await dbFactory.CreateDbContextAsync(cancellationToken);
        if (!await context.Database.CanConnectAsync(cancellationToken))
        {
            logger.UnableToConnectToDb(TypeName, InstanceCode);
            return;
        }

        var supportedConfig = prizeConfigProvider.GetSupportedPrizeCode();
        var prizes = context.Prizes.AsNoTracking()
                            .Where(x => supportedConfig.Contains(x.PrizeCode))
                            .Select(y => y.PrizeCode)
                            .AsAsyncEnumerable();
        await foreach (var prizeCode in prizes) 
        {
            var config = prizeConfigProvider.GetConfigByPrize(prizeCode);
            if (config is UnknownPrizeConfig)
                //where cause shouldn't happen , but to be safe....er... i mean after a period of time code changing
                continue;
            await channel.Writer.WriteAsync(config, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogShutdownServices(TypeName, InstanceCode);
        return Task.CompletedTask;
    }
}