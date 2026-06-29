using SunAdmin.Domain.Enums;

namespace SunAdmin.Contracts.CodeGeneration;

public sealed record CodeGenerationTemplateQuery(
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null);

public sealed record CodeGenerationTemplateDto(
    long Id,
    string Name,
    string TemplateKey,
    string TargetKind,
    string Content,
    RecordStatus Status,
    bool IsBuiltIn,
    DateTime CreatedAt);

public sealed record CreateCodeGenerationTemplateRequest(
    string Name,
    string TemplateKey,
    string TargetKind,
    string Content);
