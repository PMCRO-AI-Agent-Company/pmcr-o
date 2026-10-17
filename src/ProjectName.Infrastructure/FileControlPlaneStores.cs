using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ProjectName.Contracts;

namespace ProjectName.Infrastructure;

public abstract class FileStoreBase
{
    protected readonly string Root;
    protected static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    protected FileStoreBase(string root)
    {
        Root = Path.GetFullPath(root);
    }

    protected string PathFor(string id)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(id))).ToLowerInvariant();
        return Path.Combine(Root, hash + ".json");
    }

    protected void WriteAtomic(string path, object value, bool mutable = false)
    {
        Directory.CreateDirectory(Root);
        var json = JsonSerializer.Serialize(value, value.GetType(), JsonOptions);
        if (!mutable && File.Exists(path))
        {
            if (File.ReadAllText(path) == json) return;
            throw new InvalidOperationException("immutable-record-conflict");
        }
        var staging = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(staging, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                stream.Write(bytes);
                stream.Flush(true);
            }
            if (mutable && File.Exists(path))
            {
                // Replace atomically and retain the previous committed version as the backup.
                var history = Path.Combine(Root, "history");
                Directory.CreateDirectory(history);
                var backup = Path.Combine(history, Path.GetFileNameWithoutExtension(path) + "." + Guid.NewGuid().ToString("N") + ".json");
                File.Replace(staging, path, backup, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(staging, path);
            }
        }
        finally { if (File.Exists(staging)) File.Delete(staging); }
    }
}

public sealed class FileCapabilityRegistrationStore : FileStoreBase, ICapabilityRegistrationStore
{
    public FileCapabilityRegistrationStore(string root) : base(root) { }

    public CapabilityRegistration? Get(string registrationId)
    {
        var path = PathFor(registrationId);
        return File.Exists(path) ? JsonSerializer.Deserialize<CapabilityRegistration>(File.ReadAllText(path)) : null;
    }

    public void Save(CapabilityRegistration registration)
    {
        Directory.CreateDirectory(Root);
        WriteAtomic(PathFor(registration.RegistrationId), registration);
    }
}

public sealed class FileGrantStore : FileStoreBase, IGrantStore
{
    public FileGrantStore(string root) : base(root) { }

    public AgentExecutionGrant? Get(string grantId)
    {
        var path = PathFor(grantId);
        return File.Exists(path) ? JsonSerializer.Deserialize<AgentExecutionGrant>(File.ReadAllText(path)) : null;
    }

    public void Save(AgentExecutionGrant grant)
    {
        Directory.CreateDirectory(Root);
        if (string.IsNullOrWhiteSpace(grant.GrantId)) throw new InvalidOperationException("grant-id-required");
        WriteAtomic(PathFor(grant.GrantId), grant);
    }
}

public sealed class FileRevocationStore : FileStoreBase, IRevocationStore
{
    public FileRevocationStore(string root) : base(root) { }

    public bool IsRevoked(string targetType, string targetId) => File.Exists(PathFor(targetType + ":" + targetId));

    public void Save(RevocationRecord revocation)
    {
        Directory.CreateDirectory(Root);
        var key = revocation.TargetType + ":" + revocation.TargetId;
        WriteAtomic(PathFor(key), revocation);
    }
}

public sealed class FileActivationStore : FileStoreBase, IActivationStore
{
    public FileActivationStore(string root) : base(root) { }

    public ActivationRecord? Get(string frameworkId)
    {
        var path = PathFor(frameworkId);
        return File.Exists(path) ? JsonSerializer.Deserialize<ActivationRecord>(File.ReadAllText(path)) : null;
    }

    public void Save(ActivationRecord activation)
    {
        Directory.CreateDirectory(Root);
        if (string.IsNullOrWhiteSpace(activation.FrameworkId)) throw new InvalidOperationException("framework-id-required");
        WriteAtomic(PathFor(activation.FrameworkId), activation, mutable: true);
    }
}
