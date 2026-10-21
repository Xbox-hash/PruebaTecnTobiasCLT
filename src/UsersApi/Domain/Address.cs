using System.ComponentModel.DataAnnotations;

namespace UsersApi.Domain;

public class Address
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? ZipCode { get; set; }

    // Propiedad de navegación
    public User User { get; set; } = null!;
}
