# Deployment Guide

sun-admin MVP deployment is based on Docker Compose with three services:

- `sun-admin-api`
- `sun-admin-web`
- `mysql`

The database is MySQL. Do not replace it with another database unless the technical solution is updated first.

## Environment Files

Start from the example file:

```bash
cp .env.example .env
```

Required production changes:

- Set a strong `MYSQL_ROOT_PASSWORD`.
- Set a strong `MYSQL_PASSWORD`.
- Set `Jwt__Secret` to a long random value.
- Set `Seed__AdminPassword` to a strong initial password.
- Set `ASPNETCORE_ENVIRONMENT=Production`.
- Set `Database__SyncStructure=false` unless a release explicitly requires CodeFirst synchronization.
- Confirm `Database__SeedData=true` only when seed initialization is needed.

## Build And Start

```bash
docker compose --env-file .env up --build -d
```

Check service status:

```bash
docker compose ps
docker compose logs -f api
docker compose logs -f web
docker compose logs -f mysql
```

Health endpoints:

- `GET /health`
- `GET /health/ready`

## Ports

Default local ports:

- API: `5000:8080`
- Web: `5173:80`
- MySQL: `3306:3306`

For production, expose the web service through a reverse proxy and keep MySQL private to the deployment network whenever possible.

## Database Initialization

The backend is expected to initialize the database on startup:

1. Synchronize table structure when `Database__SyncStructure=true`.
2. Create the built-in `super_admin` role.
3. Create example built-in roles `readonly_admin` and `user_admin`.
4. Create the initial administrator account.
5. Create built-in system menus and permission codes.
6. Bind the administrator to `super_admin`.
7. Bind `super_admin` to all menus and permissions.
8. Bind `readonly_admin` to user, role, and menu view permissions.
9. Bind `user_admin` to user management permissions.

The seed process does not create a default normal user. Create temporary users manually when validating role assignments.

Production recommendation:

- Use `Database__SyncStructure=false`.
- Keep `Database__SeedData=true` for idempotent seed checks if the seed implementation is safe to rerun.
- Back up MySQL before upgrading.

## Image Build

The compose file builds images from:

- Backend Dockerfile: `src/SunAdmin.Api/Dockerfile`
- Frontend Dockerfile: `web/sun-admin-web/Dockerfile`

If published images are introduced later, replace the `build` sections with explicit `image` tags and versioned image names.

## Upgrade Checklist

1. Review release notes and database changes.
2. Back up MySQL.
3. Pull or build new images.
4. Apply environment variable changes.
5. Start services.
6. Verify `/health/ready`.
7. Login with an administrator account.
8. Validate user, role, menu, and permission flows.

## Security Notes

- Never commit `.env`.
- Rotate default administrator credentials immediately.
- Use HTTPS at the reverse proxy or ingress layer.
- Keep JWT secrets different across environments.
- Do not expose MySQL publicly.
- Treat frontend menu and button filtering as display logic only. Backend API authorization is the final permission boundary.
