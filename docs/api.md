# API Draft

This document captures the MVP API surface described in `docs/technical-solution.md`. Final request and response DTOs should be generated from implementation or OpenAPI once the backend exists.

## Conventions

Base path:

```text
/api
```

Success response:

```json
{
  "code": "OK",
  "message": "success",
  "data": {}
}
```

Paged data:

```json
{
  "items": [],
  "total": 0,
  "pageIndex": 1,
  "pageSize": 20
}
```

Error response:

```json
{
  "code": "VALIDATION_ERROR",
  "message": "parameter validation failed",
  "errors": {
    "userName": ["user name is required"]
  }
}
```

Recommended error codes:

- `VALIDATION_ERROR`
- `UNAUTHORIZED`
- `FORBIDDEN`
- `NOT_FOUND`
- `CONFLICT`
- `BUSINESS_ERROR`
- `INTERNAL_ERROR`

## Authentication

JWT Bearer authentication is used for protected endpoints.

JWT claims:

- `sub`: user ID
- `name`: user name
- `display_name`: display name
- `roles`: role codes

Permissions should be queried on the server by user ID instead of storing all permission codes in the JWT.

Built-in roles:

- `super_admin`: server-side bypass for all permission checks.
- `readonly_admin`: example role for `user:view`, `role:view`, and `menu:view`.
- `user_admin`: example role for `user:view`, `user:create`, `user:update`, and `user:delete`.

The frontend may hide menus, pages, and buttons based on `/api/auth/me`, but this is display logic only. Page navigation is not a security boundary; protected backend APIs must enforce permissions.

## Auth APIs

### POST /api/auth/login

Login with user name or email and password.

Request:

```json
{
  "account": "admin",
  "password": "ChangeMe_123456"
}
```

Response data:

```json
{
  "accessToken": "jwt-token",
  "expiresAt": "2026-06-22T12:00:00Z"
}
```

### POST /api/auth/logout

Logs out the current session on the client side. The MVP does not require refresh tokens or token blacklists.

### GET /api/auth/me

Returns current user, role codes, permission codes, and menu tree.

Response data:

```json
{
  "id": 1,
  "userName": "admin",
  "displayName": "Administrator",
  "email": "admin@sun-admin.local",
  "roles": ["super_admin"],
  "permissions": ["user:create", "user:update"],
  "menus": []
}
```

### POST /api/auth/change-password

Request:

```json
{
  "oldPassword": "ChangeMe_123456",
  "newPassword": "NewStrongPassword_123"
}
```

## User APIs

| Method | Path | Permission | Description |
| --- | --- | --- | --- |
| GET | `/api/users` | `user:view` | Query users |
| GET | `/api/users/{id}` | `user:view` | Get user detail |
| POST | `/api/users` | `user:create` | Create user |
| PUT | `/api/users/{id}` | `user:update` | Update user |
| DELETE | `/api/users/{id}` | `user:delete` | Soft delete user |
| POST | `/api/users/{id}/enable` | `user:update` | Enable user |
| POST | `/api/users/{id}/disable` | `user:update` | Disable user |
| POST | `/api/users/{id}/reset-password` | `user:update` | Reset password |
| PUT | `/api/users/{id}/roles` | `user:update` | Assign roles |

Notes:

- User name and email should be unique.
- Built-in administrator accounts should be protected by business rules.
- Deleting users should use soft delete.

## Role APIs

| Method | Path | Permission | Description |
| --- | --- | --- | --- |
| GET | `/api/roles` | `role:view` | Query roles |
| GET | `/api/roles/{id}` | `role:view` | Get role detail |
| POST | `/api/roles` | `role:create` | Create role |
| PUT | `/api/roles/{id}` | `role:update` | Update role |
| DELETE | `/api/roles/{id}` | `role:delete` | Delete role |
| PUT | `/api/roles/{id}/menus` | `role:update` | Assign menus and permissions |

Notes:

- Role code should be unique.
- Built-in `super_admin` cannot be deleted.
- MVP should prevent deleting roles that are already assigned to users.

## Menu APIs

| Method | Path | Permission | Description |
| --- | --- | --- | --- |
| GET | `/api/menus/tree` | `menu:view` | Get menu tree |
| GET | `/api/menus/{id}` | `menu:view` | Get menu detail |
| POST | `/api/menus` | `menu:create` | Create menu or permission item |
| PUT | `/api/menus/{id}` | `menu:update` | Update menu or permission item |
| DELETE | `/api/menus/{id}` | `menu:delete` | Delete menu or permission item |

Menu types:

- `Directory`: sidebar directory.
- `Page`: sidebar page with route and component mapping.
- `Button`: button or API permission item with a required `PermissionCode`.

Notes:

- API permissions should reuse `PermissionCode`, such as `user:create`.
- Menu tree returned to the frontend should include only `Directory` and `Page`.
- Button permissions should be returned as permission codes.
- Menus with children cannot be deleted.
- Built-in system menus cannot be deleted.

## Authorization Rule

Protected APIs should declare permissions in code, for example:

```csharp
[RequirePermission("user:create")]
```

Authorization flow:

1. Read the current user ID from JWT claims.
2. If the user has role code `super_admin`, allow access.
3. Query enabled permissions through user-role and role-menu relations.
4. Compare the required permission code.
5. Return 403 when permission is missing.

Return 401 when authentication is missing or invalid. Return 403 when authentication succeeds but the required permission is absent.
