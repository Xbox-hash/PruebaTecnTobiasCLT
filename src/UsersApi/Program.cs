using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UsersApi.Infrastructure.Persistence;
using UsersApi.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// registramo el dbcontextfactory punto obligatorio
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

//configuro swagger con soporte para el boton authorize de api key
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Users & Currency API",
        Version =
"v1"
    });

    // Definición del esquema de seguridad X-API-KEY
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "X-API-KEY",
        Description = "Ingresá tu API Key para autenticar las peticiones",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "ApiKey"
        }
    };

    c.AddSecurityDefinition("ApiKey", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { securityScheme, Array.Empty<string>() }
        });
});

var app = builder.Build();

// aplicar migration automatico al arrancar
var factory = app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
await using (var db = await factory.CreateDbContextAsync())
{
    await db.Database.MigrateAsync();
}

//habilitar swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Users & Currency API v1");
});

//middleware de api key (obs: SIEMPRE despue de UseSwagger para no bloquear el UI)
app.UseMiddleware<ApiKeyMiddleware>();

app.MapGet("/", () => "API de Usuarios, Direcciones y Divisas activa");

app.Run();