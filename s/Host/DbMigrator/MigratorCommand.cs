namespace FSH.Starter.DbMigrator;

/// <summary>
/// Lightweight command-line parser. Avoids dragging in System.CommandLine for
/// a handful of flags — keep this honest and minimal.
///
/// Verbs:   apply | seed | seed-demo | list-pending  (default: apply)
/// Flags:
///          --catalog-only   skip per-tenant migrations
///          --seed           after apply, also run SeedAsync per tenant
///          --help / -h      print help text
/// </summary>
internal sealed record MigratorCommand(
    string Command,
    bool CatalogOnly,
    bool SeedAfter,
    bool Help)
{
    private static readonly string[] KnownVerbs = ["apply", "seed", "seed-demo", "list-pending"];

    public static MigratorCommand Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var rawVerb = args.FirstOrDefault(a => !a.StartsWith('-')) ?? "apply";
        // Canonicalise to a known verb via OrdinalIgnoreCase match (CA1308 forbids
        // ToLowerInvariant for security-sensitive normalisation).
        var verb = KnownVerbs.FirstOrDefault(v => string.Equals(v, rawVerb, StringComparison.OrdinalIgnoreCase))
            ?? rawVerb;

        var catalogOnly = args.Any(a => string.Equals(a, "--catalog-only", StringComparison.OrdinalIgnoreCase));
        var seedAfter = args.Any(a => string.Equals(a, "--seed", StringComparison.OrdinalIgnoreCase));
        var help = args.Any(a => a is "-h" or "--help");

        return new MigratorCommand(verb, catalogOnly, seedAfter, help);
    }

    public const string HelpText = """
        FSH DbMigrator — apply EF Core migrations across all module databases.

        Usage:
          dotnet run --project src/Host/FSH.Starter.DbMigrator -- [verb] [options]

        Verbs:
          apply           Apply pending migrations (default). Use --seed to also run SeedAsync.
          seed            Run only the SeedAsync step.
          seed-demo       Provision the demo with users, tickets, and chat. Dev-only — refuses to run unless
                          DOTNET_ENVIRONMENT=Development.
          list-pending    Print pending migrations without applying anything.

        Options:
          --tenant <id>        Restrict to a single tenant id (default: all tenants).
          --catalog-only       Skip the per-tenant pass; only the tenant catalog is migrated.
          --seed               After apply, also run SeedAsync.
          -h, --help           Print this help text.

        Exit codes:
          0 — success
          1 — failure (see logged exception)
        """;
}