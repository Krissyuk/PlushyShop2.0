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

class Product(string name, string creator, string creatorId, string category, string info, string town, double price, double deliveryPrice)
{
    public string Name { get; set; } = name;
    public string Creator { get; set; } = creator;
    public string CreatorId { get; set; } = creatorId;
    public string Category { get; set; } = category;
    public string Info { get; set; } = info;
    public string Town { get; set; } = town;
    public double Price { get; set; } = price;
    public double DeliveryPrice { get; set; } = deliveryPrice;
}

class User(int id, string nickName, string login, string password, string gender)
{
    public int Id { get; set; } = id;
    public string NickName { get; set; } = nickName;
    public string Login { get; set; } = login;
    public string Password { get; set; } = password;
    public string Gender { get; set; } = gender;
}

record class UpdateUserDTO(int id, string nickName, string login, string password, string gender);
record class UpdateProductDTO(string name, string category, string info, string town, double price, double deliveryPrice);
