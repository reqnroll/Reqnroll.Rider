#nullable enable
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Caching.ReqnrollJsonSettings;

[ShellComponent]
public class ReqnrollSettingsProvider
{
    public static readonly ReqnrollSettings DefaultSettings = new();
    private readonly ConcurrentDictionary<IProject, ReqnrollSettings> _jsonSettingsRepository = new();
    private readonly ConcurrentDictionary<IProject, ReqnrollSettings> _appConfigSettingsRepository = new();
    private readonly ConcurrentDictionary<Lifetime, HashSet<IProject>> _projectsInSolutionRepository = new();

    /// <summary>
    /// Update reqnroll settings
    /// </summary>
    /// <param name="project"></param>
    /// <param name="source"></param>
    /// <param name="settings"></param>
    /// <returns>true if it changes the current project config</returns>
    public bool TryUpdate(IProject project, ConfigSource source, ReqnrollSettings? settings)
    {
        var solution = project.GetSolution();
        // GetSolution() return null when the project is deleted
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (solution is null)
            return false;
        var lifetime = solution.GetSolutionLifetimes().UntilSolutionCloseLifetime;

        RegisterProjectLifetime(project, lifetime);

        switch (source)
        {
            case ConfigSource.Json:
            {
                if (settings == null)
                    _jsonSettingsRepository.TryRemove(project, out _);
                else
                    _jsonSettingsRepository[project] = settings;
                return true;
            }
            case ConfigSource.AppConfig:
                if (settings == null)
                    _appConfigSettingsRepository.TryRemove(project, out _);
                else
                    _appConfigSettingsRepository[project] = settings;
                return !_jsonSettingsRepository.ContainsKey(project);
            default:
                return false;
        }
    }

    public ReqnrollSettings GetSettings(IProject? project)
    {
        if (project == null)
            return DefaultSettings;
        if (_jsonSettingsRepository.TryGetValue(project, out var jsonSettings))
            return jsonSettings;
        if (_appConfigSettingsRepository.TryGetValue(project, out var appConfigSettings))
            return appConfigSettings;
        return DefaultSettings;
    }

    public ReqnrollSettings GetDefaultSettings()
    {
        return _jsonSettingsRepository.FirstOrDefault().Value ?? _appConfigSettingsRepository.FirstOrDefault().Value ?? DefaultSettings;
    }

    private void RegisterProjectLifetime(IProject project, Lifetime lifetime)
    {

        if (!_projectsInSolutionRepository.TryGetValue(lifetime, out var projectsWithSettings))
        {
            projectsWithSettings = new HashSet<IProject>();
            var addedProjects = _projectsInSolutionRepository.GetOrAdd(lifetime, projectsWithSettings);
            if (addedProjects == projectsWithSettings)
            {
                lifetime.OnTermination(() =>
                {
                    if (!_projectsInSolutionRepository.TryRemove(lifetime, out var projectsToRemove))
                        return;

                    foreach (var projectToRemove in projectsToRemove)
                    {
                        _jsonSettingsRepository.TryRemove(projectToRemove, out _);
                        _appConfigSettingsRepository.TryRemove(projectToRemove, out _);
                    }
                });
            }
        }

        lock (projectsWithSettings)
            projectsWithSettings.Add(project);
    }
}