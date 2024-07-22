using Bogus;
using FakeDataGenerator;
using FakeDataGenerator.Infrastructure.Impl;
using FakeDataGenerator.Models;
using Microsoft.EntityFrameworkCore;
using ZLogger;

var builder = Host.CreateDefaultBuilder();
try
{
    builder.ConfigureServices((hostContext, services) =>
                              {
                                  var connectionStr = hostContext.Configuration.GetConnectionString("IssuranceDB");
                                  if (string.IsNullOrEmpty(connectionStr)) return;
                                  services.AddPooledDbContextFactory<IssuranceContext>(options =>
                                           {
                                               options.UseSqlServer(connectionStr);
                                           })
                                          .AddScoped<
                                               IssuranceContext>(p => p
                                                                     .GetRequiredService<
                                                                          IDbContextFactory<IssuranceContext>>()
                                                                     .CreateDbContext()); //add a default scroped in case ppl get it wrong
                                  services.AddHostedService<DbServices>();
                                  services.AddTransient<Faker<Member>, MemberFaker>();
                                  services.AddTransient<IDataGenerator, DataGenerator>();
                              })
           .ConfigureLogging(logging =>
                             {
                                 logging.ClearProviders();
                                 logging.AddZLoggerConsole(options =>
                                                           {
                                                               //options.UseJsonFormatter();
                                                           })
                                        .AddZLoggerFile($"{Environment.CurrentDirectory}/db.log");
                             });
    var                     app = builder.Build();
    CancellationTokenSource cts = new();
    await app.RunAsync(cts.Token);
}
catch
{
    // ignored
}