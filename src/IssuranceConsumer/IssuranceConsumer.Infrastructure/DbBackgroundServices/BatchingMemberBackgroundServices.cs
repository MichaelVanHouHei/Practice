using System.Threading.Channels;
using IssuranceConsumer.Infrastructure.Extensions;
using IssuranceConsumer.Infrastructure.PrizeProcessor.Interfaces;
using IssuranceConsumer.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IssuranceConsumer.Infrastructure.DbBackgroundServices;

public class BatchingMemberBackgroundServices(
    Channel<PrizeConfig>         channel,
    IServiceProvider                          sp,
    ILogger<BatchingMemberBackgroundServices> logger) : BackgroundService
{
    private static readonly string InstanceCode = Guid.NewGuid().ToString();
    private static readonly string TypeName     = nameof(BatchingMemberBackgroundServices);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartAsync(stoppingToken);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogBeginServices(TypeName, InstanceCode);

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.CancelTokenRequested(TypeName, InstanceCode);
                return;
            }

            try
            {
                await using var scope             = sp.CreateAsyncScope();
                var             batchingProcessor = scope.ServiceProvider.GetRequiredService<IPrizeProcessor>();
                var             config            = await channel.Reader.ReadAsync(cancellationToken);
                await batchingProcessor.ProduceToRequestQueeueAsync(config, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.UnExpected(TypeName, InstanceCode, ex.Message);
            }

            await Task.Delay(-1, cancellationToken);
        }
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogShutdownServices(TypeName, InstanceCode);
        return Task.CompletedTask;
    }
}