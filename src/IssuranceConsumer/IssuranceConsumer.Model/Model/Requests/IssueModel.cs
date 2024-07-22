using System.Runtime.Serialization;

namespace IssuranceConsumer.Model.Model.Requests;

public record IssueModel
{
    [DataMember(Name = "Id")] public int Id { get; set; }

    [DataMember(Name = "Prize_Code")] public string PrizeCode { get; set; }

    //hard code , later change if requirements has more prize code , so if for extensible , write another provider for it 
    [DataMember(Name = "Qty")]
    public int
        Quantity
    {
        get;
        set;
    } // => PrizeCode switch { "PRIZE_F10"     => 1 ,"PRIZE_NEW_MEMBER" =>1, "PRIZE_FA" => 2    , _ => 0 };

    public void Deconstruct(out int id, out string prizeCode, out int quantity)
    {
        id        = Id;
        prizeCode = PrizeCode;
        quantity  = Quantity;
    }

    public override string ToString()
    {
        return $"{nameof(Id)}: {Id}, {nameof(PrizeCode)}: {PrizeCode}, {nameof(Quantity)}: {Quantity}";
    }
}