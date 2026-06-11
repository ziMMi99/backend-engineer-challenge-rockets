using System.Text.Json;
using Rockets.Api.Models;

namespace Rockets.Api.Services;

public class MessageParser : IMessageParser
{
	private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	public ParsedRocketMessage Parse(MessageEnvelope envelope)
	{
		ParsedRocketMessage parsedMessage = new ParsedRocketMessage {
			Metadata = envelope.Metadata,
			Message = envelope.Metadata.MessageType switch
			{
				"RocketLaunched" => envelope.Message.Deserialize<RocketLaunchedMessage>(SerializerOptions)!,
				"RocketSpeedIncreased" => envelope.Message.Deserialize<RocketSpeedIncreasedMessage>(SerializerOptions)!,
				"RocketSpeedDecreased" => envelope.Message.Deserialize<RocketSpeedDecreasedMessage>(SerializerOptions)!,
				"RocketMissionChanged" => envelope.Message.Deserialize<RocketMissionChangedMessage>(SerializerOptions)!,
				"RocketExploded" => envelope.Message.Deserialize<RocketExplodedMessage>(SerializerOptions)!,
				_ => throw new InvalidOperationException("Unknown message type")
			}
		};

		return parsedMessage;
	}
}
