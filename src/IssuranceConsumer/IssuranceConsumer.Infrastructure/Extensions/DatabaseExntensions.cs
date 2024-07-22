using IssuranceConsumer.Infrastructure.Models;
using IssuranceConsumer.Model.Model.Requests;
using Microsoft.EntityFrameworkCore;

namespace IssuranceConsumer.Infrastructure.Extensions;

public static class DatabaseExntensions
{
    //public static DateTime WithinDay = TimeProvider.System.GetUtcNow().AddHours(-24 + 7).DateTime;
    //// public static DateTime WithinDay = TimeProvider.System.GetUtcNow().AddHours(-24).DateTime; //should DI time provider instead of static
    //public static IQueryable<Member> FindF10(this IQueryable<Member> query)
    //{
    //    return query.AsNoTracking().Where(x => x.IsActive && x.AvgBet > 100);
    //}
    //public static IQueryable<Member> FindFA(this IQueryable<Member> query)
    //{
    //    //i dont enum here because C# enum slow
    //    return query.AsNoTracking().Where(x => x.CardTier == "VIP" && x.AvgBet > 100_000);
    //}
    //public static IQueryable<Member> FindNewMembers(this IQueryable<Member> query)
    //{
    //    return query.AsNoTracking().Where(x => x.JoinDate >= WithinDay);
    //}
    public static IQueryable<int> BatchMemberIds(this IQueryable<Member> query, int offset, int batchSize)
    {
        return query.AsNoTracking().OrderBy(x => x.MemberId).Skip(offset).Take(batchSize).Select(y => y.MemberId);
    }

    public static IssueModel ToIssueModel(this int memberId, string prizeCode, int quantity)
    {
        return new IssueModel
        {
            Id        = memberId,
            PrizeCode = prizeCode,
            Quantity  = quantity
        };
    }
}