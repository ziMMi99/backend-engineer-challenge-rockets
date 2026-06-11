using System.Collections.Generic;
using Rockets.Api.Models;

namespace Rockets.Api.Services;

internal sealed class RocketStreamAggregate {
	private readonly object _gate = new();
	private readonly SortedDictionary<int, ParsedRocketMessage> _pendingMessages = new();

	public RocketState? CurrentState { get; private set; }
	public int NextExpectedMessageNumber { get; private set; } = 1;

	public RocketStreamAggregate() {
	}

	public RocketStreamAggregate(RocketState state) {
		CurrentState = state;
		NextExpectedMessageNumber = state.LastMessageNumber + 1;
	}

	public RocketState? SnapshotState() {
		lock (_gate) {
			return Clone(CurrentState);
		}
	}

	public void ReplaceState(RocketState state) {
		lock (_gate) {
			CurrentState = Clone(state);
			NextExpectedMessageNumber = state.LastMessageNumber + 1;

			if (state.IsTerminated) {
				_pendingMessages.Clear();
			}
		}
	}

	public void Process(ParsedRocketMessage parsed) {
		lock (_gate) {
			if (CurrentState?.IsTerminated == true) {
				return;
			}

			int messageNumber = parsed.Metadata.MessageNumber;

			if (messageNumber < NextExpectedMessageNumber) {
				return;
			}

			if (messageNumber > NextExpectedMessageNumber) {
				if (!_pendingMessages.ContainsKey(messageNumber)) {
					_pendingMessages.Add(messageNumber, parsed);
				}

				return;
			}

			ApplyAndDrain(parsed);
		}
	}

	private void ApplyAndDrain(ParsedRocketMessage parsed) {
		if (!TryApply(parsed)) {
			return;
		}

		if (CurrentState?.IsTerminated == true) {
			_pendingMessages.Clear();
			return;
		}

		NextExpectedMessageNumber++;

		while (_pendingMessages.TryGetValue(NextExpectedMessageNumber, out ParsedRocketMessage? pending)) {
			_pendingMessages.Remove(NextExpectedMessageNumber);

			if (!TryApply(pending)) {
				return;
			}

			if (CurrentState?.IsTerminated == true) {
				_pendingMessages.Clear();
				return;
			}

			NextExpectedMessageNumber++;
		}
	}

	private bool TryApply(ParsedRocketMessage parsed) {
		RocketState? updated = RocketStateProjector.Apply(CurrentState, parsed);
		if (updated is null) {
			return false;
		}

		CurrentState = updated;

		if (updated.IsTerminated) {
			_pendingMessages.Clear();
		}

		return true;
	}

	private static RocketState? Clone(RocketState? state) {
		if (state is null) {
			return null;
		}

		return new RocketState {
			Id = state.Id,
			IsLaunched = state.IsLaunched,
			IsTerminated = state.IsTerminated,
			Mission = state.Mission,
			RocketType = state.RocketType,
			Speed = state.Speed,
			LastMessageNumber = state.LastMessageNumber,
			ExplosionReason = state.ExplosionReason
		};
	}
}