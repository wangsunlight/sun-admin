using SunAdmin.Contracts.CodeGeneration;
using SunAdmin.Contracts.Common;

namespace SunAdmin.Application.Abstractions;

public interface ICodeGenerationService
{
    Task<PagedResult<CodeGenerationTemplateDto>> GetTemplatesAsync(CodeGenerationTemplateQuery query, CancellationToken cancellationToken = default);
    Task<CodeGenerationTemplateDto> CreateTemplateAsync(CreateCodeGenerationTemplateRequest request, CancellationToken cancellationToken = default);
    Task<CodeGenerationTemplateDto> UpdateTemplateAsync(long id, UpdateCodeGenerationTemplateRequest request, CancellationToken cancellationToken = default);
    Task DeleteTemplateAsync(long id, CancellationToken cancellationToken = default);
}
