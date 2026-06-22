# sun-admin

sun-admin is a lightweight, reusable admin system template based on .NET 8 and React. The MVP focuses on authentication, RBAC, user management, role management, menu management, API permission checks, and a deployable local development baseline.

## Tech Stack

Backend:

- .NET 8 + ASP.NET Core Web API
- C# 12 with nullable reference types
- FreeSql + MySQL
- JWT Bearer Authentication
- Custom permission authorization policy
- FluentValidation
- Serilog
- Swagger or Scalar
- xUnit + WebApplicationFactory + Testcontainers MySQL

Frontend:

- React + TypeScript + Vite
- Ant Design
- React Router
- TanStack Query or ahooks
- Axios

Deployment:

- Docker
- Docker Compose
- MySQL

## Quick Start

The repository is expected to use this structure:

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

1. Copy environment variables:

```bash
cp .env.example .env
```

2. Review `.env` and set strong local secrets, especially `JWT_SECRET` and `SEED_ADMIN_PASSWORD`.

3. Start the full stack:

```bash
docker compose up --build
```

Expected local endpoints:

- API: `http://localhost:5000`
- API health: `http://localhost:5000/health`
- API readiness: `http://localhost:5000/health/ready`
- Web: `http://localhost:5173`
- MySQL: `localhost:3306`

The Web container serves the Vite build through nginx and proxies `/api` to the API container.

## Default Account

Initial administrator values are configured through environment variables:

- Username: `admin`
- Email: `admin@sun-admin.local`
- Password: `ChangeMe_123456`

Change the default password before using any shared or production-like environment.

## Built-in Roles And RBAC Check

Seed initialization creates these built-in roles:

- `super_admin`: full backend API access through the server-side super administrator bypass.
- `readonly_admin`: example role with read-only access to users, roles, and menus.
- `user_admin`: example role with user management permissions only.

No extra default normal user is created. To verify RBAC from a fresh environment:

1. Login with the seeded administrator account and change its password.
2. Create a temporary test user.
3. Assign `readonly_admin`, then verify user/role/menu list APIs work while create/update/delete APIs return 403.
4. Assign `user_admin`, then verify user management APIs work while role and menu write APIs return 403.
5. Assign `super_admin`, then verify protected user, role, and menu APIs are allowed.

Permission codes used by the backend are `user:view`, `user:create`, `user:update`, `user:delete`, `role:view`, `role:create`, `role:update`, `role:delete`, `menu:view`, `menu:create`, `menu:update`, and `menu:delete`.

The frontend only limits menu and button visibility. It does not enforce page access. Backend API authorization is the final permission boundary.

## Directory Overview

```text
.
├── docs/                         Documentation
├── src/                          Backend solution and projects
│   ├── SunAdmin.Api/             Controllers, middleware, auth, OpenAPI
│   ├── SunAdmin.Application/     Use cases, validators, application services
│   ├── SunAdmin.Contracts/       Request/response DTOs and shared API models
│   ├── SunAdmin.Domain/          Entities, enums, domain constants and rules
│   └── SunAdmin.Infrastructure/  FreeSql, repositories, seed data, JWT, logging
├── tests/                        Unit and integration tests
├── web/sun-admin-web/            React admin frontend
├── docker-compose.yml            Local full-stack orchestration
└── .env.example                  Environment variable template
```

## Documentation

- [Technical solution](docs/technical-solution.md)
- [Development guide](docs/development.md)
- [Deployment guide](docs/deployment.md)
- [API draft](docs/api.md)
