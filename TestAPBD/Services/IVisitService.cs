using TestAPBD.Models.DTOs;

namespace TestAPBD.Services;

public interface IVisitService
{
    public Task<VisitDetailsDto> GetVisitAsync(int visitId);
    public Task CreateVisitAsync(CreateVisitDto dto);
}