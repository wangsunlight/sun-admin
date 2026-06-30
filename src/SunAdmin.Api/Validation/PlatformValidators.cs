using System.Text.Json;
using FluentValidation;
using SunAdmin.Contracts.CodeGeneration;
using SunAdmin.Contracts.Dictionaries;
using SunAdmin.Contracts.Exports;
using SunAdmin.Contracts.Files;
using SunAdmin.Contracts.Notifications;

namespace SunAdmin.Api.Validation;

public sealed class CreateDictionaryRequestValidator : AbstractValidator<CreateDictionaryRequest>
{
    public CreateDictionaryRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(64)
            .Matches("^[A-Za-z][A-Za-z0-9_:-]*$")
            .WithMessage("Dictionary code must start with a letter and contain only letters, digits, _, :, or -.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Description).MaximumLength(256);
    }
}

public sealed class UpdateDictionaryRequestValidator : AbstractValidator<UpdateDictionaryRequest>
{
    public UpdateDictionaryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Description).MaximumLength(256);
    }
}

public sealed class UpsertDictionaryItemRequestValidator : AbstractValidator<UpsertDictionaryItemRequest>
{
    public UpsertDictionaryItemRequestValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Value).NotEmpty().MaximumLength(128);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateNotificationRequestValidator : AbstractValidator<CreateNotificationRequest>
{
    public CreateNotificationRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
        RuleFor(x => x)
            .Must(x => !x.PublishAt.HasValue || !x.ExpiresAt.HasValue || x.ExpiresAt.Value > x.PublishAt.Value)
            .WithMessage("ExpiresAt must be later than PublishAt.");
    }
}

public sealed class UpdateNotificationRequestValidator : AbstractValidator<UpdateNotificationRequest>
{
    public UpdateNotificationRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(4000);
        RuleFor(x => x)
            .Must(x => !x.PublishAt.HasValue || !x.ExpiresAt.HasValue || x.ExpiresAt.Value > x.PublishAt.Value)
            .WithMessage("ExpiresAt must be later than PublishAt.");
    }
}

public sealed class CreateFileResourceRequestValidator : AbstractValidator<CreateFileResourceRequest>
{
    public CreateFileResourceRequestValidator()
    {
        RuleFor(x => x.OriginalFileName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(128);
        RuleFor(x => x.SizeBytes).GreaterThan(0).LessThanOrEqualTo(500L * 1024 * 1024);
        RuleFor(x => x.StorageProvider).MaximumLength(64);
        RuleFor(x => x.StoragePath).NotEmpty().MaximumLength(1024);
    }
}

public sealed class CreateExportTaskRequestValidator : AbstractValidator<CreateExportTaskRequest>
{
    public CreateExportTaskRequestValidator()
    {
        RuleFor(x => x.TaskName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.ExportType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.ParametersJson)
            .MaximumLength(8000)
            .Must(BeJsonObject)
            .When(x => !string.IsNullOrWhiteSpace(x.ParametersJson))
            .WithMessage("ParametersJson must be a valid JSON object.");
    }

    private static bool BeJsonObject(string? value)
    {
        try
        {
            using var document = JsonDocument.Parse(value!);
            return document.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

public sealed class CreateCodeGenerationTemplateRequestValidator : AbstractValidator<CreateCodeGenerationTemplateRequest>
{
    public CreateCodeGenerationTemplateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.TemplateKey)
            .NotEmpty()
            .MaximumLength(128)
            .Matches("^[A-Za-z][A-Za-z0-9_.:-]*$")
            .WithMessage("TemplateKey must start with a letter and contain only letters, digits, _, ., :, or -.");
        RuleFor(x => x.TargetKind).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(20000);
    }
}

public sealed class UpdateCodeGenerationTemplateRequestValidator : AbstractValidator<UpdateCodeGenerationTemplateRequest>
{
    public UpdateCodeGenerationTemplateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.TargetKind).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(20000);
    }
}
