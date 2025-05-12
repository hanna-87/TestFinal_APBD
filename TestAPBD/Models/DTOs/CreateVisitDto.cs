using System.ComponentModel.DataAnnotations;

namespace TestAPBD.Models.DTOs;

public class CreateVisitDto
{
    [Required]
    public int VisitId { get; set; }
    [Required]
    public int ClientId { get; set; }
    [Required]
    public string MechanicLicenceNumber { get; set; } = String.Empty;

    public List<ServiceInputDto> Services { get; set; } = [];
}

public class ServiceInputDto
{
    [Required]
    [MaxLength(100)]
    public string ServiceName { get; set; } = String.Empty;
    [Required]
    [Range(0.01,99999999.99)]
    public decimal ServiceFee { get; set; }
}