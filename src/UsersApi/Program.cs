using Microsoft.EntityFrameworkCore;
using UsersApi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// registramo el dbcontextfactory punto obligatorio
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// aplicar migration automatico al arrancar
var factory = app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
await using (var db = await factory.CreateDbContextAsync())
{
    await db.Database.MigrateAsync();
}

app.MapGet("/", () => "API de Usuarios, Direcciones y Divisas activa");

app.Run();