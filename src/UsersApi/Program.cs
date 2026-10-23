using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UsersApi.Api;
using UsersApi.Application.Abstractions;
using UsersApi.Application.Addresses.Commands;
using UsersApi.Application.Addresses.Queries;
using UsersApi.Application.Currencies.Commands;
using UsersApi.Application.Currencies.Queries;
using UsersApi.Application.CurrencyConversion;
using UsersApi.Application.Users.Commands;
using static UsersApi.Application.Users.Commands.CreateUserCommands;
using UsersApi.Application.Users.Queries;
using UsersApi.Infrastructure.Persistence;
using UsersApi.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Users & Currency API",
        Version = "v1"
    });

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

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<ICommandHandler<CreateUserCommand, UserResponse?>, CreateUserHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateUserCommand, UpdateUserResult>, UpdateUserHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteUserCommand, bool>, DeleteUserHandler>();
builder.Services.AddScoped<IQueryHandler<GetUsersQuery, List<UserResponse>>, GetUsersHandler>();
builder.Services.AddScoped<IQueryHandler<GetUserByIdQuery, UserResponse?>, GetUserByIdHandler>();
builder.Services.AddScoped<ICommandHandler<BulkCreateUsersCommand, BulkResult>, BulkCreateUsersHandler>();

builder.Services.AddScoped<ICommandHandler<CreateAddressCommand, AddressResponse?>, CreateAddressHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateAddressCommand, AddressResponse?>, UpdateAddressHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteAddressCommand, bool>, DeleteAddressHandler>();
builder.Services.AddScoped<IQueryHandler<GetUserAddressesQuery, List<AddressResponse>?>, GetUserAddressesHandler>();

builder.Services.AddScoped<ICommandHandler<CreateCurrencyRequest, CurrencyResponse?>, CreateCurrencyHandler>();
builder.Services.AddScoped<IQueryHandler<GetCurrenciesQuery, List<CurrencyResponse>>, GetCurrenciesHandler>();
builder.Services.AddScoped<ICommandHandler<ConvertCurrencyRequest, ConvertCurrencyResponse?>, ConvertCurrencyHandler>();

var app = builder.Build();

var factory = app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
await using (var db = await factory.CreateDbContextAsync())
{
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Users & Currency API v1");
});

app.UseMiddleware<ApiKeyMiddleware>();

app.MapGet("/", () => "API de Usuarios, Direcciones y Divisas activa");

app.MapUserEndpoints();
app.MapAddressEndpoints();
app.MapCurrencyEndpoints();

app.Run();
