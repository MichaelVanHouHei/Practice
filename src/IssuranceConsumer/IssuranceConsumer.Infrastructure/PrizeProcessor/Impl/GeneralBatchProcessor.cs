using System.Threading.Channels;
using IssuranceConsumer.Infrastructure.Extensions;
using IssuranceConsumer.Infrastructure.Models;
using IssuranceConsumer.Infrastructure.PrizeProcessor.Interfaces;
using IssuranceConsumer.Model;
using IssuranceConsumer.Model.Model.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetFabric.Hyperlinq;

namespace IssuranceConsumer.Infrastructure.PrizeProcessor.Impl;

/// <summary>
///     Processes batches of prize requests and writes them to a channel.
/// </summary>
public class GeneralBatchProcessor(
    Channel<List<IssueModel>> channel,
    ILogger<GeneralBatchProcessor>              logger,
    IDbContextFactory<InsurancesContext>        dbContextFactory) : IPrizeProcessor
{
    private static readonly string InstanceCode = Guid.NewGuid().ToString();
    private static readonly string TypeName     = nameof(GeneralBatchProcessor);

    /// <summary>
    ///     Produces prize requests to the request queue asynchronously.
    /// </summary>
    /// <param name="config">The configuration for prize processing.</param>
    /// <param name="token">A cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with a boolean result indicating success or failure.</returns>
    public async Task<bool> ProduceToRequestQueeueAsync(PrizeConfig config, CancellationToken token = default)
    {
        try
        {
            logger.LogBeginServices(TypeName, InstanceCode);
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);
            var             offset    = 0;
            var             total     = 0;
            logger.Revived(TypeName, InstanceCode, config.PrizeName, config);
            while (true) //while true here because if statement can handle more2 
            {
                if (!await dbContext.Database.CanConnectAsync(token))
                {
                    logger.UnableToConnectToDb(TypeName, InstanceCode);
                    continue; // wait reconnect
                }

                var members = await dbContext.Members
                                             .Where(config.Filter)
                                             .BatchMemberIds(offset, config.BatchSize)
                                             .ToListAsync(token);
                //heap allocation here , refactor  later 
                logger.LogBatchFetch(TypeName, InstanceCode, config.PrizeName, members.Count, total);
                if (members.Count == 0)
                {
                    logger.FetchBatchCompleted(TypeName, InstanceCode, config.PrizeName, total);
                    return true;
                }

                //var count = members.Count / 100;
                //foreach (var chunkMembers in members.Chunk(count))
                //{
                await
                    channel.Writer
                           .WriteAsync([.. members.AsValueEnumerable().Select(y => y.ToIssueModel(config.PrizeName, config.IssuesQuantity))],
                                       token);
                //}


                total += members.Count;
                if (members.Count < config.BatchSize)
                {
                    logger.FetchBatchCompleted(TypeName, InstanceCode, config.PrizeName, total);
                    break;
                }

                offset += config.BatchSize;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.UnExpected(TypeName, InstanceCode, ex.Message);
        }

        return false;
    }
}