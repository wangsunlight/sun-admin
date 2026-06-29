using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Common;
using SunAdmin.Contracts.CodeGeneration;
using SunAdmin.Contracts.Common;
using SunAdmin.Domain.Entities;

namespace SunAdmin.Infrastructure.Services;

public sealed class CodeGenerationService(IFreeSql freeSql, IEntityAuditService auditService) : ICodeGenerationService
{
    public async Task<PagedResult<CodeGenerationTemplateDto>> GetTemplatesAsync(CodeGenerationTemplateQuery query, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var selector = freeSql.Select<CodeGenerationTemplate>().Where(x => x.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            selector = selector.Where(x => x.Name.Contains(query.Keyword) || x.TemplateKey.Contains(query.Keyword) || x.TargetKind.Contains(query.Keyword));
        }

        var total = await selector.CountAsync(cancellationToken);
        var templates = await selector.OrderByDescending(x => x.Id).Page(pageIndex, pageSize).ToListAsync(cancellationToken);
        return new PagedResult<CodeGenerationTemplateDto>(templates.Select(ToDto).ToList(), total, pageIndex, pageSize);
    }

    public async Task<CodeGenerationTemplateDto> CreateTemplateAsync(CreateCodeGenerationTemplateRequest request, CancellationToken cancellationToken = default)
    {
        if (await freeSql.Select<CodeGenerationTemplate>().Where(x => x.DeletedAt == null && x.TemplateKey == request.TemplateKey).AnyAsync(cancellationToken))
        {
            throw new BusinessException("CONFLICT", "Template key already exists.");
        }

        var template = new CodeGenerationTemplate
        {
            Name = request.Name.Trim(),
            TemplateKey = request.TemplateKey.Trim(),
            TargetKind = request.TargetKind.Trim(),
            Content = request.Content
        };
        template.Id = await freeSql.Insert(template).ExecuteIdentityAsync(cancellationToken);
        await auditService.WriteAsync(nameof(CodeGenerationTemplate), template.Id.ToString(), "Create", null, template, cancellationToken);
        return ToDto(template);
    }

    private static CodeGenerationTemplateDto ToDto(CodeGenerationTemplate template)
    {
        return new CodeGenerationTemplateDto(template.Id, template.Name, template.TemplateKey, template.TargetKind, template.Content, template.Status, template.IsBuiltIn, template.CreatedAt);
    }
}
