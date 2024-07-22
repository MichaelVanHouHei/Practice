using Bogus;
using FakeDataGenerator.Models;

namespace FakeDataGenerator.Infrastructure.Impl;

public class MemberFaker : Faker<Member>
{
    public MemberFaker()
    {
        RuleFor(m => m.Name,     f => f.Name.FullName());
        RuleFor(m => m.CardTier, f => f.PickRandom("Mass", "VIP")); //yet again , enum harm performance
        RuleFor(m => m.IsActive, f => f.Random.Bool() ? 1 : 0);
        RuleFor(m => m.JoinDate, f => f.Date.RecentDateOnly());
        RuleFor(m => m.AvgBet,   f => f.Random.Number(1, 1000));
    }
}