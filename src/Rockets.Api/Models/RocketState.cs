namespace Rockets.Api.Models;

public class RocketState {
	
	public Guid Id { get; init; }
	public bool IsLaunched { get; set; }
	public bool IsTerminated { get; set; }
	public string? Mission { get; set; }
	public string? RocketType { get; set; }
	public double Speed { get; set; }
	public int LastMessageNumber { get; set; }
	public string? ExplosionReason { get; set; }
}
