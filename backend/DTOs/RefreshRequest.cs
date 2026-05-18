using System.ComponentModel.DataAnnotations;

namespace SmeKpiDashboard.DTOs;

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
