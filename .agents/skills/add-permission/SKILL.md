---
name: add-permission
description: Add a new permission end-to-end — server constant + endpoint gate, and (admin app) mirror it into the permissions catalog + route guard. Use when a new endpoint needs authorization. See modules/identity.md .
argument-hint: [ModuleName] [Resource] [Action]
---

# Add Permission

A permission spans server + the admin app. The dashboard app does **not** mirror permissions — it reads
them from the JWT and relies on the server's 403.

## Step 1 — Server constant (`Modules.{X}.Contracts/Authorization/{X}Permissions.cs`)

Add the constant to the resource group and ensure it's in the module's `All` collection. Convention:
`Permissions.{Resource}.{Action}`.

```csharp
public static class {X}Permissions
{
    public static class {Resources}
    {
        public const string View   = "Permissions.{Resources}.View";
        public const string Create = "Permissions.{Resources}.Create";   // ← new
    }
    public static IReadOnlyList<FshPermission> All { get; } = [ /* … include the new one … */ ];
}
```

The module already calls `PermissionConstants.Register({X}Permissions.All)` in `ConfigureServices`, so a new entry in `All` is picked up automatically.

## Step 2 — Gate the endpoint

```csharp
.RequirePermission({X}Permissions.{Resources}.Create);
```

⚠️ `RequiredPermissionAttribute` implements `IRequiredPermissionMetadata`. **Never let a second/duplicate of that interface exist** — it silently disables **all** `.RequirePermission()` gates app-wide. (See `.agents/rules/modules/identity.md`.)

## Step 3 — (admin only) gate the route

```tsx
{ path: "{resources}/new",
  element: <RouteGuard perms={[{Module}Permissions.{Resources}.Create]}><Create{Resource}Page /></RouteGuard> },
```

## Dashboard

No mirror, no `RouteGuard`. The JWT carries only role names — the app fetches the permission list from `GET /api/v1/identity/permissions` at hydration and the server enforces access; a missing permission yields a 403 the UI surfaces. Routes aren't permission-gated; to hide a nav entry, set `perm`/`anyPerm` on the item in `src/components/layout/nav-data.ts`. Permission-gated specs mock `GET /identity/permissions` with the grants they need (shell mocks stub it to `[]`).

## Checklist

- [ ] Server constant added to `{X}Permissions` **and** its `All` collection
- [ ] Endpoint gated with `.RequirePermission(...)`; no duplicate `IRequiredPermissionMetadata`
- [ ] (admin) mirrored in `lib/permissions.ts` (+ `PERMISSION_CATALOG` if role-editor-visible)
- [ ] (admin) route wrapped in `<RouteGuard perms={[…]}>`; permission added to `ADMIN_PERMS` seed
