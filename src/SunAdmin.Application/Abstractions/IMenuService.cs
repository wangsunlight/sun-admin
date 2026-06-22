using SunAdmin.Contracts.Menus;

namespace SunAdmin.Application.Abstractions;

public interface IMenuService
{
    Task<IReadOnlyList<MenuTreeNodeDto>> GetTreeAsync(CancellationToken cancellationToken = default);
    Task<MenuDto?> GetAsync(long id, CancellationToken cancellationToken = default);
    Task<MenuDto> CreateAsync(CreateMenuRequest request, CancellationToken cancellationToken = default);
    Task<MenuDto> UpdateAsync(long id, UpdateMenuRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
