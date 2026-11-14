using NINA.Core.Utility;
using NINA.Plugin.TargetScheduler.Discord.Models;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NINA.Plugin.TargetScheduler.Discord.Services;

public interface ITargetSchedulerClient {
    Task<IReadOnlyList<TargetProgress>> GetTargetProgressAsync(
        string apiBaseUrl,
        bool includeInactiveTargets,
        CancellationToken token);
}

public sealed class TargetSchedulerClient : ITargetSchedulerClient {
    private static readonly HttpClient Http = new() {
        Timeout = TimeSpan.FromSeconds(30)
    };

    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<TargetProgress>> GetTargetProgressAsync(
        string apiBaseUrl,
        bool includeInactiveTargets,
        CancellationToken token) {
        if (string.IsNullOrWhiteSpace(apiBaseUrl)) {
            throw new InvalidOperationException("Target Scheduler API base URL is not configured.");
        }

        var baseUri = apiBaseUrl.TrimEnd('/') + "/";
        var profiles = await GetAsync<List<TsProfile>>(baseUri + "profiles", token).ConfigureAwait(false)
            ?? new List<TsProfile>();

        var activeProfiles = profiles.Where(profile => profile.Active).ToList();
        if (activeProfiles.Count == 0) {
            activeProfiles = profiles;
        }

        var results = new List<TargetProgress>();
        foreach (var profile in activeProfiles) {
            token.ThrowIfCancellationRequested();
            var projects = await GetAsync<List<TsProject>>(baseUri + $"profiles/{profile.ProfileId}/projects", token)
                .ConfigureAwait(false) ?? new List<TsProject>();

            foreach (var project in projects.Where(project => IsProjectIncluded(project))) {
                token.ThrowIfCancellationRequested();
                var targets = await GetAsync<List<TsTarget>>(baseUri + $"projects/{project.ProjectId}/targets", token)
                    .ConfigureAwait(false) ?? new List<TsTarget>();

                foreach (var target in targets) {
                    if (!includeInactiveTargets && !target.Active) {
                        continue;
                    }

                    var plans = (target.ExposurePlan ?? new List<TsExposurePlan>())
                        .Select(plan => (
                            FilterName: plan.FilterName,
                            Desired: plan.Desired,
                            Accepted: plan.Accepted,
                            Acquired: plan.Acquired));

                    results.Add(TargetProgressCalculator.FromExposurePlans(
                        project.Name,
                        target.Name,
                        target.Active,
                        plans));
                }
            }
        }

        return TargetProgressCalculator.SortByProgress(results);
    }

    private static bool IsProjectIncluded(TsProject project) {
        return string.Equals(project.State, "ACTIVE", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<T> GetAsync<T>(string url, CancellationToken token) {
        Logger.Debug($"Target Scheduler API GET {url}");
        using var response = await Http.GetAsync(url, token).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) {
            var body = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
            throw new InvalidOperationException(
                $"Target Scheduler API request failed ({(int)response.StatusCode}): {url}. {body}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, token).ConfigureAwait(false);
    }

    private sealed class TsProfile {
        public string ProfileId { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
    }

    private sealed class TsProject {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
    }

    private sealed class TsTarget {
        public int TargetId { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }

        [JsonPropertyName("ExposurePlan")]
        public List<TsExposurePlan> ExposurePlan { get; set; }
    }

    private sealed class TsExposurePlan {
        public string FilterName { get; set; }
        public int Desired { get; set; }
        public int Accepted { get; set; }
        public int Acquired { get; set; }
    }
}
