using Microsoft.EntityFrameworkCore;
using UsersApi.Domain;

namespace UsersApi.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    //  DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Currency> Currencies => Set<Currency>();

    // configuracion con fluentapi
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // indice unico para el correo (requisito de la documentacion)
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        //un user puede tener varios addresses 1:n (con borrado en cascada)
        modelBuilder.Entity<User>()
            .HasMany(u => u.Addresses)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // indice para el codigo en moneda (PYG, USD)
        modelBuilder.Entity<Currency>()
            .HasIndex(c => c.Code)
            .IsUnique();

        // presicion para la tasa decimal
        modelBuilder.Entity<Currency>()
            .Property(c => c.RateToBase)
            .HasPrecision(18, 4);

        // seed para datos iniciales para poder comprabar la conversion
        modelBuilder.Entity<Currency>().HasData(
            new Currency { Id = 1, Code = "PYG", Name = "Guaraní", RateToBase = 1m },
            new Currency { Id = 2, Code = "USD", Name = "Dólar", RateToBase = 7300m }
        );
    }
}