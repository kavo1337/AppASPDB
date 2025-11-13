using appApi.Models;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var cs = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Not default string connection!");

builder.Services.AddDbContext<DBContext>(opt =>
{
    opt.UseSqlServer(cs);
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DBContext>();
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/ping", () => Results.Ok(new {ping = "ping"}));

app.MapGet("/products", async (DBContext db) =>
    Results.Ok(await db.Users.AsNoTracking().ToListAsync())
);

app.MapPost("/products", async (DBContext db, Users input) =>
{
    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest(new { error = "Name is required" });

    var entity = new Users
    {
        Name = input.Name,
        Email = input.Email,
        Password = input.Password,
        Username = input.Username,
        Hobby = input.Hobby,
        Age = input.Age,
        Balance = input.Balance
    };

    db.Users.Add(entity);
    await db.SaveChangesAsync();

    return Results.Created($"/products/{entity.Id}", entity);
});

app.MapDelete("/products/{id}", async (DBContext db, int id) =>
{
    var user = await db.Users.FindAsync(id);
    if (user == null)
        return Results.NotFound(new { error = "User not found" });

    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
