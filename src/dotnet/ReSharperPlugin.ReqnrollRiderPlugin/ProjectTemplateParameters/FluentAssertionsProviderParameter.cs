using System;
using System.Collections.Generic;
using JetBrains.Rider.Backend.Features.ProjectModel.ProjectTemplates.DotNetExtensions;
using JetBrains.Rider.Backend.Features.ProjectModel.ProjectTemplates.DotNetTemplates;
using JetBrains.Rider.Model;
using Microsoft.TemplateEngine.Abstractions;

namespace ReSharperPlugin.ReqnrollRiderPlugin.ProjectTemplateParameters;

public class FluentAssertionsProviderParameter()
    : DotNetTemplateParameter("includeFluentAssertions", "FluentAssertions", "Add FluentAssertions library")
{

    public override RdProjectTemplateOption CreateContent(
        ITemplateInfo templateInfo,
        ITemplateParameter templateParameter,
        Dictionary<string, object> context)
    {
        if (!templateParameter.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))
            return null;
        var parameter = templateInfo.GetParameter(Name);
        if (parameter == null)
            return null;

        var boolOptions = new Dictionary<string, string> {{"true", "Include"}, {"false", "Exclude"}};

        var options = new List<RdProjectTemplateChoice>();
        foreach (var boolOption in boolOptions)
            options.Add(new RdProjectTemplateChoice(boolOption.Key, boolOption.Value));

        return new RdProjectTemplateChoiceOption(parameter.DefaultValue, false, options, Name, PresentableName, Tooltip);
    }
}