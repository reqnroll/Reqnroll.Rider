using System.Collections.Generic;
using JetBrains.Rider.Backend.Features.ProjectModel.ProjectTemplates.DotNetExtensions;
using JetBrains.Rider.Backend.Features.ProjectModel.ProjectTemplates.DotNetTemplates;
using JetBrains.Rider.Model;

namespace ReSharperPlugin.SpecflowRiderPlugin.ProjectTemplateParameters
{
    public class TargetFrameworkProviderParameter : DotNetTemplateParameter
    {
        public TargetFrameworkProviderParameter() : base("targetFramework", "Framework", null)
        {
            
        }
        
        public override RdProjectTemplateContent CreateContent(DotNetProjectTemplateExpander expander, IDotNetTemplateContentFactory factory, int index, IDictionary<string, string> context)
        {
            var parameter = expander.TemplateInfo.GetParameter(Name);
            if (parameter == null)
            {
                return factory.CreateNextParameters(new[] {expander}, index + 1, context);
            }
            
            var options = new List<RdProjectTemplateGroupOption>();
            foreach (var choice in parameter.Choices)
            {
                var content = factory.CreateNextParameters(new[] {expander}, index + 1, context);
                
                options.Add(new RdProjectTemplateGroupOption(
                    choice.Key,
                    choice.Value.Description ?? choice.Key,
                    null, content));
            }
            return new RdProjectTemplateGroupParameter(Name,PresentableName, parameter.DefaultValue, Tooltip, options);
        }
    }
}