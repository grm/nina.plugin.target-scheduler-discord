using FluentAssertions;
using NINA.Plugin.TargetScheduler.Discord.Services;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Discord.Tests;

public class TargetProgressCalculatorTests {
    [Test]
    public void Calculates_percent_from_accepted_over_desired() {
        var progress = TargetProgressCalculator.FromExposurePlans(
            "Nebulae",
            "M31",
            true,
            new[] {
                ("Ha", 30, 15, 20),
                ("OIII", 20, 5, 5)
            });

        progress.PercentComplete.Should().BeApproximately(40d, 0.01);
        progress.Filters.Should().HaveCount(2);
        progress.Filters[0].FilterName.Should().Be("Ha");
        progress.Filters[0].Accepted.Should().Be(15);
        progress.Filters[0].Acquired.Should().Be(20);
    }

    [Test]
    public void Sorts_targets_by_percent_descending() {
        var sorted = TargetProgressCalculator.SortByProgress(new[] {
            TargetProgressCalculator.FromExposurePlans("A", "Low", true, new[] { ("L", 10, 1, 1) }),
            TargetProgressCalculator.FromExposurePlans("B", "High", true, new[] { ("L", 10, 9, 9) }),
            TargetProgressCalculator.FromExposurePlans("C", "Mid", true, new[] { ("L", 10, 5, 5) })
        });

        sorted.Select(target => target.TargetName).Should().Equal("High", "Mid", "Low");
    }
}
