using System.ComponentModel.DataAnnotations;

namespace TestAPBD.Models.DTOs;

public class VisitDetailsDto
{
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public ClientDto Client { get; set; }
    [Required]
    public MechanicDto Mechanic { get; set; }
    public List<VisitServiceDto> VisitServices { get; set; } = [];
}

public class VisitServiceDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [Range(0.01, 99999999.99)]
    public decimal ServiceFee { get; set; }
}

public class MechanicDto
{
    [Required]
    public int MechanicId { get; set; }
    [Required]
    [MaxLength(14)]
    public string LicenceNumber { get; set; } = String.Empty;
}

public class ClientDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = String.Empty;
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = String.Empty;
    [Required]
    public DateTime DateOfBirth { get; set; }
}