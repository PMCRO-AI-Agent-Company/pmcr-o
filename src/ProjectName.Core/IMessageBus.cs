namespace ProjectName.Core;

using ProjectName.Contracts;

public interface IMessageBus
{
    void Publish(PmcroMessageEnvelope message);
    PmcroMessageEnvelope? Peek(IReadOnlySet<string>? kinds = null, string? targetAgent = null);
    PmcroMessageEnvelope? Claim(string workerId, TimeSpan lease, IReadOnlySet<string>? kinds = null, string? targetAgent = null);
    void Acknowledge(string messageId);
    IReadOnlyList<PmcroMessageEnvelope> Snapshot();
}
