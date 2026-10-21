using System.ComponentModel.DataAnnotations;

namespace UsersApi.Domain;

public class Currency
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public decimal RateToBase { get; set; }
}
