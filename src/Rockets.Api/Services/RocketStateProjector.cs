using Rockets.Api.Models;

namespace Rockets.Api.Services;

internal static class RocketStateProjector {
	public static RocketState? Apply(RocketState? state, ParsedRocketMessage parsed) {
		switch (parsed.Message) {
			case RocketLaunchedMessage launched:
				return new RocketState {
					Id = parsed.Metadata.Channel,
					IsLaunched = true,
					LastMessageNumber = parsed.Metadata.MessageNumber,
					RocketType = launched.Type,
					Speed = launched.LaunchSpeed,
					Mission = launched.Mission
				};

			case RocketSpeedIncreasedMessage increased:
				if (state is null || state.IsTerminated) {
					return null;
				}

				state.Speed += increased.By;
				state.LastMessageNumber = parsed.Metadata.MessageNumber;
				return state;

			case RocketSpeedDecreasedMessage decreased:
				if (state is null || state.IsTerminated) {
					return null;
				}

				state.Speed -= decreased.By;
				state.LastMessageNumber = parsed.Metadata.MessageNumber;
				return state;

			case RocketMissionChangedMessage changed:
				if (state is null || state.IsTerminated) {
					return null;
				}

				state.Mission = changed.NewMission;
				state.LastMessageNumber = parsed.Metadata.MessageNumber;
				return state;

			case RocketExplodedMessage exploded:
				if (state is null || state.IsTerminated) {
					return null;
				}

				state.IsTerminated = true;
				state.ExplosionReason = exploded.Reason;
				state.LastMessageNumber = parsed.Metadata.MessageNumber;
				return state;

			default:
				throw new InvalidOperationException("Unknown message type");
		}
	}
}