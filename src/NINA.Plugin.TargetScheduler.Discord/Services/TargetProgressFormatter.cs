using NINA.Plugin.TargetScheduler.Discord.Models;
using System.Text;

namespace NINA.Plugin.TargetScheduler.Discord.Services;

public static class TargetProgressFormatter {
    public const string EmbedTitle = "🔭 Target Scheduler Progress";

    private const int ProgressBarWidth = 10;

    public static string FormatMessage(IReadOnlyList<TargetProgress> targets, DateTimeOffset timestamp) {
        var builder = new StringBuilder();
        builder.AppendLine($"**{EmbedTitle}**");
        builder.AppendLine($"{timestamp:yyyy-MM-dd HH:mm} UTC");
        builder.AppendLine();

        if (targets.Count == 0) {
            builder.Append("_No active targets found._");
            return builder.ToString();
        }

        AppendOverview(builder, targets);
        builder.AppendLine();

        for (var index = 0; index < targets.Count; index++) {
            if (index > 0) {
                builder.AppendLine();
            }

            builder.Append(FormatTargetBlock(targets[index]));
        }

        return builder.ToString().TrimEnd();
    }

    public static string FormatTargetLine(TargetProgress target) => FormatTargetBlock(target);

    public static string FormatTargetBlock(TargetProgress target) {
        var percent = Math.Clamp(target.PercentComplete, 0d, 100d);
        var label = FormatLabel(target);
        var statusIcon = StatusIcon(percent, target.Filters);
        var builder = new StringBuilder();
        builder.AppendLine($"**🌌 {label}** {statusIcon}".TrimEnd());
        builder.AppendLine($"`{ProgressBar(percent)}` **{percent:0}%**");

        var filters = AggregateFilters(target.Filters).ToList();
        if (filters.Count == 0) {
            builder.Append("_no exposure plans_");
            return builder.ToString();
        }

        if (filters.All(filter => filter.Accepted == 0 && filter.Acquired == 0)) {
            builder.Append("_no frames yet_");
            return builder.ToString();
        }

        builder.Append(string.Join(" | ", filters.Select(FormatFilter)));

        var desired = filters.Sum(filter => filter.Desired);
        var accepted = filters.Sum(filter => filter.Accepted);
        if (desired > 0 && percent < 99.95) {
            builder.AppendLine();
            builder.Append($"**Accepted:** {accepted}/{desired}");
        }

        return builder.ToString();
    }

    public static int EmbedColor(IReadOnlyList<TargetProgress> targets) {
        if (targets == null || targets.Count == 0) {
            return 0x99AAB5;
        }

        var average = targets.Average(target => target.PercentComplete);
        if (average >= 99.95) {
            return 0x57F287;
        }

        if (average >= 50) {
            return 0xFEE75C;
        }

        if (average > 0) {
            return 0xE67E22;
        }

        return 0x99AAB5;
    }

    public static IReadOnlyList<string> SplitForDiscord(string content, int maxLength = 3900) {
        if (string.IsNullOrEmpty(content)) {
            return Array.Empty<string>();
        }

        if (content.Length <= maxLength) {
            return new[] { content };
        }

        var chunks = new List<string>();
        var blocks = content.Replace("\r\n", "\n").Split(new[] { "\n\n" }, StringSplitOptions.None);
        var current = new StringBuilder();

        foreach (var block in blocks) {
            var candidate = current.Length == 0 ? block : current + "\n\n" + block;
            if (candidate.Length <= maxLength) {
                if (current.Length > 0) {
                    current.Append("\n\n");
                }

                current.Append(block);
                continue;
            }

            if (current.Length > 0) {
                chunks.Add(current.ToString());
                current.Clear();
            }

            if (block.Length <= maxLength) {
                current.Append(block);
                continue;
            }

            foreach (var piece in SplitOversizedBlock(block, maxLength)) {
                chunks.Add(piece);
            }
        }

        if (current.Length > 0) {
            chunks.Add(current.ToString());
        }

        return chunks;
    }

    private static void AppendOverview(StringBuilder builder, IReadOnlyList<TargetProgress> targets) {
        var complete = targets.Count(target => target.PercentComplete >= 99.95);
        var inProgress = targets.Count(target => target.PercentComplete is > 0 and < 99.95);
        var empty = targets.Count - complete - inProgress;
        var average = targets.Average(target => target.PercentComplete);
        var desired = targets.Sum(target => AggregateFilters(target.Filters).Sum(filter => filter.Desired));
        var accepted = targets.Sum(target => AggregateFilters(target.Filters).Sum(filter => filter.Accepted));

        builder.AppendLine("**Overview**");
        builder.AppendLine($"Targets: {targets.Count}");
        builder.AppendLine($"Complete: {complete}");
        builder.AppendLine($"In progress: {inProgress}");
        builder.AppendLine($"Not started: {empty}");
        if (desired > 0) {
            builder.AppendLine($"Accepted / Desired: {accepted}/{desired}");
        }

        builder.Append($"`{ProgressBar(average)}` **{average:0}%** avg");
    }

    private static IEnumerable<string> SplitOversizedBlock(string block, int maxLength) {
        var lines = block.Split('\n');
        var current = new StringBuilder();
        foreach (var line in lines) {
            var candidate = current.Length == 0 ? line : current + "\n" + line;
            if (candidate.Length <= maxLength) {
                if (current.Length > 0) {
                    current.Append('\n');
                }

                current.Append(line);
                continue;
            }

            if (current.Length > 0) {
                yield return current.ToString();
                current.Clear();
            }

            if (line.Length <= maxLength) {
                current.Append(line);
            } else {
                for (var index = 0; index < line.Length; index += maxLength) {
                    yield return line.Substring(index, Math.Min(maxLength, line.Length - index));
                }
            }
        }

        if (current.Length > 0) {
            yield return current.ToString();
        }
    }

    private static string FormatLabel(TargetProgress target) {
        if (string.IsNullOrWhiteSpace(target.ProjectName)) {
            return target.TargetName;
        }

        if (string.Equals(target.ProjectName, target.TargetName, StringComparison.OrdinalIgnoreCase)) {
            return target.TargetName;
        }

        return $"{target.ProjectName} / {target.TargetName}";
    }

    private static string StatusIcon(double percent, IReadOnlyList<FilterProgress> filters) {
        if (percent >= 99.95) {
            return "✅";
        }

        if (filters.Count > 0 && filters.All(filter => filter.Accepted == 0 && filter.Acquired == 0)) {
            return "🌑";
        }

        if (percent > 0) {
            return "🔄";
        }

        return "🌑";
    }

    private static string ProgressBar(double percent) {
        var filled = (int)Math.Round(Math.Clamp(percent, 0d, 100d) / 100d * ProgressBarWidth);
        filled = Math.Clamp(filled, 0, ProgressBarWidth);
        return new string('█', filled) + new string('░', ProgressBarWidth - filled);
    }

    private static IEnumerable<FilterProgress> AggregateFilters(IReadOnlyList<FilterProgress> filters) {
        return filters
            .GroupBy(filter => filter.FilterName, StringComparer.OrdinalIgnoreCase)
            .Select(group => new FilterProgress {
                FilterName = group.First().FilterName,
                Desired = group.Sum(filter => filter.Desired),
                Accepted = group.Sum(filter => filter.Accepted),
                Acquired = group.Sum(filter => filter.Acquired)
            })
            .OrderBy(filter => filter.FilterName, StringComparer.OrdinalIgnoreCase);
    }

    private static string FormatFilter(FilterProgress filter) {
        if (filter.Acquired == filter.Accepted) {
            return $"{filter.FilterName}: {filter.Accepted}/{filter.Desired}";
        }

        return $"{filter.FilterName}: {filter.Accepted}/{filter.Desired} ({filter.Acquired} acq)";
    }
}
