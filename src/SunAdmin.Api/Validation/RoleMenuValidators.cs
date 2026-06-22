using System.Linq.Expressions;
using FluentValidation;
using SunAdmin.Contracts.Menus;
using SunAdmin.Contracts.Roles;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Api.Validation;

public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(64);
    }
}

public sealed class CreateMenuRequestValidator : AbstractValidator<CreateMenuRequest>
{
    public CreateMenuRequestValidator()
    {
        Include(new MenuRequestRules<CreateMenuRequest>(
            request => request.Name,
            request => request.Type,
            request => request.RoutePath,
            request => request.Component,
            request => request.Icon,
            request => request.PermissionCode));
    }
}

public sealed class UpdateMenuRequestValidator : AbstractValidator<UpdateMenuRequest>
{
    public UpdateMenuRequestValidator()
    {
        Include(new MenuRequestRules<UpdateMenuRequest>(
            request => request.Name,
            request => request.Type,
            request => request.RoutePath,
            request => request.Component,
            request => request.Icon,
            request => request.PermissionCode));
    }
}

internal sealed class MenuRequestRules<T> : AbstractValidator<T>
{
    public MenuRequestRules(
        Expression<Func<T, string>> name,
        Expression<Func<T, MenuType>> type,
        Expression<Func<T, string?>> routePath,
        Expression<Func<T, string?>> component,
        Expression<Func<T, string?>> icon,
        Expression<Func<T, string?>> permissionCode)
    {
        var getType = type.Compile();

        RuleFor(name).NotEmpty().MaximumLength(64);
        RuleFor(routePath).MaximumLength(256);
        RuleFor(component).MaximumLength(256);
        RuleFor(icon).MaximumLength(64);
        RuleFor(permissionCode).MaximumLength(128);
        RuleFor(routePath)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .When(request => getType(request) == MenuType.Page)
            .WithMessage("Page menu routePath is required.");
        RuleFor(permissionCode)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .When(request => getType(request) == MenuType.Button)
            .WithMessage("Button menu permissionCode is required.");
    }
}
