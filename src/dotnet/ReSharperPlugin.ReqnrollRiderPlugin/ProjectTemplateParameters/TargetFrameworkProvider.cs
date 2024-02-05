using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Rider.Backend.Features.ProjectModel.ProjectTemplates.DotNetExtensions;

namespace ReSharperPlugin.ReqnrollRiderPlugin.ProjectTemplateParameters
{
    [ShellComponent]
    public class TargetFrameworkProvider : IDotNetTemplateParameterProvider
    {
        public int Priority => 40;

        public IReadOnlyCollection<DotNetTemplateParameter> Get()
        {
            return new[] {new TargetFrameworkProviderParameter()};
        }
    }
}