using IssuranceConsumer.Model;

namespace IssuranceConsumer.Infrastructure.PrizeProcessor.Interfaces;

public interface IPrizeProcessor
{
    Task<bool> ProduceToRequestQueeueAsync(PrizeConfig config, CancellationToken token);
}