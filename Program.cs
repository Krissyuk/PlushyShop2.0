List<Product> repo =
[
    new(0,"Кукла","Криссюк",0,"Вязание","Обычная кукла",100),
];

var builder = WebApplication.CreateBuilder();
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(a => a.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

string message = "";

app.MapGet("products", (int param = 0) =>
{
    string buffer = message;
    message = "";
    if (param != 0)
        return new { repo = repo.FindAll(x => x.Id == param), message = buffer };
    return new { repo, message = buffer };
});

app.MapGet("create", ([AsParameters] Product dto) =>
    repo.Add(dto));

app.MapGet("update", ([AsParameters] UpdateProductDTO dto) => 
{ 
    var o = repo.Find(x => x.Id == dto.Id);
    if (o == null)
        return;
    if (dto.Name != o.Name)
        {
        o.Name = dto.Name;
        message += $"Продукт №{o.Id} изменен\n";
        }
    if (dto.Category != o.Category)
        {
        o.Category = dto.Category;
        message += $"Продукт №{o.Id} изменен\n";
        }
    if (dto.Info != o.Info)
        {
        o.Info = dto.Info;
        message += $"Продукт №{o.Id} изменен\n";
        }
    if (dto.Price != o.Price)
        {
        o.Price = dto.Price;
        message += $"Продукт №{o.Id} изменен\n";
        }
});

app.Run();

class Product(int id, string name, string creator, int creatorId, string category, string info, double price)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string Creator { get; set; } = creator;
    public int CreatorId { get; set; } = creatorId;
    public string Category { get; set; } = category;
    public string Info { get; set; } = info;
    public double Price { get; set; } = price;
}

class User(int id, string nickName, string login, string password, string gender)
{
    public int Id { get; set; } = id;
    public string NickName { get; set; } = nickName;
    public string Login { get; set; } = login;
    public string Password { get; set; } = password;
    public string Gender { get; set; } = gender;
}

record class UpdateUserDTO(int Id, string NickName="", string Login="", string Password="", string Gender="Не определено");
record class UpdateProductDTO(int Id, string Name="", string Category ="", string Info="", int Price = 0);
