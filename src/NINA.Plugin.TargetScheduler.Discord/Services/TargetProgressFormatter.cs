using NINA.Plugin.TargetScheduler.Discord.Models;
using System.Text;

namespace NINA.Plugin.TargetScheduler.Discord.Services;

public static class TargetProgressFormatter {
    private const int ProgressBarWidth = 10;

    public static string FormatMessage(IReadOnlyList<TargetProgress> targets, DateTimeOffset timestamp) {
        var builder = new StringBuilder();
        builder.AppendLine($"**Target Scheduler progress** — {timestamp:yyyy-MM-dd HH:mm} UTC");

        if (targets.Count == 0) {
            builder.Append("_No active targets found._");
            return builder.ToString();
        }

        var complete = targets.Count(target => target.PercentComplete >= 99.95);
        var inProgress = targets.Count(target => target.PercentComplete is > 0 and < 99.95);
        builder.AppendLine($"_{targets.Count} target(s) · {complete} complete · {inProgress} in progress_");
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
        var bar = ProgressBar(percent);
        var label = FormatLabel(target);
        var header = $"`{bar}` **{percent:0}%** · **{label}**";

        var filters = AggregateFilters(target.Filters).ToList();
        if (filters.Count == 0) {
            return header + "\n_no exposure plans_";
        }

        if (filters.All(filter => filter.Accepted == 0 && filter.Acquired == 0)) {
            return header + "\n_no frames yet_";
        }

        if (percent >= 99.95 && filters.All(filter => filter.Desired <= 0 || filter.Accepted >= filter.Desired)) {
            var names = string.Join(" · ", filters.Select(filter => $"`{filter.FilterName}`"));
            return header + "\n" + names + " ✓";
        }

        return header + "\n" + string.Join(" · ", filters.Select(FormatFilter));
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

    private static string ProgressBar(double percent) {
        var filled = (int)Math.Round(percent / 100d * ProgressBarWidth);
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
            return $"`{filter.FilterName}` {filter.Accepted}/{filter.Desired}";
        }

        return $"`{filter.FilterName}` {filter.Accepted}/{filter.Desired} ({filter.Acquired})";
    }
}
