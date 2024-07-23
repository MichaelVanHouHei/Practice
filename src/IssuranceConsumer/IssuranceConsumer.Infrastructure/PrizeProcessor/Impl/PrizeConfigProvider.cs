using IssuranceConsumer.Infrastructure.PrizeProcessor.Interfaces;
using IssuranceConsumer.Model;
using Microsoft.Extensions.Logging;
using NetFabric.Hyperlinq;

namespace IssuranceConsumer.Infrastructure.PrizeProcessor.Impl;

/// <summary>
///     Provides prize configuration settings for various prize types.
/// </summary>
public class PrizeConfigProvider(ILogger<PrizeConfigProvider> logger , TimeProvider timeProvider) //(TimeProvider _timeProvier)
    : IPrizeConfigProvider
{
    private static readonly string InstanceCode = Guid.NewGuid().ToString();

    /// <summary>
    ///     A constant representing the cutoff date for new member eligibility (24 hours ago).
    ///     TODO DI timer provider instead of datetime.utcNow since different system is different
    /// </summary>
    public static DateTime WithinDay; // = DateTime.UtcNow.AddHours(-24 + 7);

    //TODO convert this rule base to external , like a class that managing rule , abstract factories for those similar critica 
    // so later one , we can use GetSupportedPrize to filter out all unsupported prize
    private static readonly Dictionary<string, PrizeConfig> _ConfigDict = new()
    {
        {
            "PRIZE_F10", new KnownPrizeConfig
            {
                PrizeName = "PRIZE_F10",
                IssuesQuantity = 1,
                Filter = member => member.IsActive && member.AvgBet > 100,
                BatchSize = 10_000 //TODO tune performance for special prize , since differernt crticia differernt members count
            }
        },
        {
            "PRIZE_FA", new KnownPrizeConfig
            {
                PrizeName = "PRIZE_FA",
                IssuesQuantity = 2,
                Filter = member => member.CardTier == "VIP" && member.AvgBet > 100_000,
                BatchSize = 10_000 //TODO tune performance for special prize , since differernt crticia differernt members count
            }
        },
        {
            "PRIZE_NEW_MEMBER", new KnownPrizeConfig
            {
                PrizeName = "PRIZE_NEW_MEMBER",
                IssuesQuantity = 1,
                Filter = member => member.JoinDate >= WithinDay,
                BatchSize = 10_000 //TODO tune performance for special prize , since differernt crticia differernt members count
            }
        }
    };

    /// <summary>
    ///     Retrieves a list of supported prize codes.
    /// </summary>
    /// <returns>A list of strings representing the supported prize codes.</returns>
    public List<string> GetSupportedPrizeCode()
    {
        return _ConfigDict.AsValueEnumerable().Select(y => y.Key).ToList();
    }

    /// <summary>
    ///     Gets the prize configuration for a specified prize code.
    /// </summary>
    /// <param name="prizeCode">The code of the prize for which to retrieve the configuration.</param>
    /// <returns>
    ///     The corresponding <see cref="PrizeConfig" /> for the specified prize code,
    ///     or an <see cref="UnknownPrizeConfig" /> if the prize code is not supported.
    /// </returns>
    public PrizeConfig GetConfigByPrize(string prizeCode)
    {
        var today = timeProvider.GetUtcNow().DateTime;
        WithinDay = new DateTime(today.Year,today.Month,today.Day,7,0,0);
        if (!_ConfigDict.TryGetValue(prizeCode, out var prizeConfig))
            return new UnknownPrizeConfig(); // BAD Practice , throw unsupportedPrizeException
        return prizeConfig;
    }
}