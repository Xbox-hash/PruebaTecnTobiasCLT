using System.ComponentModel.DataAnnotations;

namespace UsersApi.Domain;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string? PasswordHash { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Relación 1:N con Address
    public List<Address> Addresses { get; set; } = new();
}
