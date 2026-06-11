using Rockets.Api.Models;

namespace Rockets.Api.Services;

public interface IMessageParser {
	public ParsedRocketMessage Parse(MessageEnvelope envelope);
}