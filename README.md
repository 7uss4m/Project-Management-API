# Task & Project Management API

A .NET 10 REST API for organizing work into projects, managing tasks, and assigning them to team members.

## Running Locally

```bash
dotnet run --project src/TaskManager.API
```

The API listens on the URLs in `src/TaskManager.API/Properties/launchSettings.json` (default **http://localhost:5068**). The SQLite database (`taskmanager.db`) is created and migrated automatically on first run — no setup required.

## Running Tests

```bash
dotnet test
```

## Authentication

Register or log in to obtain a JWT, then send `Authorization: Bearer {token}` on all other endpoints.

```http
POST /auth/register
Content-Type: application/json

{ "name": "Alice", "email": "alice@example.com", "password": "your-secure-password" }
```

```http
POST /auth/login
Content-Type: application/json

{ "email": "alice@example.com", "password": "your-secure-password" }
```

Response body includes `token`, `userId`, `name`, and `email`. Configure signing in `appsettings.json` under `JwtSettings` (replace `Key` in production).

OpenAPI / Scalar: use the **Authorize** / Bearer field with your token.

## Configuration & secrets

- **Database**: uses local SQLite (`taskmanager.db`) via `ConnectionStrings:DefaultConnection` in `appsettings*.json`. No cloud credentials are stored in the repo.
- **JWT keys**: `JwtSettings:Key` values in `appsettings.json`, `appsettings.Development.json`, and `appsettings.Testing.json` are **development/test-only**. For any real deployment:
  - Override `JwtSettings` via **environment variables** or a secrets store (e.g. Azure Key Vault, AWS Secrets Manager, or `dotnet user-secrets`).
  - Never commit production signing keys to git; rotate keys if a real key is ever exposed.
- **Test passwords**: hard-coded passwords (e.g. `Test123!`, `TestUserPw1!`) live only in test projects to seed users for integration tests and are not used by the running app in production.

## API Overview

| Resource | Base URL |
|---|---|
| Auth | `POST /auth/register`, `POST /auth/login` (no auth required) |
| Users | `GET/POST/PUT/DELETE /users` |
| Projects | `GET/POST/PUT/DELETE /projects` |
| Project Members | `POST /projects/{id}/members`, `DELETE /projects/{id}/members/{userId}` |
| Tasks | `GET/POST /projects/{id}/tasks`, `PATCH /projects/{id}/tasks/{taskId}` |
| Reports | `POST /reports` |

### Task Filtering

```
GET /projects/{id}/tasks?status=InProgress&priority=High&assigneeId=5
```

All filter parameters are optional and compose. Invalid enum values return `400 Bad Request`. A non-existent `assigneeId` returns `400 Bad Request`.

### Report Engine

```json
POST /reports
{
  "type": "tasks_by_status",
  "parameters": { "projectId": 1 }
}
```

Available types: `tasks_by_status`, `tasks_by_priority`

## Key Decisions

**SQLite** — Zero external dependencies. The database file is created automatically. Swap the connection string in `appsettings.json` to use PostgreSQL with no code changes.

**JWT authentication** — Bearer tokens issued on register/login; `JwtBearer` validates on each request. **Authorization** is enforced in application services: project access requires membership (creators are added as `Owner`); task access requires project membership or being the task assignee. Non-members may list tasks only when filtering by their own `assigneeId`.

**Password storage** — Passwords are hashed with BCrypt. `POST /users` and `POST /auth/register` both require a password (minimum 8 characters) so new accounts can sign in via `POST /auth/login`.

**Soft delete** — All deletes set `IsDeleted = true`. Deleting a project cascades soft-delete to its tasks and memberships in a single transaction. No data is permanently removed.

**`PATCH` for task updates** — Tasks use `PATCH` for partial updates, with focused sub-endpoints (`/status`, `/assignee`) for the most common single-field operations.

**Enum wire format** — The API accepts and returns enum names as strings (`"InProgress"`, `"High"`) rather than numeric values. This keeps responses readable and avoids client-side coupling to enum indices.

**Pagination on all collections** — Every list endpoint returns `{ items, page, pageSize, totalCount, totalPages }`. Added by design to prevent future breaking changes as data grows.

**Strategy pattern for reports** — Adding a new report type requires creating one new class and one DI registration. The engine core never changes. See `ARCHITECTURE.md` for the full design rationale.
