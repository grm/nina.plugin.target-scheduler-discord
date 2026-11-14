using FluentAssertions;
using NINA.Plugin.TargetScheduler.Discord.Services;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Discord.Tests;

public class TargetProgressFormatterTests {
    [Test]
    public void Formats_one_line_per_target_with_filter_counts() {
        var targets = TargetProgressCalculator.SortByProgress(new[] {
            TargetProgressCalculator.FromExposurePlans(
                "Nebulae",
                "M31",
                true,
                new[] { ("Ha", 30, 12, 15), ("OIII", 20, 5, 5) })
        });

        var message = TargetProgressFormatter.FormatMessage(targets, DateTimeOffset.Parse("2026-11-14T20:00:00Z"));

        message.Should().Contain("Target Scheduler progress");
        message.Should().Contain("34.0%");
        message.Should().Contain("Nebulae / M31");
        message.Should().Contain("Ha 12/30 (acq 15)");
        message.Should().Contain("OIII 5/20 (acq 5)");
    }

    [Test]
    public void Splits_long_messages_for_discord_limit() {
        var longLine = new string('x', 500);
        var content = string.Join("\n", Enumerable.Repeat(longLine, 10));
        var chunks = TargetProgressFormatter.SplitForDiscord(content, 900);

        chunks.Should().HaveCountGreaterThan(1);
        chunks.Should().OnlyContain(chunk => chunk.Length <= 900);
        string.Join("\n", chunks).Should().Be(content);
    }
}
