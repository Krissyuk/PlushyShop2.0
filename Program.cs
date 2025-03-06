using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System;

var adminRole = new Role("admin");
var creatorRole  = new Role("creator");
var userRole  = new Role("user");

List<Product> repo =
[
    new(0,"Кукла","Криссюк",0,"Вязание","Обычная кукла",100),
];

var repousers = new List<User>
{
    new ("tom@gmail.com", "12345", adminRole),
    new ("bob@gmail.com", "55555", userRole),
};

var builder = WebApplication.CreateBuilder();
builder.Services.AddCors();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/accessdenied";
    });
builder.Services.AddAuthorization();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors(a => a.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapGet("/accessdenied", async (HttpContext context) =>
{
    context.Response.StatusCode = 403;
    await context.Response.WriteAsync("Access Denied");
});
app.MapGet("/login", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    string loginForm = @"<!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8' />
        <title>METANIT.COM</title>
    </head>
    <body>
        <h2>Login Form</h2>
        <form method='post'>
            <p>
                <label>Email</label><br />
                <input name='email' />
            </p>
            <p>
                <label>Password</label><br />
                <input type='password' name='password' />
            </p>
            <input type='submit' value='Login' />
        </form>
    </body>
    </html>";
    await context.Response.WriteAsync(loginForm);
});

app.MapPost("/login", async (string? returnUrl, HttpContext context) =>
{
    // получаем из формы email и пароль
    var form = context.Request.Form;
    // если email и/или пароль не установлены, посылаем статусный код ошибки 400
    if (!form.ContainsKey("email") || !form.ContainsKey("password"))
        return Results.BadRequest("Email и/или пароль не установлены");
    string email = form["email"];
    string password = form["password"];

    // находим пользователя 
    User? user = repousers.FirstOrDefault(p => p.Email == email && p.Password == password);
    // если пользователь не найден, отправляем статусный код 401
    if (user is null) return Results.Unauthorized();
    var claims = new List<Claim>
    {
        new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
        new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.Name)
    };
    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
    await context.SignInAsync(claimsPrincipal);
    return Results.Redirect(returnUrl ?? "/");
});
// доступ только для роли admin
app.Map("/admin", [Authorize(Roles = "admin")] () => "Admin Panel");

// доступ только для ролей admin и user
app.Map("/", [Authorize(Roles = "admin, user")] (HttpContext context) =>
{
    var login = context.User.FindFirst(ClaimsIdentity.DefaultNameClaimType);
    var role = context.User.FindFirst(ClaimsIdentity.DefaultRoleClaimType);
    return $"Name: {login?.Value}\nRole: {role?.Value}";
});
app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return "Данные удалены";
});

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
    var p = repo.Find(x => x.Id == dto.Id);
    if (p == null)
        return;
    if (dto.Name != p.Name)
        {
        p.Name = dto.Name;
        message += $"Продукт №{p.Id} изменен\n";
        }
    if (dto.Category != p.Category)
        {
        p.Category = dto.Category;
        message += $"Продукт №{p.Id} изменен\n";
        }
    if (dto.Info != p.Info)
        {
        p.Info = dto.Info;
        message += $"Продукт №{p.Id} изменен\n";
        }
    if (dto.Price != p.Price)
        {
        p.Price = dto.Price;
        message += $"Продукт №{p.Id} изменен\n";
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

class User
{
    public string Email { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }
    public User(string email, string password, Role role)
    {
        Email = email;
        Password = password;
        Role = role;
    }
}
class Role
{
    public string Name { get; set; }
    public Role(string name) => Name = name;
}

record class UpdateUserDTO(int Id, string NickName="", string Login="", string Password="", string Gender="Не определено");
record class UpdateProductDTO(int Id, string Name="", string Category ="", string Info="", int Price = 0);
