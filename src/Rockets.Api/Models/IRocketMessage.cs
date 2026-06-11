namespace Rockets.Api.Models;

public interface IRocketMessage { }

public sealed record RocketLaunchedMessage(string Type, int LaunchSpeed, string Mission) : IRocketMessage;
public sealed record RocketSpeedIncreasedMessage(int By) : IRocketMessage;
public sealed record RocketSpeedDecreasedMessage(int By) : IRocketMessage;
public sealed record RocketMissionChangedMessage(string NewMission) : IRocketMessage;
public sealed record RocketExplodedMessage(string Reason) : IRocketMessage;