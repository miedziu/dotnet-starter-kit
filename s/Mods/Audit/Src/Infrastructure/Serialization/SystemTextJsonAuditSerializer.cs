using FSH.Mods.Audit.Spec;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FSH.Mods.Audit.Infrastructure.Serialization;

public sealed class SystemTextJsonAuditSerializer : IAuditSerializer
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false
    };

    public string SerializePayload(object payload) => JsonSerializer.Serialize(payload, Opts);
}