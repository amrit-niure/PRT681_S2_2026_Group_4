namespace TicketingApi.Workflows;

/// <summary>Temporal connection settings, bound from the "Temporal" section.</summary>
public class TemporalOptions
{
    public const string SectionName = "Temporal";

    /// <summary>host:port of the Temporal frontend (gRPC).</summary>
    public string Address { get; set; } = "localhost:7233";

    public string Namespace { get; set; } = "default";

    public string TaskQueue { get; set; } = "ticket-emails";
}
