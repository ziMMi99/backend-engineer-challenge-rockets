using Rockets.Api.Models;

namespace Rockets.Api.Services;

public interface IRocketStateService {

	RocketState? Get(Guid id);
	IReadOnlyCollection<RocketState> GetAll();
	void Upsert(RocketState state);
	void Process(ParsedRocketMessage parsedMessage);
}
