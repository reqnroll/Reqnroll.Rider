using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Rider.Backend.Features.ProjectModel.ProjectTemplates.DotNetExtensions;

namespace ReSharperPlugin.SpecflowRiderPlugin.ProjectTemplateParameters
{
    [ShellComponent]
    public class UnitTestProvider : IDotNetTemplateParameterProvider
    {
        public int Priority => 50;

        public IReadOnlyCollection<DotNetTemplateParameter> Get()
        {
            return new[] {new UnitTestProviderParameter()};
        }
    }
}