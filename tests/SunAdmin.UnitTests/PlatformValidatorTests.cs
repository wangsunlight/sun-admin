using SunAdmin.Api.Validation;
using SunAdmin.Application.Menus;
using SunAdmin.Contracts.CodeGeneration;
using SunAdmin.Contracts.Exports;
using SunAdmin.Contracts.Files;
using SunAdmin.Contracts.Notifications;
using SunAdmin.Domain.Constants;
using SunAdmin.Domain.Enums;

namespace SunAdmin.UnitTests;

public sealed class PlatformValidatorTests
{
    [Fact]
    public void FileResourceValidator_RejectsInvalidMetadata()
    {
        var validator = new CreateFileResourceRequestValidator();
        var result = validator.Validate(new CreateFileResourceRequest(
            string.Empty,
            string.Empty,
            -1,
            "local",
            string.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateFileResourceRequest.OriginalFileName));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateFileResourceRequest.SizeBytes));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateFileResourceRequest.StoragePath));
    }

    [Fact]
    public void ExportTaskValidator_RequiresJsonObjectParameters()
    {
        var validator = new CreateExportTaskRequestValidator();
        var invalid = validator.Validate(new CreateExportTaskRequest("Users", "users", "[1,2,3]"));
        var valid = validator.Validate(new CreateExportTaskRequest("Users", "users", "{\"status\":\"Enabled\"}"));

        Assert.False(invalid.IsValid);
        Assert.True(valid.IsValid);
    }

    [Fact]
    public void NotificationValidator_RequiresExpiryAfterPublishTime()
    {
        var validator = new CreateNotificationRequestValidator();
        var result = validator.Validate(new CreateNotificationRequest(
            "维护通知",
            "今晚维护",
            NotificationLevel.Info,
            new DateTime(2026, 6, 29, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 6, 29, 11, 0, 0, DateTimeKind.Utc),
            false));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CodeGenerationTemplateValidator_RejectsUnsafeTemplateKey()
    {
        var validator = new CreateCodeGenerationTemplateRequestValidator();
        var result = validator.Validate(new CreateCodeGenerationTemplateRequest(
            "模板",
            "1 invalid key",
            "backend",
            "content"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateCodeGenerationTemplateRequest.TemplateKey));
    }

    [Fact]
    public void SystemPageRegistry_ContainsPlatformWritePermissions()
    {
        var permissions = SystemPageRegistry.Buttons
            .Select(x => x.PermissionCode)
            .Where(x => x is not null)
            .ToHashSet();

        Assert.Contains(SystemPermissionCodes.NotificationDelete, permissions);
        Assert.Contains(SystemPermissionCodes.CodeGenerationUpdate, permissions);
        Assert.Contains(SystemPermissionCodes.CodeGenerationDelete, permissions);
    }
}
