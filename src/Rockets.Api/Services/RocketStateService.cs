using System.Collections.Concurrent;
using Rockets.Api.Models;

namespace Rockets.Api.Services;

public class RocketStateService : IRocketStateService {
	private readonly ConcurrentDictionary<Guid, RocketStreamAggregate> _streams = new();

	public RocketState? Get(Guid id) {
		if (!_streams.TryGetValue(id, out RocketStreamAggregate? stream)) {
			return null;
		}

		return stream.SnapshotState();
	}

	public IReadOnlyCollection<RocketState> GetAll() {
		return _streams.Values
			.Select(stream => stream.SnapshotState())
			.OfType<RocketState>()
			.OrderBy(state => state.Id)
			.ToList();
	}

	public void Upsert(RocketState state) {
		RocketStreamAggregate stream = _streams.GetOrAdd(state.Id, _ => new RocketStreamAggregate());
		stream.ReplaceState(state);
	}

	public void Process(ParsedRocketMessage parsedMessage) {
		RocketStreamAggregate stream =
			_streams.GetOrAdd(parsedMessage.Metadata.Channel, _ => new RocketStreamAggregate());
		stream.Process(parsedMessage);
	}
}