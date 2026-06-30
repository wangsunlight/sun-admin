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
        var templateKey = request.TemplateKey.Trim();
        if (await freeSql.Select<CodeGenerationTemplate>().Where(x => x.DeletedAt == null && x.TemplateKey == templateKey).AnyAsync(cancellationToken))
        {
            throw new BusinessException("CONFLICT", "Template key already exists.");
        }

        var template = new CodeGenerationTemplate
        {
            Name = request.Name.Trim(),
            TemplateKey = templateKey,
            TargetKind = request.TargetKind.Trim(),
            Content = request.Content
        };
        template.Id = await freeSql.Insert(template).ExecuteIdentityAsync(cancellationToken);
        await auditService.WriteAsync(nameof(CodeGenerationTemplate), template.Id.ToString(), "Create", null, template, cancellationToken);
        return ToDto(template);
    }

    public async Task<CodeGenerationTemplateDto> UpdateTemplateAsync(long id, UpdateCodeGenerationTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = await GetEntityAsync(id, cancellationToken);
        if (template.IsBuiltIn && request.Status != template.Status)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in template status cannot be changed.");
        }

        var before = Clone(template);
        template.Name = request.Name.Trim();
        template.TargetKind = request.TargetKind.Trim();
        template.Content = request.Content;
        template.Status = request.Status;
        template.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<CodeGenerationTemplate>().SetSource(template).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(CodeGenerationTemplate), template.Id.ToString(), "Update", before, template, cancellationToken);
        return ToDto(template);
    }

    public async Task DeleteTemplateAsync(long id, CancellationToken cancellationToken = default)
    {
        var template = await GetEntityAsync(id, cancellationToken);
        if (template.IsBuiltIn)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in template cannot be deleted.");
        }

        var before = Clone(template);
        template.TemplateKey = BuildDeletedUniqueValue(template.TemplateKey, template.Id);
        template.DeletedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<CodeGenerationTemplate>().SetSource(template).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(CodeGenerationTemplate), template.Id.ToString(), "Delete", before, null, cancellationToken);
    }

    private async Task<CodeGenerationTemplate> GetEntityAsync(long id, CancellationToken cancellationToken)
    {
        return await freeSql.Select<CodeGenerationTemplate>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "Template not found.");
    }

    private static CodeGenerationTemplateDto ToDto(CodeGenerationTemplate template)
    {
        return new CodeGenerationTemplateDto(template.Id, template.Name, template.TemplateKey, template.TargetKind, template.Content, template.Status, template.IsBuiltIn, template.CreatedAt);
    }

    private static CodeGenerationTemplate Clone(CodeGenerationTemplate value)
    {
        return new CodeGenerationTemplate
        {
            Id = value.Id,
            Name = value.Name,
            TemplateKey = value.TemplateKey,
            TargetKind = value.TargetKind,
            Content = value.Content,
            Status = value.Status,
            IsBuiltIn = value.IsBuiltIn,
            CreatedAt = value.CreatedAt,
            UpdatedAt = value.UpdatedAt,
            DeletedAt = value.DeletedAt
        };
    }

    private static string BuildDeletedUniqueValue(string value, long id)
    {
        var suffix = $"__deleted_{id}";
        const int maxLength = 128;
        var baseLength = Math.Max(1, maxLength - suffix.Length);
        var normalized = value.Length > baseLength ? value[..baseLength] : value;
        return normalized + suffix;
    }
}
