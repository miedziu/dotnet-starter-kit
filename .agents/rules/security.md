# Web security & request governance

CORS, security headers, rate limiting, idempotency. See `modules/identity.md` for auth/JWT/permissions.

## CORS (`Web/Cors/`)

Policy `FSHCorsPolicy`. When `CorsOptions.AllowAll=true`: `SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials()` — **NOT** `AllowAnyOrigin()`. `Access-Control-Allow-Origin: *` is illegal with credentialed requests; SignalR's negotiate runs credentialed, so `AllowAnyOrigin()` silently breaks SignalR. `UseHeroCors()` runs **before** `UseHttpsRedirection()` so OPTIONS isn't 307-redirected.

## Security headers (`Web/Security/`)

`UseHeroSecurityHeaders()` sets `X-Content-Type-Options`, `X-Frame-Options: DENY`, `Referrer-Policy`, HSTS, CSP. `SecurityHeadersOptions.ExcludedPaths` defaults to `["/scalar","/openapi"]` — keep excluded.

## Rate limiting (`Web/RateLimiting/`)

Chained partitioned fixed-window: **user → IP** (defaults 1000 / 200 / 300 per 60s) + stricter `"auth"` policy (10/60s). Health paths unlimited. Rejection → 429 + ProblemDetails + `Retry-After`. `RateLimitingOptions.Enabled` read eagerly — when false, middleware skipped.

## Idempotency (`Web/Idempotency/`)

Opt-in with **`.WithIdempotency()`**. Reads `Idempotency-Key` header (max 128 chars, 24h TTL); replays return cached response with `Idempotency-Replayed: true`. Use on replay-safe POSTs.