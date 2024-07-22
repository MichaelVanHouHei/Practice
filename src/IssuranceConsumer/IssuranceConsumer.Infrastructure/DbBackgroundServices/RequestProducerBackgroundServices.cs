using System.Text.Json;
using System.Threading.Channels;
using Flurl.Http;
using IssuranceConsumer.Infrastructure.Extensions;
using IssuranceConsumer.Model.Model.Requests;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IssuranceConsumer.Infrastructure.DbBackgroundServices;

//since only one enpoint we dont DI ClientFactory here 
public class RequestProducerBackgroundServices(
    Channel<List<IssueModel>> channel,
    IHttpClientFactory                          clients,
    ILogger<RequestProducerBackgroundServices>  logger)
    : BackgroundService
{
    private static readonly string TypeName = nameof(RequestProducerBackgroundServices);
    // private readonly        IFlurlClient _client  = new FlurlClient(clients.CreateClient("productEndpoints"));

    private readonly string InstanceCode = Guid.NewGuid().ToString();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return StartAsync(stoppingToken);
    }

    /// <summary>
    ///     background services that request to issues endpoint
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogBeginServices(TypeName, InstanceCode);

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.CancelTokenRequested(TypeName, InstanceCode);
                continue; // yes dont end services 
            }

            try
            {
                var models = await channel.Reader.ReadAsync(cancellationToken);
                if (models.Count == 0) continue;
                var prize = models[0].PrizeCode;
                logger.PendingConfig(TypeName, InstanceCode, prize, models.Count);
                //foreach (var model in models)
                //{
                //    await PostIssuesAsync(model, cancellationToken);
                //}
                // //  DONT use when all since there are rate limit unless there is semaphore
                //   await Task.WhenAll(models.Select(y => PostIssuesAsync(y, cancellationToken)));
                //indeed , this is not thread safe
                await Parallel.ForEachAsync(models,
                                            new ParallelOptions
                                            {
                                                MaxDegreeOfParallelism = Math.Min(1, Environment.ProcessorCount / 2),
                                                CancellationToken      = cancellationToken
                                            },
                                            async (model, token) => { await PostIssuesAsync(model, token); });
                logger.CompletedAllBatchRequests(TypeName, InstanceCode, prize, models.Count);
                //using var semaphorse = new SemaphoreSlim(Environment.ProcessorCount);
                //List<Task> tasks = new();
                //foreach (var model in models)
                //{
                //    try
                //    {
                //        await semaphorse.WaitAsync(cancellationToken);
                //        tasks.Add(PostIssuesAsync(model, cancellationToken));
                //    }
                //    finally
                //    {
                //        semaphorse.Release();
                //    }

                //}

                //await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                logger.UnExpected(TypeName, InstanceCode, ex.Message);
                // Microsoft.Extensions.Http.Polly
                // AddPolicyHandler 
            }

            await Task.Delay(-1, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogShutdownServices(TypeName, InstanceCode);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     ///TODO we  have to add resilences to here
    ///     Microsoft.Extensions.Http.Polly
    ///     AddPolicyHandler
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<bool> PostIssuesAsync(IssueModel model, CancellationToken cancellationToken)
    {
        IFlurlClient client = new FlurlClient(clients.CreateClient("productEndpoints"));
        await using var result = await client.Request("api/issueprize")
                                             .AllowAnyHttpStatus()
                                             .WithHeader("Keep-Alive", "false")
                                             .PostJsonAsync(model, HttpCompletionOption.ResponseHeadersRead,
                                                            cancellationToken)
                                             .ReceiveStream();
        //if (result.StatusCode != 200) return false; // this indicates services unavalible ,etc network issues ...
        //logger.RequstingIssueModel(TypeName, InstanceCode, model);
        //await using var streams = await result.GetStreamAsync();
        // if (streams is null) logger.FailedRequstIssueModel(TypeName, InstanceCode, model, new IssueResult());
        var issueResult =
            (await JsonSerializer.DeserializeAsync<IssueResult?>(result,
                                                                 IssueResultSourceGenerator.Default.IssueResult,
                                                                 cancellationToken))!;
        if (issueResult.Status != 200 && !string.IsNullOrEmpty(issueResult.Error))
        {
            logger.FailedRequstIssueModel(TypeName, InstanceCode, model, issueResult);
            // logger the error message
            //this indicates really have some wrong for our data}
            return false;
        }


        return true;
    }
}