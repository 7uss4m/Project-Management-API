# Task & Project Management API - Architecture

## Layer Structure

This solution follows a clean layered design to keep business rules independent from delivery and persistence concerns.

- `TaskManager.API` - controllers, middleware, OpenAPI (Swashbuckle) + Scalar UI, JWT token creation (`IJwtTokenService`), HTTP-scoped current user (`ICurrentUserService`), DI wiring for host-only concerns
- `TaskManager.Application` - use cases (`IUserService`, `IProjectService`, `ITaskService`, `IAuthService`), ownership-aware business rules, `ReportEngine` + `IReportGenerator` strategies, `IReportService` (authorization before report generation)
- `TaskManager.Domain` - entities, enums, domain exceptions, repository interfaces, no framework dependencies
- `TaskManager.Infrastructure` - EF Core `DbContext`, entity configurations, repository implementations, SQLite; registers JWT bearer authentication alongside the database (shared hosting configuration)

## Core Design Principles

- Deliberate REST semantics (resource-oriented URLs, correct status codes, consistent errors)
- Strict separation of concerns between API, application logic, domain model, and data access
- Report engine extensibility through Strategy pattern (`IReportGenerator` per report type)
- One-command local startup (`dotnet run`) with SQLite migrations applied at startup (skipped in `Testing` for integration tests)
- Interactive API docs via OpenAPI JSON + Scalar (Bearer security scheme for JWT)

## Decision Log

Each decision below is intentionally documented for reviewer traceability.

| Decision Area | Choice | Why This Choice |
|---|---|---|
| User management | Full User CRUD endpoints; `POST /users` requires `password` (min 8 chars), stored as BCrypt hash | Same as register/login: every creatable user can authenticate; avoids empty-password accounts. |
| Password hashing | BCrypt (`BCrypt.Net-Next`) | Strong adaptive hashing without pulling in full ASP.NET Core Identity for this scope. |
| User credentials storage | `User.PasswordHash` on entity + EF migration for existing SQLite rows (default empty string) | Keeps auth data in the same aggregate as profile fields; migration backfills safely for pre-auth databases. |
| JWT configuration | Symmetric key, issuer, audience, expiry from `JwtSettings` in configuration | Simple deployment model; production must replace development signing key. |
| Project visibility | `GET /projects` returns only projects where the caller is a member (`GetPagedForMemberUserAsync`) | Enforces tenancy at read time without leaking other users’ project names or ids in list responses. |
| Project creation | Creator is inserted as a `ProjectMember` with role `Owner` | Guarantees the creating user can always manage the project they just created. |
| Task visibility | Members see all tasks in a project; non-members may list only when `assigneeId` equals their user id; single-task access if member or assignee | Matches “team + assignee” collaboration without opening full project data to outsiders. |
| Report access | `IReportService` checks project membership when `parameters.projectId` is present, then delegates to `ReportEngine` | Reports cannot be used to aggregate data from projects the user does not belong to. |
| Auth endpoints | `POST /auth/register`, `POST /auth/login` (no `[Authorize]`) | Clear onboarding and token acquisition; all other controllers require a valid bearer token. |
| API documentation | Swashbuckle OpenAPI + Scalar at `/scalar`; spec at `/swagger/v1/swagger.json` | Modern explorer UI and standard import path for Postman and similar tools. |
| Delete semantics | Soft delete via `IsDeleted` | Preserves historical records and reduces accidental data loss while still hiding deleted data from normal reads. |
| Project delete behavior | Cascade soft delete to related tasks and project members | Prevents orphaned records and keeps project lifecycle deterministic. |
| Task updates | Focused endpoints + `PATCH` (`/status`, `/assignee`, and partial task patch) | Matches partial-update intent, reduces over-posting risk, and keeps update operations explicit. |
| Enum wire format | Accept/return enum names (e.g. `"InProgress"`) | Improves API readability and avoids client-side enum index coupling. |
| Collection responses | Include pagination metadata | Makes API scalable and predictable as data volume grows; avoids future breaking changes. |
| Filter validation | Invalid enum filters return `400 Bad Request` | Treats invalid filters as client input errors instead of silently ignoring bad input. |
| Assignee filter validation | Non-existent `assigneeId` returns `400 Bad Request` | Makes filters strict and explicit; prevents silent mismatches caused by invalid user references. |
| Report parameters | Typed DTO per report type | Gives strong validation and maintainability; avoids weakly typed parameter bags. |
| Part 2a first report | `tasks_by_status` | Clear baseline report with straightforward grouping, easy to verify correctness. |
| Part 2b AI extension | `tasks_by_priority` | Natural second strategy proving extensibility with minimal core changes. |
| Startup behavior | `dotnet run` + auto-migrate SQLite at startup | Meets assessment run requirement and reduces setup friction for reviewers. |
| Canonical error vocabulary | `validation_error`, `not_found`, `conflict`, `forbidden`, `unauthorized`, `unexpected_error` | Balances clarity and practicality for this API scope while remaining extensible. |
| Authentication / authorization | JWT bearer + service-layer checks (`ICurrentUserService`, membership / assignee rules) | Standard stateless API auth; keeps authorization next to business rules while controllers stay thin. |

## Authentication and Authorization (concise)

1. **Authentication** — ASP.NET Core JWT bearer validates the `Authorization` header. Claims include `sub` (user id) and are read by `ICurrentUserService` in the API layer.
2. **Authorization** — Controllers are mostly `[Authorize]` except `AuthController`. Fine-grained rules live in application services (`ProjectService`, `TaskService`, `ReportService`) using `IProjectRepository.IsMemberAsync` and task `AssigneeId`, throwing `ForbiddenException` when access is denied (mapped to **403** with `type: "forbidden"`).
3. **User identity in tests** — Integration tests seed a known user and use `CreateAuthenticatedClient()` to attach a valid JWT without hard-coding token strings.

## API Error Contract

All endpoint errors should return a consistent envelope:

```json
{
  "type": "not_found",
  "message": "Project with id 42 was not found",
  "errors": {}
}
```

This keeps error handling stable for clients and avoids leaking internal exception details.

Canonical `type` values used across the API:

- `validation_error`
- `not_found`
- `conflict`
- `forbidden`
- `unauthorized`
- `unexpected_error`

`unauthorized` is used for invalid login credentials (`POST /auth/login`). Missing or invalid bearer tokens are handled by the JWT middleware (**401** with the standard `WWW-Authenticate` challenge), not always through the JSON envelope above.

## Report Engine Extensibility

The report engine (`ReportEngine`) resolves report generators from DI by type key. New report types are implemented as new `IReportGenerator` classes and registered in DI, without modifying engine core dispatch logic. HTTP entry points use `IReportService`, which enforces project membership when a `projectId` is supplied, then calls `ReportEngine.GenerateAsync`.

Expected stable output contract for all report types:

- `reportType`
- `generatedAt`
- `labels[]`
- `series[]` (each with `name` and `data[]`)

## AI Extension

### Exact Prompt Used

> "Here are the core contracts of a report engine I built. Please implement a new report type called `tasks_by_priority` following the same pattern. The new class must implement `IReportGenerator` with `ReportType = "tasks_by_priority"`, use `ITaskRepository.GetAllForProjectAsync`, group tasks by each `Priority` enum value, and return a `ReportResult` in the same shape as the existing `TasksByStatusReport`."
>
> The following files were provided as context: `IReportGenerator.cs`, `ReportEngine.cs`, `ReportRequest.cs`, `ReportResult.cs`, `TasksByStatusReport.cs`.

### What AI Got Right / What Needed Fixing

**Got right:**
- Correctly implemented the `IReportGenerator` interface with the right method signature and `CancellationToken` parameter
- Used `Enum.GetValues<Priority>()` to enumerate all priority labels — consistent with how `TasksByStatusReport` enumerates statuses
- Used `ITaskRepository.GetAllForProjectAsync` for data access, matching the existing pattern exactly
- DI registration instruction was correct: `services.AddScoped<IReportGenerator, TasksByPriorityReport>()`
- Output shape (`Labels`, `Series[0].Data`) matched the contract without any prompting

**Fixed:**
- The AI made `ProjectId` optional with a fallback that queried all tasks across all projects. This was inconsistent with the baseline report, which treats a missing `ProjectId` as an argument error. Changed to throw `ArgumentException` when `ProjectId` is null, matching `TasksByStatusReport`'s behavior.
