namespace TicketingApi.Workflows;

public record TicketEmailRequest(int TicketId, string To, string Subject, string Body);
