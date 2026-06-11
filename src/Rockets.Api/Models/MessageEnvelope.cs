using System.Text.Json;

namespace Rockets.Api.Models;

public class MessageEnvelope {
	public MessageMetadata Metadata { get; set; }
	public JsonElement Message { get; set; }
}

public class MessageMetadata  {
	public Guid Channel { get; set; }
	public int MessageNumber { get; set; }
	public DateTimeOffset MessageTime { get; set; }
	public string MessageType { get; set; } = string.Empty;
}
