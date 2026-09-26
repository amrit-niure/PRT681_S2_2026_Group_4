namespace TicketingApi.Workflows;

/// <summary>Input for <see cref="TicketEmailWorkflow"/>: one email about one ticket.</summary>
public record TicketEmailRequest(int TicketId, string To, string Subject, string Body);
