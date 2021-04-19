using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.ReSharper.Host.Features.ProjectModel.ProjectTemplates.DotNetExtensions;

namespace ReSharperPlugin.SpecflowRiderPlugin.ProjectTemplateParameters
{
    [ShellComponent]
    public class FluentAssertionsProvider : IDotNetTemplateParameterProvider
    {
        public int Priority => 60;

        public IReadOnlyCollection<DotNetTemplateParameter> Get()
        {
            return new[] {new FluentAssertionsProviderParameter()};
        }
    }
}