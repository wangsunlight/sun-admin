using Microsoft.AspNetCore.Mvc;
using SunAdmin.Api.Security;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.CodeGeneration;
using SunAdmin.Contracts.Common;
using SunAdmin.Domain.Constants;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/code-generation")]
public sealed class CodeGenerationController(ICodeGenerationService codeGenerationService) : ControllerBase
{
    [HttpGet("templates")]
    [RequirePermission(SystemPermissionCodes.CodeGenerationView)]
    public async Task<ActionResult<ApiResponse<PagedResult<CodeGenerationTemplateDto>>>> GetTemplates([FromQuery] CodeGenerationTemplateQuery query, CancellationToken cancellationToken)
    {
        return ApiResponse<PagedResult<CodeGenerationTemplateDto>>.Ok(await codeGenerationService.GetTemplatesAsync(query, cancellationToken));
    }

    [HttpPost("templates")]
    [RequirePermission(SystemPermissionCodes.CodeGenerationCreate)]
    public async Task<ActionResult<ApiResponse<CodeGenerationTemplateDto>>> CreateTemplate(CreateCodeGenerationTemplateRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<CodeGenerationTemplateDto>.Ok(await codeGenerationService.CreateTemplateAsync(request, cancellationToken));
    }
}
