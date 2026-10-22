using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UsersApi.Application.Abstractions;
using UsersApi.Domain;
using UsersApi.Infrastructure.Persistence;

namespace UsersApi.Application.Users.Commands
{
    public class CreateUserCommands
    {
        // lo que viaja del body request
        public record CreateUserCommand(string Name, string Email);

        //el response que vamos a devolver al cliente

        public record UserResponse(int Id, string Name, string Email, bool IsActive);

        //validamos con fluentValidation
        public class CreateUserValidator : AbstractValidator<CreateUserCommand>
        {
            public CreateUserValidator()
            {
                RuleFor(x => x.Name).NotEmpty().WithMessage("Nombre es requerido");


                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("El email es requerido")
                    .EmailAddress().WithMessage("El formato del email no es válido")
                    .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                    .WithMessage("El email debe contener un dominio con extensión válida (ej:.com o.com.py)");
            }

        }

        //handler logica de negocio y persistencia
        public class CreateUserHandler : ICommandHandler<CreateUserCommand, UserResponse?> {
            private readonly IDbContextFactory<AppDbContext> _factory;

            public CreateUserHandler (IDbContextFactory<AppDbContext> factory)
            {
                _factory = factory;
            }

            public async Task<UserResponse?> HandleAsync(CreateUserCommand command, CancellationToken ct)
            {
                await using var db = await _factory.CreateDbContextAsync(ct);

                var normalizeEmail = command.Email.Trim().ToLowerInvariant();

                //el email es unico en la base de datos
                var emailExists = await db.Users.AnyAsync(x => x.Email == normalizeEmail, ct);
                if (emailExists)
                {
                    return null; //el endpoint interceptara null 
                }

                var user = new User
                {
                    Name = command.Name.Trim(),
                    Email = normalizeEmail,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };

                db.Users.Add(user);
                await db.SaveChangesAsync(ct);

                return new UserResponse(user.Id, user.Name, user.Email, user.IsActive);
            }
        }
    }
}

