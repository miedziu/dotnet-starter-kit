# Module: Identity

Auth (JWT + ASP.NET Identity), users, roles, permissions, sessions, impersonation, 2FA.

## Service shape

`IUserService` is a **facade** delegating to focused services — change behavior in specific service, not facade:

| Interface | Concern |
|---|---|
| `IUserRegistrationService` | register, external-principal create, email/phone confirm |
| `IUserProfileService` | get/list/count, update profile, image, existence checks |
| `IUserStatusService` | activate/deactivate (`DeleteAsync` == deactivate), audited toggles |
| `IUserRoleService` | role assignment, admin-role guards |
| `IUserPasswordService` | forgot/reset/change password, history + expiry |
| `IUserPermissionService` | effective permissions, cache invalidation |

`ChangePassword`/`Update`/`Delete` flow: facade → service → EF/UserManager. `CancellationToken` = `default` on interfaces; propagated to EF sinks (note: `UserManager`/`RoleManager` have no CT overloads).

## Permission gating footgun

`RequiredPermissionAttribute` implements `IRequiredPermissionMetadata`. **Never let a second/duplicate appear** — it silently disables **all** `.RequirePermission()` gates. Permission constants in `Shared/Identity/*Permissions.cs`.

## Hosted services (background)

- `RolePermissionSyncHostedService` — best-effort permission catalog sync; loops with `OperationCanceledException` filter, logs and continues.
- `SessionCleanupHostedService` — hourly expired-session purge; OCE handled by preceding catch.

Model for background loops: stay alive, log with context, never swallow cancellation.

## Tokens / sessions

Login `POST /api/v1/identity/token/issue` (header `X-FSH-App` enforces operator app boundary). Refresh `POST /api/v1/identity/token/refresh` cross-checks subject. Session rows written best-effort during login — failures log warning, login succeeds. Admin can't demote/deactivate last admin or seed admin (guards in `UserRoleService`/`UserStatusService`).