using SunAdmin.Contracts.Departments;

namespace SunAdmin.Application.Abstractions;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentDto>> GetTreeAsync(CancellationToken cancellationToken = default);
    Task<DepartmentDto?> GetAsync(long id, CancellationToken cancellationToken = default);
    Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<DepartmentDto> UpdateAsync(long id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
