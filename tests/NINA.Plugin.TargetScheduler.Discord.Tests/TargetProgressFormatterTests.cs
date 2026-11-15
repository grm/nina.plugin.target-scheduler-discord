using FluentAssertions;
using NINA.Plugin.TargetScheduler.Discord.Services;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Discord.Tests;

public class TargetProgressFormatterTests {
    [Test]
    public void Formats_night_summary_style_overview_and_targets() {
        var targets = TargetProgressCalculator.SortByProgress(new[] {
            TargetProgressCalculator.FromExposurePlans(
                "Nebulae",
                "M31",
                true,
                new[] { ("Ha", 30, 12, 15), ("OIII", 20, 5, 5), ("Ha", 10, 5, 5) })
        });

        var message = TargetProgressFormatter.FormatMessage(targets, DateTimeOffset.Parse("2026-11-14T20:00:00Z"));

        message.Should().Contain("🔭 Target Scheduler Progress");
        message.Should().Contain("**Overview**");
        message.Should().Contain("Targets: 1");
        message.Should().Contain("In progress: 1");
        message.Should().Contain("**🌌 Nebulae / M31** 🔄");
        message.Should().Contain("**37%**");
        message.Should().Contain("Ha: 17/40 (20 acq)");
        message.Should().Contain("OIII: 5/20");
        message.Should().Contain("**Accepted:** 22/60");
        message.Should().Contain("|");
    }

    [Test]
    public void Uses_status_icons_for_complete_and_empty_targets() {
        var complete = TargetProgressCalculator.FromExposurePlans(
            "CTB 1",
            "CTB 1",
            true,
            new[] { ("H", 10, 10, 10), ("O", 10, 10, 11) });
        var empty = TargetProgressCalculator.FromExposurePlans(
            "Draft",
            "M42",
            true,
            new[] { ("L", 30, 0, 0) });

        var message = TargetProgressFormatter.FormatMessage(
            TargetProgressCalculator.SortByProgress(new[] { complete, empty }),
            DateTimeOffset.Parse("2026-11-14T20:00:00Z"));

        message.Should().Contain("**🌌 CTB 1** ✅");
        message.Should().NotContain("CTB 1 / CTB 1");
        message.Should().Contain("H: 10/10 | O: 10/10");
        message.Should().Contain("**🌌 Draft / M42** 🌑");
        message.Should().Contain("_no frames yet_");
    }

    [Test]
    public void Splits_long_messages_on_target_blocks() {
        var block = "**🌌 Target** 🔄\n`██████████` **100%**\nL: 10/10";
        var content = string.Join("\n\n", Enumerable.Repeat(block, 8));
        var chunks = TargetProgressFormatter.SplitForDiscord(content, 200);

        chunks.Should().HaveCountGreaterThan(1);
        chunks.Should().OnlyContain(chunk => chunk.Length <= 200);
    }
}
