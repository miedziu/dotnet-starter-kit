using FSH.Framework.Core.Context;
using FSH.Mod.Audit.Spec;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Security.Claims;

namespace FSH.Mod.Audit.Core;

/// <summary>
/// Ambient-aware audit scope. Prefers HTTP context when present (the
/// common path), and falls back to the
/// <see cref="ICurrentUser"/> for non-HTTP execution (Hangfire jobs,
/// background workers). The fallback path is what attributes
/// entity-change audits captured by <c>AuditingSaveChangesInterceptor</c>
/// when EF Core flushes from a Hangfire-driven service scope.
///
/// The class name is preserved for compatibility with existing DI
/// registrations; the behaviour is now broader than its name suggests.
/// </summary>
public sealed class HttpAuditScope : IAuditScope
{
    private readonly IHttpContextAccessor _http;
    private readonly ICurrentUser? _currentUser;

    public HttpAuditScope(
        IHttpContextAccessor httpContextAccessor,
        ICurrentUser? currentUser = null)
    {
        _http = httpContextAccessor;
        _currentUser = currentUser;
    }

    public string? UserId =>
        _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _http.HttpContext?.User?.FindFirstValue("sub")
        ?? NullIfEmpty(_currentUser?.GetUserId().ToString());

    public string? UserName =>
        _http.HttpContext?.User?.Identity?.Name
        ?? _http.HttpContext?.User?.FindFirstValue("name")
        ?? _currentUser?.Name;

    // Activity.Current is populated by both ASP.NET Core (HTTP) and the
    // FshJobActivator (Hangfire), so this works in both contexts.
    public string? TraceId => Activity.Current?.TraceId.ToString();
    public string? SpanId => Activity.Current?.SpanId.ToString();

    public string? CorrelationId =>
        _http.HttpContext?.TraceIdentifier
        ?? Activity.Current?.RootId;

    public string? RequestId =>
        _http.HttpContext?.TraceIdentifier
        ?? Activity.Current?.Id;

    public string? Source =>
        _http.HttpContext?.GetEndpoint()?.DisplayName
        // Background path: the activator names the activity after the job method
        // (e.g. "MonthlyInvoiceJob.RunAsync"); a stable source key when no HTTP endpoint is in scope.
        ?? Activity.Current?.OperationName
        ?? "background";

    public AuditTag Tags => AuditTag.None;

    public IAuditScope WithTags(AuditTag tags) => this; // immutable view
    public IAuditScope WithProperties(string? userId = null, string? userName = null, string? traceId = null,
        string? spanId = null, string? correlationId = null, string? requestId = null, string? source = null, AuditTag? tags = null) => this;

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrEmpty(s) || s == Guid.Empty.ToString() ? null : s;
}