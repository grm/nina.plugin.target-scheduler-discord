using NINA.Plugin.TargetScheduler.Discord.Models;
using System.Text;

namespace NINA.Plugin.TargetScheduler.Discord.Services;

public static class TargetProgressFormatter {
    public static string FormatMessage(IReadOnlyList<TargetProgress> targets, DateTimeOffset timestamp) {
        var builder = new StringBuilder();
        builder.AppendLine($"**Target Scheduler progress** — {timestamp:yyyy-MM-dd HH:mm} UTC");

        if (targets.Count == 0) {
            builder.Append("No active targets found.");
            return builder.ToString();
        }

        foreach (var target in targets) {
            builder.AppendLine(FormatTargetLine(target));
        }

        return builder.ToString().TrimEnd();
    }

    public static string FormatTargetLine(TargetProgress target) {
        var label = string.IsNullOrWhiteSpace(target.ProjectName)
            ? target.TargetName
            : $"{target.ProjectName} / {target.TargetName}";

        var filters = target.Filters.Count == 0
            ? "(no exposure plans)"
            : string.Join(" · ", target.Filters.Select(FormatFilter));

        return $"`{target.PercentComplete,6:0.0}%` **{label}** — {filters}";
    }

    private static string FormatFilter(FilterProgress filter) {
        return $"{filter.FilterName} {filter.Accepted}/{filter.Desired} (acq {filter.Acquired})";
    }

    public static IReadOnlyList<string> SplitForDiscord(string content, int maxLength = 1900) {
        if (string.IsNullOrEmpty(content)) {
            return Array.Empty<string>();
        }

        if (content.Length <= maxLength) {
            return new[] { content };
        }

        var chunks = new List<string>();
        var lines = content.Replace("\r\n", "\n").Split('\n');
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
                chunks.Add(current.ToString());
                current.Clear();
            }

            if (line.Length <= maxLength) {
                current.Append(line);
            } else {
                for (var index = 0; index < line.Length; index += maxLength) {
                    chunks.Add(line.Substring(index, Math.Min(maxLength, line.Length - index)));
                }
            }
        }

        if (current.Length > 0) {
            chunks.Add(current.ToString());
        }

        return chunks;
    }
}
