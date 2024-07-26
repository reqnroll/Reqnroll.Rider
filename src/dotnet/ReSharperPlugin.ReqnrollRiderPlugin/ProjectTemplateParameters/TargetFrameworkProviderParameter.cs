using System;
using System.Collections.Generic;
using JetBrains.Rider.Backend.Features.ProjectModel.ProjectTemplates.DotNetExtensions;
using JetBrains.Rider.Backend.Features.ProjectModel.ProjectTemplates.DotNetTemplates;
using JetBrains.Rider.Model;
using Microsoft.TemplateEngine.Abstractions;

namespace ReSharperPlugin.ReqnrollRiderPlugin.ProjectTemplateParameters;

public class TargetFrameworkProviderParameter()
    : DotNetTemplateParameter("targetFramework", "Framework", null)
{
    public override RdProjectTemplateOption CreateContent(
        ITemplateInfo templateInfo,
        ITemplateParameter templateParameter,
        Dictionary<string, object> context
    )
    {
        if (!templateParameter.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))
            return null;
        var parameter = templateInfo.GetParameter(Name);
        if (parameter == null)
            return null;

        var options = new List<RdProjectTemplateChoice>();
        if (parameter.Choices != null)
            foreach (var choice in parameter.Choices)
                options.Add(new RdProjectTemplateChoice(choice.Key, choice.Value.Description ?? choice.Key));

        return new RdProjectTemplateChoiceOption(parameter.DefaultValue, false, options, Name, PresentableName, Tooltip);
    }
}