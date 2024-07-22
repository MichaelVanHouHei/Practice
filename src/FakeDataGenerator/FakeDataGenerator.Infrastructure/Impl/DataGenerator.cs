using System.Diagnostics;
using System.Runtime.CompilerServices;
using Bogus;
using EFCore.BulkExtensions;
using FakeDataGenerator.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetFabric.Hyperlinq;

namespace FakeDataGenerator.Infrastructure.Impl;

public class DataGenerator(
    IDbContextFactory<IssuranceContext> dbContextFactory,
    ILogger<DataGenerator>              logger,
    Faker<Member>                       faker) : IDataGenerator
{
    private static readonly int TotalBatchSize = 10_000_000; // for POC

    private static readonly int
        BatchSize = 1_000_000; // should be injected IConfiguration , but for time limited , just hardCode

    public async Task GenerateDataAsync(CancellationToken token = default)
    {
        try
        {
            logger.BeginGerneation();
            if (await GetCurrentMembersCount(token) >= TotalBatchSize)
            {
                logger.EndBecauseOfNotFirstTime();
                return;
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var batches = TotalBatchSize / BatchSize;
            //DONT do this , the list can't dispose and the RAM will eat up 
          //  await Task.WhenAll(ValueEnumerable.Range(1, batches).Select(y => DoDbOpsAsync(y, batches, token)));
          await Parallel.ForAsync(1, batches, async (i, t) =>
                                              {
                                                  var st   = new Stopwatch();
                                                  var data = GenerateFakeMembers();
                                                  logger.LogGeneratedMembers(i, batches, data.Count, st.ElapsedMilliseconds);
                                                  await BulkWriteDocs(data , t);
                                                  logger.LogBulkCompleted(i, batches, st.ElapsedMilliseconds);
                                              });
          logger.LogAllCompleted(batches, TotalBatchSize, stopWatch.ElapsedMilliseconds);
        }
        catch (Exception ex) //TODO [BAD][PRACTICE] lazy , I should log with speicific exception
        {
            logger.LogCriticalUnknown(ex.Message);
        }
    }

    //private async Task DoDbOpsAsync(int currentBatch, int totalBatches, CancellationToken token = default)
    //{
    //    var stopWatch = new Stopwatch();
    //    stopWatch.Start();
    //    var members = GenerateFakeMembers();
    //    logger.LogGeneratedMembers(currentBatch, totalBatches, members.Count , stopWatch.ElapsedMilliseconds);
    //    await BulkWriteDocs(members, token);
    //    logger.LogBulkCompleted(currentBatch, totalBatches, stopWatch.ElapsedMilliseconds);
    //}

    //can move this to DataProvider ( so that we can mock with different generation , but for poc , fine)
    private List<Member> GenerateFakeMembers()
    {
        return faker.Generate(BatchSize);
    }


    private async Task BulkWriteDocs(List<Member> members, CancellationToken token = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);
            await dbContext.BulkInsertAsync(members, cancellationToken: token);
        }
        catch (Exception ex) //TODO [BAD][PRACTICE] lazy 
        {
            logger.LogCriticalUnknownDb(ex.Message);
        }
    }

    private async Task<int> GetCurrentMembersCount(CancellationToken token = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(token);
        return await dbContext.Members.CountAsync(token);
    }
}