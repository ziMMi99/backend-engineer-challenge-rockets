using Rockets.Api.Models;

namespace Rockets.Api.Services;

public interface IMessageHandler {

	public void Handle(MessageEnvelope envelope);
}