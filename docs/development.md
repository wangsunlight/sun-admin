# Development Guide

This guide follows the MVP scope in `docs/technical-solution.md`: .NET 8 Web API, FreeSql, MySQL, JWT authentication, RBAC permissions, and a React + Vite frontend.

## Prerequisites

- .NET SDK 8
- Node.js 20 or later
- Docker and Docker Compose
- MySQL 8 when not using Docker Compose

## Expected Project Layout

```text
src/
  SunAdmin.Api/
  SunAdmin.Application/
  SunAdmin.Contracts/
  SunAdmin.Domain/
  SunAdmin.Infrastructure/
tests/
  SunAdmin.UnitTests/
  SunAdmin.IntegrationTests/
web/
  sun-admin-web/
docs/
```

Backend boundaries:

- `SunAdmin.Api`: controllers, authentication, authorization, middleware, OpenAPI, health checks.
- `SunAdmin.Application`: use cases, validators, permission checks, business orchestration.
- `SunAdmin.Contracts`: request DTOs, response DTOs, pagination, unified response models.
- `SunAdmin.Domain`: entities, enums, constants, domain rules.
- `SunAdmin.Infrastructure`: FreeSql setup, repositories, seed data, JWT, password hashing, logging.

The API layer should not query FreeSql directly, and database entities should not be returned as API response models.

## Local Configuration

Create a local environment file:

```bash
cp .env.example .env
```

Important variables:

- `ConnectionStrings__Default`: MySQL connection string used by the API.
- `Database__SyncStructure`: enables FreeSql CodeFirst table synchronization.
- `Database__SeedData`: enables initial role, admin user, menu, and permission seed data.
- `Jwt__Secret`: JWT signing secret. Use a strong value with at least 32 characters.
- `Seed__AdminUserName`, `Seed__AdminEmail`, `Seed__AdminPassword`: initial administrator account.

Local development may enable `Database__SyncStructure=true`. Production should normally set it to `false`.

Seed data creates one administrator account only. It also creates built-in roles for verification:

- `super_admin`: full backend access.
- `readonly_admin`: view users, roles, and menus.
- `user_admin`: manage users only.

Do not add a default normal user to seed data. Create temporary users manually when testing role assignments.

## Run With Docker Compose

```bash
docker compose up --build
```

Expected services:

- `sun-admin-api`
- `sun-admin-web`
- `sun-admin-mysql`

Expected endpoints:

- API: `http://localhost:5000`
- Health: `http://localhost:5000/health`
- Readiness: `http://localhost:5000/health/ready`
- Web: `http://localhost:5173`

The compose file builds the backend and frontend images from:

- `src/SunAdmin.Api/Dockerfile`
- `web/sun-admin-web/Dockerfile`

The Web container serves static assets through nginx and proxies `/api` to the API container.

## Backend Development

Typical backend commands:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/SunAdmin.Api
```

Recommended implementation order:

1. Create solution and projects.
2. Configure FreeSql, MySQL, Swagger or Scalar, Serilog, unified response, exception handling, and FluentValidation.
3. Add entities for user, role, menu, user-role, and role-menu.
4. Add CodeFirst structure synchronization and seed data.
5. Add JWT login, current user, password change, and permission authorization.
6. Add users, roles, and menus APIs.
7. Add integration tests using Testcontainers MySQL.

## Frontend Development

Typical commands after the frontend app exists:

```bash
cd web/sun-admin-web
npm install
npm run dev
npm run build
```

The frontend should request `/api/auth/me` after login and use the returned user, role codes, permission codes, and menu tree to build the sidebar and control buttons.

Button permissions should use permission codes such as:

- `user:create`
- `user:update`
- `user:delete`
- `role:update`
- `menu:create`

Frontend checks are for visibility and ergonomics only. They hide menus and buttons, but they must not be treated as access control for pages or APIs. Backend `[RequirePermission]` checks are the final RBAC boundary.

## RBAC Verification

Use this flow after `docker compose up --build`:

1. Login as the configured administrator, for example `admin` / `ChangeMe_123456` in the default compose setup.
2. Create a temporary user with a non-default password.
3. Assign `readonly_admin` and verify:
   - `GET /api/users`, `GET /api/roles`, and `GET /api/menus/tree` return 200.
   - `POST /api/users`, `POST /api/roles`, and `POST /api/menus` return 403.
4. Assign `user_admin` and verify:
   - user view/create/update/delete APIs are allowed.
   - role and menu write APIs return 403.
5. Assign `super_admin` and verify protected user, role, and menu APIs are allowed.

The backend permission codes are:

- Users: `user:view`, `user:create`, `user:update`, `user:delete`.
- Roles: `role:view`, `role:create`, `role:update`, `role:delete`.
- Menus: `menu:view`, `menu:create`, `menu:update`, `menu:delete`.

## Testing Expectations

Unit tests should cover:

- Password hashing and verification.
- Permission tree and menu tree construction.
- Built-in role and built-in menu protection rules.
- Business rules such as preventing deletion of menus with children.

Integration tests should cover:

- Successful and failed login.
- Disabled users cannot login.
- Missing token returns 401.
- Missing permission returns 403.
- `super_admin` can access protected APIs.
- Core user, role, and menu CRUD flows.

Integration tests should use MySQL through Testcontainers to avoid behavior differences from other databases.
