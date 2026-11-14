namespace NINA.Plugin.TargetScheduler.Discord.Models;

public sealed class TargetProgress {
    public string ProjectName { get; init; } = string.Empty;
    public string TargetName { get; init; } = string.Empty;
    public bool Active { get; init; }
    public double PercentComplete { get; init; }
    public IReadOnlyList<FilterProgress> Filters { get; init; } = Array.Empty<FilterProgress>();
}

public sealed class FilterProgress {
    public string FilterName { get; init; } = string.Empty;
    public int Desired { get; init; }
    public int Accepted { get; init; }
    public int Acquired { get; init; }
}
