using System.Runtime.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.MapPost("/api/issueprize", (IssueModel model ) => new IssueResult(){Status = 200 , Error = ""})
.WithName("issueprize")
.WithOpenApi();

app.Run();
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

