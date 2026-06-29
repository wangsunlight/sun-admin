using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Common;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Files;
using SunAdmin.Domain.Entities;

namespace SunAdmin.Infrastructure.Services;

public sealed class FileResourceService(IFreeSql freeSql, ICurrentUser currentUser, IEntityAuditService auditService) : IFileResourceService
{
    public async Task<PagedResult<FileResourceDto>> GetPageAsync(FileQuery query, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var selector = freeSql.Select<FileResource>().Where(x => x.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            selector = selector.Where(x => x.FileName.Contains(query.Keyword) || x.OriginalFileName.Contains(query.Keyword) || x.StoragePath.Contains(query.Keyword));
        }

        var total = await selector.CountAsync(cancellationToken);
        var files = await selector.OrderByDescending(x => x.Id).Page(pageIndex, pageSize).ToListAsync(cancellationToken);
        return new PagedResult<FileResourceDto>(files.Select(ToDto).ToList(), total, pageIndex, pageSize);
    }

    public async Task<FileResourceDto> CreateAsync(CreateFileResourceRequest request, CancellationToken cancellationToken = default)
    {
        var file = new FileResource
        {
            FileName = $"{Guid.NewGuid():N}-{request.OriginalFileName}",
            OriginalFileName = request.OriginalFileName.Trim(),
            ContentType = request.ContentType.Trim(),
            SizeBytes = request.SizeBytes,
            StorageProvider = string.IsNullOrWhiteSpace(request.StorageProvider) ? "local" : request.StorageProvider.Trim(),
            StoragePath = request.StoragePath.Trim(),
            UploadedBy = currentUser.UserId
        };
        file.Id = await freeSql.Insert(file).ExecuteIdentityAsync(cancellationToken);
        await auditService.WriteAsync(nameof(FileResource), file.Id.ToString(), "Create", null, file, cancellationToken);
        return ToDto(file);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var file = await freeSql.Select<FileResource>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "File not found.");
        file.DeletedAt = DateTime.UtcNow;
        await freeSql.Update<FileResource>().SetSource(file).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(FileResource), file.Id.ToString(), "Delete", file, null, cancellationToken);
    }

    private static FileResourceDto ToDto(FileResource file)
    {
        return new FileResourceDto(file.Id, file.FileName, file.OriginalFileName, file.ContentType, file.SizeBytes, file.StorageProvider, file.StoragePath, file.UploadedBy, file.CreatedAt);
    }
}
