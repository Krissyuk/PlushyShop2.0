List<Order> repo =
[
    new(1,new(2005,3,11),"1","1","1","1", "79826996614","Выполнено"),
];

var builder = WebApplication.CreateBuilder();
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(a => a.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

string message = "";

app.MapGet("orders", (int param = 0) =>
{
    string buffer = message;
    message = "";
    if (param != 0)
        return new { repo = repo.FindAll(x => x.Number == param), message = buffer };
    return new { repo, message = buffer };
});

app.MapGet("create", ([AsParameters] Order dto) =>
    repo.Add(dto));

app.MapGet("update", ([AsParameters] UpdateOrderDTO dto) => 
{ 
    var o = repo.Find(x => x.Number == dto.Number);
    if (o == null)
        return;
    if (dto.Status != o.Status && dto.Status != "")
        {
        o.Status = dto.Status;
        message += $"Статус заявки №{o.Number} изменен\n";
        if (o.Status == "завершена")
        {
            message += $"Заявка №{o.Number} завершена\n";
            o.EndDate = DateOnly.FromDateTime(DateTime.Now);
        }
        }
    if (dto.Problemtype != "")
        o.Problemtype = dto.Problemtype;
    if (dto.Master != "")
        o.Master = dto.Master;
    if (dto.Comment != "")
        o.Comment.Add(dto.Comment);
});

app.Run();

class Order(int number, DateOnly startDate, string device, string model, string problemtype, string fullnameclient, string phone, string status)
{
    public int Number { get; set; } = number;
    public DateOnly StartDate { get; set; } = startDate;
    public string Device { get; set; } = device;
    public string Model { get; set; } = model;
    public string Problemtype { get; set; } = problemtype;
    public string Fullnameclient { get; set; } = fullnameclient;
    public string Phone { get; set; } = phone;
    public string Status { get; set; } = status;
    public string? Master { get; set; } = "Не назначено";
    public DateOnly? EndDate { get; set; } = null;
    public List<string> Comment { get; set; } = [];
}

record class UpdateOrderDTO(int Number, string? Status = "", string? Problemtype = "", string? Master = "", string? Comment = "");
