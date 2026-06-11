using System.Text.Json;
using Rockets.Api.Models;
using Rockets.Api.Services;

namespace Rockets.Api.Tests;

internal static class Program {
	private static readonly Guid RocketId = Guid.Parse("193270a9-c9cf-404a-8f83-838e71d9ae67");

	private static int Main() {
		Run("out_of_order_messages_are_buffered_and_drained_in_order", OutOfOrderMessagesAreBufferedAndDrainedInOrder);
		Run("duplicate_delivery_is_ignored", DuplicateDeliveryIsIgnored);
		Run("late_redelivery_is_ignored", LateRedeliveryIsIgnored);
		Run("messages_before_launch_wait_until_launch_arrives", MessagesBeforeLaunchWaitUntilLaunchArrives);
		Run("explosion_marks_terminal_and_ignores_later_messages", ExplosionMarksTerminalAndIgnoresLaterMessages);

		Console.WriteLine("All rocket buffering tests passed.");
		return 0;
	}

	private static void OutOfOrderMessagesAreBufferedAndDrainedInOrder() {
		MessageHandler handler = CreateHandler(out IRocketStateService store);

		Handle(handler, 1, "RocketLaunched", new { type = "Falcon-9", launchSpeed = 500, mission = "ARTEMIS" });
		Handle(handler, 3, "RocketMissionChanged", new { newMission = "SHUTTLE_MIR" });
		Handle(handler, 2, "RocketSpeedIncreased", new { by = 3000 });

		RocketState state = RequireState(store);

		AssertEqual(3, state.LastMessageNumber, "last message number");
		AssertEqual(3500d, state.Speed, "speed");
		AssertEqual("SHUTTLE_MIR", state.Mission, "mission");
	}

	private static void DuplicateDeliveryIsIgnored() {
		MessageHandler handler = CreateHandler(out IRocketStateService store);

		Handle(handler, 1, "RocketLaunched", new { type = "Falcon-9", launchSpeed = 500, mission = "ARTEMIS" });
		Handle(handler, 2, "RocketSpeedIncreased", new { by = 3000 });
		Handle(handler, 2, "RocketSpeedIncreased", new { by = 3000 });

		RocketState state = RequireState(store);

		AssertEqual(2, state.LastMessageNumber, "last message number");
		AssertEqual(3500d, state.Speed, "speed");
	}

	private static void LateRedeliveryIsIgnored() {
		MessageHandler handler = CreateHandler(out IRocketStateService store);

		Handle(handler, 1, "RocketLaunched", new { type = "Falcon-9", launchSpeed = 500, mission = "ARTEMIS" });
		Handle(handler, 2, "RocketSpeedIncreased", new { by = 3000 });
		Handle(handler, 2, "RocketSpeedIncreased", new { by = 3000 });
		Handle(handler, 3, "RocketMissionChanged", new { newMission = "LATE_REDDELIVERY" });
		Handle(handler, 2, "RocketSpeedIncreased", new { by = 3000 });

		RocketState state = RequireState(store);

		AssertEqual(3, state.LastMessageNumber, "last message number");
		AssertEqual(3500d, state.Speed, "speed");
		AssertEqual("LATE_REDDELIVERY", state.Mission, "mission");
	}

	private static void MessagesBeforeLaunchWaitUntilLaunchArrives() {
		MessageHandler handler = CreateHandler(out IRocketStateService store);

		Handle(handler, 2, "RocketSpeedIncreased", new { by = 3000 });
		Handle(handler, 3, "RocketMissionChanged", new { newMission = "SHUTTLE_MIR" });

		AssertNull(store.Get(RocketId), "state should not exist before launch");

		Handle(handler, 1, "RocketLaunched", new { type = "Falcon-9", launchSpeed = 500, mission = "ARTEMIS" });

		RocketState state = RequireState(store);

		AssertEqual(3, state.LastMessageNumber, "last message number");
		AssertEqual(3500d, state.Speed, "speed");
		AssertEqual("SHUTTLE_MIR", state.Mission, "mission");
	}

	private static void ExplosionMarksTerminalAndIgnoresLaterMessages() {
		MessageHandler handler = CreateHandler(out IRocketStateService store);

		Handle(handler, 1, "RocketLaunched", new { type = "Falcon-9", launchSpeed = 500, mission = "ARTEMIS" });
		Handle(handler, 2, "RocketSpeedIncreased", new { by = 3000 });
		Handle(handler, 3, "RocketExploded", new { reason = "PRESSURE_VESSEL_FAILURE" });
		Handle(handler, 4, "RocketMissionChanged", new { newMission = "AFTER_EXPLOSION" });

		RocketState state = RequireState(store);

		AssertTrue(state.IsTerminated, "rocket should be terminal");
		AssertEqual("PRESSURE_VESSEL_FAILURE", state.ExplosionReason, "explosion reason");
		AssertEqual(3, state.LastMessageNumber, "last message number");
		AssertEqual(3500d, state.Speed, "speed");
		AssertEqual("ARTEMIS", state.Mission, "mission");
	}

	private static MessageHandler CreateHandler(out IRocketStateService store) {
		store = new RocketStateService();
		return new MessageHandler(new MessageParser(), store);
	}

	private static void Handle(MessageHandler handler, int messageNumber, string messageType, object payload) {
		handler.Handle(new MessageEnvelope {
			Metadata = new MessageMetadata {
				Channel = RocketId,
				MessageNumber = messageNumber,
				MessageTime = DateTimeOffset.UtcNow,
				MessageType = messageType
			},
			Message = JsonSerializer.SerializeToElement(payload)
		});
	}

	private static RocketState RequireState(IRocketStateService store) {
		RocketState? state = store.Get(RocketId);
		if (state is null) {
			throw new InvalidOperationException("Expected rocket state to exist.");
		}

		return state;
	}

	private static void Run(string name, Action test) {
		try {
			test();
			Console.WriteLine($"PASS: {name}");
		}
		catch (Exception ex) {
			Console.Error.WriteLine($"FAIL: {name}");
			Console.Error.WriteLine(ex);
			Environment.ExitCode = 1;
			throw;
		}
	}

	private static void AssertEqual<T>(T expected, T actual, string label) {
		if (!EqualityComparer<T>.Default.Equals(expected, actual)) {
			throw new InvalidOperationException($"Expected {label} to be '{expected}', but was '{actual}'.");
		}
	}

	private static void AssertNull(object? value, string label) {
		if (value is not null) {
			throw new InvalidOperationException($"Expected {label} to be null.");
		}
	}

	private static void AssertTrue(bool condition, string label) {
		if (!condition) {
			throw new InvalidOperationException($"Expected {label} to be true.");
		}
	}
}