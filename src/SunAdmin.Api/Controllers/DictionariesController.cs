using Microsoft.AspNetCore.Mvc;
using SunAdmin.Api.Security;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Dictionaries;
using SunAdmin.Domain.Constants;

namespace SunAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DictionariesController(IDictionaryService dictionaryService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(SystemPermissionCodes.DictionaryView)]
    public async Task<ActionResult<ApiResponse<PagedResult<DictionaryDto>>>> GetPage([FromQuery] DictionaryQuery query, CancellationToken cancellationToken)
    {
        return ApiResponse<PagedResult<DictionaryDto>>.Ok(await dictionaryService.GetPageAsync(query, cancellationToken));
    }

    [HttpGet("{id:long}")]
    [RequirePermission(SystemPermissionCodes.DictionaryView)]
    public async Task<ActionResult<ApiResponse<DictionaryDto>>> Get(long id, CancellationToken cancellationToken)
    {
        var dictionary = await dictionaryService.GetAsync(id, cancellationToken);
        return dictionary is null ? NotFound(ApiResponse<DictionaryDto>.Fail("NOT_FOUND", "Dictionary not found.")) : ApiResponse<DictionaryDto>.Ok(dictionary);
    }

    [HttpPost]
    [RequirePermission(SystemPermissionCodes.DictionaryCreate)]
    public async Task<ActionResult<ApiResponse<DictionaryDto>>> Create(CreateDictionaryRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<DictionaryDto>.Ok(await dictionaryService.CreateAsync(request, cancellationToken));
    }

    [HttpPut("{id:long}")]
    [RequirePermission(SystemPermissionCodes.DictionaryUpdate)]
    public async Task<ActionResult<ApiResponse<DictionaryDto>>> Update(long id, UpdateDictionaryRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<DictionaryDto>.Ok(await dictionaryService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(SystemPermissionCodes.DictionaryDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(long id, CancellationToken cancellationToken)
    {
        await dictionaryService.DeleteAsync(id, cancellationToken);
        return ApiResponse.Ok();
    }

    [HttpPost("{dictionaryId:long}/items")]
    [RequirePermission(SystemPermissionCodes.DictionaryUpdate)]
    public async Task<ActionResult<ApiResponse<DictionaryItemDto>>> CreateItem(long dictionaryId, UpsertDictionaryItemRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<DictionaryItemDto>.Ok(await dictionaryService.UpsertItemAsync(dictionaryId, null, request, cancellationToken));
    }

    [HttpPut("{dictionaryId:long}/items/{itemId:long}")]
    [RequirePermission(SystemPermissionCodes.DictionaryUpdate)]
    public async Task<ActionResult<ApiResponse<DictionaryItemDto>>> UpdateItem(long dictionaryId, long itemId, UpsertDictionaryItemRequest request, CancellationToken cancellationToken)
    {
        return ApiResponse<DictionaryItemDto>.Ok(await dictionaryService.UpsertItemAsync(dictionaryId, itemId, request, cancellationToken));
    }

    [HttpDelete("{dictionaryId:long}/items/{itemId:long}")]
    [RequirePermission(SystemPermissionCodes.DictionaryDelete)]
    public async Task<ActionResult<ApiResponse>> DeleteItem(long dictionaryId, long itemId, CancellationToken cancellationToken)
    {
        await dictionaryService.DeleteItemAsync(dictionaryId, itemId, cancellationToken);
        return ApiResponse.Ok();
    }
}
