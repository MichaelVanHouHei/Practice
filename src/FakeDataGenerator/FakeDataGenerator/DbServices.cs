namespace FakeDataGenerator;

public class DbServices(IDataGenerator dataGenerator) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return dataGenerator.GenerateDataAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}