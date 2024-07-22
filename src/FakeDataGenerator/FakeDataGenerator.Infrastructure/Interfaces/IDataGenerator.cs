public interface IDataGenerator
{
    public Task GenerateDataAsync(CancellationToken token);
}