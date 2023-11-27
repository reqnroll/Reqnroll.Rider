#nullable enable
namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions;

public record SpecflowStepScope(string? Feature, string? Scenario, string? Tag);