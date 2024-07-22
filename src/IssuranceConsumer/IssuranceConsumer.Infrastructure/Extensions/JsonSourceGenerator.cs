using System.Text.Json.Serialization;
using IssuranceConsumer.Model.Model.Requests;

namespace IssuranceConsumer.Infrastructure.Extensions;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(IssueModel))]
public partial class IssueModelJsonSourceGenerator : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(IssueResult))]
public partial class IssueResultSourceGenerator : JsonSerializerContext
{
}