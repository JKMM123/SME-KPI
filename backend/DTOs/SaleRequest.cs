using System.ComponentModel.DataAnnotations;

namespace SmeKpiDashboard.DTOs;

public class SaleRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}
