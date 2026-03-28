namespace phonic.Models;

public class AudioDeviceItem
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public bool IsDefault { get; init; }
}
