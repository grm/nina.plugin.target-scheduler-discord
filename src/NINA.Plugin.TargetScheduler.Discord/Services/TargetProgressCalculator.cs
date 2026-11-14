using NINA.Plugin.TargetScheduler.Discord.Models;

namespace NINA.Plugin.TargetScheduler.Discord.Services;

public static class TargetProgressCalculator {
    public static TargetProgress FromExposurePlans(
        string projectName,
        string targetName,
        bool active,
        IEnumerable<(string FilterName, int Desired, int Accepted, int Acquired)> plans) {
        var filters = plans
            .Select(plan => new FilterProgress {
                FilterName = string.IsNullOrWhiteSpace(plan.FilterName) ? "?" : plan.FilterName.Trim(),
                Desired = Math.Max(0, plan.Desired),
                Accepted = Math.Max(0, plan.Accepted),
                Acquired = Math.Max(0, plan.Acquired)
            })
            .OrderBy(filter => filter.FilterName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var desired = filters.Sum(filter => filter.Desired);
        var accepted = filters.Sum(filter => Math.Min(filter.Accepted, filter.Desired == 0 ? filter.Accepted : filter.Desired));
        var percent = desired <= 0
            ? (filters.Count == 0 ? 0d : 100d)
            : Math.Min(100d, accepted * 100d / desired);

        return new TargetProgress {
            ProjectName = projectName ?? string.Empty,
            TargetName = targetName ?? string.Empty,
            Active = active,
            PercentComplete = percent,
            Filters = filters
        };
    }

    public static IReadOnlyList<TargetProgress> SortByProgress(IEnumerable<TargetProgress> targets) {
        return targets
            .OrderByDescending(target => target.PercentComplete)
            .ThenBy(target => target.ProjectName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(target => target.TargetName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
