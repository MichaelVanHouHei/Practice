using System.Linq.Expressions;
using IssuranceConsumer.Infrastructure.Models;

namespace IssuranceConsumer.Model;

public abstract class PrizeConfig
{
    public string                         PrizeName      { get; set; }
    public int                            IssuesQuantity { get; set; }
    public Expression<Func<Member, bool>> Filter         { get; set; }
    public int                            BatchSize      { get; set; }

    public override string ToString()
    {
        return "default";
    }
}

public class KnownPrizeConfig : PrizeConfig
{
    public override string ToString()
    {
        return
            $"{nameof(PrizeName)}: {PrizeName}, {nameof(IssuesQuantity)}: {IssuesQuantity}, {nameof(Filter)}: {Filter}, {nameof(BatchSize)}: {BatchSize}";
    }
}

public class UnknownPrizeConfig : PrizeConfig
{
    public override string ToString()
    {
        return "Unknown Prize Config";
    }
}