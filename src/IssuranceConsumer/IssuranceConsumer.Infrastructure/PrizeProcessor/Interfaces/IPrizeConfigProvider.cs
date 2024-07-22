using IssuranceConsumer.Model;

namespace IssuranceConsumer.Infrastructure.PrizeProcessor.Interfaces;

public interface IPrizeConfigProvider
{
    List<string> GetSupportedPrizeCode();
    PrizeConfig  GetConfigByPrize(string prizeCode);
}