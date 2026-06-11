namespace Rockets.Api.Models;

public class ParsedRocketMessage {
	public MessageMetadata Metadata { get; set; }
	public IRocketMessage Message { get; set; }
}
