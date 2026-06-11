using Rockets.Api.Models;

namespace Rockets.Api.Services;

public class MessageHandler : IMessageHandler
{
	private readonly IMessageParser _parser;
	private readonly IRocketStateService _rocketStateService;

	public MessageHandler(IMessageParser parser, IRocketStateService rocketStateService)
	{
		_parser = parser;
		_rocketStateService = rocketStateService;
	}

	public void Handle(MessageEnvelope envelope)
	{
		ParsedRocketMessage parsedRocketMessage = _parser.Parse(envelope);
		_rocketStateService.Process(parsedRocketMessage);
	}
}
