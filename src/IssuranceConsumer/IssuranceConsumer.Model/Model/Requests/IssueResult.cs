using System.Runtime.Serialization;

namespace IssuranceConsumer.Model.Model.Requests;

public record IssueResult
{
    [DataMember(Name = "status")] public int Status { get; set; }

    [DataMember(Name = "error_msg")] public string Error { get; set; }

    public override string ToString()
    {
        return $"{nameof(Status)}: {Status}, {nameof(Error)}: {Error}";
    }

    public void Deconstruct(out int status, out string error)
    {
        status = Status;
        error  = Error;
    }
}