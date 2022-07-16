using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;

namespace ReSharperPlugin.SpecflowRiderPlugin.Caching.SpecflowJsonSettings
{
    [ShellComponent]
    public class SpecflowSettingsProvider
    {
        private readonly ConcurrentDictionary<IProject, SpecflowSettings> _settingsRepository = new ConcurrentDictionary<IProject, SpecflowSettings>();
        private readonly ConcurrentDictionary<Lifetime, HashSet<IProject>> _projectsInSolutionRepository = new ConcurrentDictionary<Lifetime, HashSet<IProject>>();

        public void Update(IProject project, SpecflowSettings settings)
        {
            var lifetime = project.GetSolution().GetLifetime();

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
                                                   _settingsRepository.TryRemove(projectToRemove, out _);
                                           });
                }
            }

            lock (projectsWithSettings)
                projectsWithSettings.Add(project);

            _settingsRepository[project] = settings;
        }

        public SpecflowSettings GetSettings(IProject project)
        {
            var settings = _settingsRepository.GetOrAdd(project, _ => new SpecflowSettings());

            return settings;
        }

        public SpecflowSettings GetDefaultSettings()
        {
            var settings = _settingsRepository.FirstOrDefault().Value;
            if (settings == null)
                settings = new SpecflowSettings();
            
            return settings;
        }
    }
}