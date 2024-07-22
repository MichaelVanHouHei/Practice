using Microsoft.Extensions.Logging;
using ZLogger;

namespace FakeDataGenerator.Infrastructure.Impl;

public static partial class LoggerSourceGenerator
{
    [ZLoggerMessage(LogLevel.Information, "FakeDataGenreation Start")]
    public static partial void BeginGerneation(this ILogger logger);

    [ZLoggerMessage(LogLevel.Information, "Table init already")]
    public static partial void EndBecauseOfNotFirstTime(this ILogger logger);

    [ZLoggerMessage(LogLevel.Information, "[{currentBatches}/{totalBatches}]Generating members:{genCount} timeUsed:{timeUsed}")]
    public static partial void LogGeneratedMembers(this ILogger logger, int currentBatches, int totalBatches,
                                                   int          genCount,long timeUsed);

    [ZLoggerMessage(LogLevel.Information, "[{currentBatches}/{totalBatches}]Bulk Insert Completed : {timeUsed}ms")]
    public static partial void LogBulkCompleted(this ILogger logger, int currentBatches, int totalBatches,
                                                long         timeUsed);

    [ZLoggerMessage(LogLevel.Information,
                    "All Bulk Completed TotalBatches:{totalBatches} , inserted:{inserted} , time used : {timeUsed}")]
    public static partial void LogAllCompleted(this ILogger logger, int totalBatches, long inserted, long timeUsed);

    #region lazy bad practice logging

    [ZLoggerMessage(LogLevel.Information, "Error occur during generation process :{message}")]
    public static partial void LogCriticalUnknown(this ILogger logger, string message);

    [ZLoggerMessage(LogLevel.Information, "Unknown error in bulk : {message}")]
    public static partial void LogCriticalUnknownDb(this ILogger logger, string message);

    #endregion
}