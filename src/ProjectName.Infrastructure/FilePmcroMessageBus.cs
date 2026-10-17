namespace ProjectName.Infrastructure;

using System.Text.Json;
using ProjectName.Contracts;
using ProjectName.Core;

public sealed class FilePmcroMessageBus : IMessageBus
{
    private readonly string _root;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public FilePmcroMessageBus(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        _root = Path.GetFullPath(root);
        Directory.CreateDirectory(_root);
    }

    public void Publish(PmcroMessageEnvelope message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var safe = message with { Status = "queued", LeaseOwner = null, LeaseExpiresAt = null };
        var path = PathFor(safe.MessageId);
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(stream, safe, _json);
    }

    public PmcroMessageEnvelope? Peek(IReadOnlySet<string>? kinds = null, string? targetAgent = null) =>
        Eligible(Snapshot(), kinds, targetAgent).FirstOrDefault();

    public PmcroMessageEnvelope? Claim(string workerId, TimeSpan lease, IReadOnlySet<string>? kinds = null, string? targetAgent = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        foreach (var candidate in Eligible(Snapshot(), kinds, targetAgent))
        {
            var path = PathFor(candidate.MessageId);
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                var current = JsonSerializer.Deserialize<PmcroMessageEnvelope>(stream, _json);
                if (current is null || !IsEligible(current, kinds, targetAgent)) continue;
                var claimed = current with
                {
                    Status = "claimed",
                    LeaseOwner = workerId,
                    LeaseExpiresAt = DateTimeOffset.UtcNow.Add(lease),
                };
                stream.Position = 0;
                stream.SetLength(0);
                JsonSerializer.Serialize(stream, claimed, _json);
                return claimed;
            }
            catch (IOException)
            {
                // Another process owns the message; try the next one.
            }
        }
        return null;
    }

    public void Acknowledge(string messageId)
    {
        var path = PathFor(messageId);
        using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var current = JsonSerializer.Deserialize<PmcroMessageEnvelope>(stream, _json)
            ?? throw new InvalidDataException("message-invalid");
        var acknowledged = current with { Status = "acknowledged", LeaseExpiresAt = null };
        stream.Position = 0;
        stream.SetLength(0);
        JsonSerializer.Serialize(stream, acknowledged, _json);
    }

    public IReadOnlyList<PmcroMessageEnvelope> Snapshot() => Directory.EnumerateFiles(_root, "*.json")
        .Select(ReadSafe)
        .OfType<PmcroMessageEnvelope>()
        .OrderByDescending(m => m.Priority)
        .ThenBy(m => m.CreatedAt)
        .ToArray();

    private PmcroMessageEnvelope? ReadSafe(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return JsonSerializer.Deserialize<PmcroMessageEnvelope>(stream, _json);
        }
        catch (IOException) { return null; }
        catch (JsonException) { return null; }
    }

    private static IEnumerable<PmcroMessageEnvelope> Eligible(IEnumerable<PmcroMessageEnvelope> source, IReadOnlySet<string>? kinds, string? targetAgent) =>
        source.Where(m => IsEligible(m, kinds, targetAgent));

    private static bool IsEligible(PmcroMessageEnvelope message, IReadOnlySet<string>? kinds, string? targetAgent)
    {
        var available = message.Status == "queued" ||
            (message.Status == "claimed" && message.LeaseExpiresAt is { } expiry && expiry <= DateTimeOffset.UtcNow);
        return available && (kinds is null || kinds.Contains(message.Kind)) &&
            (targetAgent is null || StringComparer.Ordinal.Equals(targetAgent, message.TargetAgent));
    }

    private string PathFor(string messageId) => Path.Combine(_root, messageId + ".json");
}
