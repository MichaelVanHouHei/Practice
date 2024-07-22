using IssuranceConsumer.Model;
using IssuranceConsumer.Model.Model.Requests;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace IssuranceConsumer.Infrastructure.Extensions;

public static partial class LoggingSourceGenerator
{
    [ZLoggerMessage(LogLevel.Information, Message = "Begin Services:[{servicesName}|{id}]")]
    public static partial void LogBeginServices(this ILogger logger, string servicesName, string id);

    [ZLoggerMessage(LogLevel.Information, Message = "Shutdown Services:[{servicesName}|{id}]")]
    public static partial void LogShutdownServices(this ILogger logger, string servicesName, string id);

    [ZLoggerMessage(LogLevel.Critical,
                    Message = "[{servicesName}|{id}] Unexpected handled exception message:{message}")]
    public static partial void UnExpected(this ILogger logger, string servicesName, string id, string message);

    [ZLoggerMessage(LogLevel.Critical,
                    Message = "[{servicesName}|{id}] Got Prize : {prizeCode} , with Config:{config}")]
    public static partial void Revived(this ILogger logger, string servicesName, string id, string prizeCode,
                                       PrizeConfig  config);

    [ZLoggerMessage(LogLevel.Critical,
                    Message = "[{servicesName}|{id}] prize code :  {prizeCode} total fetched:{total}")]
    public static partial void FetchBatchCompleted(this ILogger logger,    string servicesName, string id,
                                                   string       prizeCode, int    total);

    [ZLoggerMessage(LogLevel.Critical, Message = "[{servicesName}|{id}] posting data issue model{model}")]
    public static partial void RequstingIssueModel(this ILogger logger, string servicesName, string id,
                                                   IssueModel   model);

    [ZLoggerMessage(LogLevel.Critical,
                    Message = "[{servicesName}|{id}] failed posting data model {model} with result {result}")]
    public static partial void FailedRequstIssueModel(this ILogger logger, string      servicesName, string id,
                                                      IssueModel   model,  IssueResult result);

    [ZLoggerMessage(LogLevel.Critical, Message = "[{servicesName}|{id}] Async cancel requested")]
    public static partial void CancelTokenRequested(this ILogger logger, string servicesName, string id);

    [ZLoggerMessage(LogLevel.Information,
                    Message =
                        "[{servicesName}|{id}]fetching prize Code : {prizeCode} with memberCount  : {memberCount} current count : {count} ")]
    public static partial void LogBatchFetch(this ILogger logger,      string servicesName, string id, string prizeCode,
                                             int          memberCount, int    count);

    [ZLoggerMessage(LogLevel.Information, Message = "[{servicesName}|{id}] pending {prizeName} requests : {count}")]
    public static partial void PendingConfig(this ILogger logger, string servicesName, string id, string prizeName,
                                             int          count);

    [ZLoggerMessage(LogLevel.Information, Message = "[{servicesName}|{id}] pending {prizeName} requests : {count}")]
    public static partial void CompletedAllBatchRequests(this ILogger logger, string servicesName, string id,
                                                         string       prizeName,
                                                         int          count);

    [ZLoggerMessage(LogLevel.Information, Message = "[{servicesName}|{id}] Unable to connect to database")]
    public static partial void UnableToConnectToDb(this ILogger logger, string servicesName, string id);
}