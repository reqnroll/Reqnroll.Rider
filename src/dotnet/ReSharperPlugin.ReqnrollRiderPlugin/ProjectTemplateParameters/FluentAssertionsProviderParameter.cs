using System.Collections.Generic;
using JetBrains.Rider.Backend.Features.ProjectModel.ProjectTemplates.DotNetExtensions;
using JetBrains.Rider.Backend.Features.ProjectModel.ProjectTemplates.DotNetTemplates;
using JetBrains.Rider.Model;

namespace ReSharperPlugin.ReqnrollRiderPlugin.ProjectTemplateParameters
{
    public class FluentAssertionsProviderParameter : DotNetTemplateParameter
    {
        public FluentAssertionsProviderParameter() : base("includeFluentAssertions", "FluentAssertions", "Add FluentAssertions library")
        {

        }

        public override RdProjectTemplateContent CreateContent(DotNetProjectTemplateExpander expander, IDotNetTemplateContentFactory factory, int index, IDictionary<string, string> context)
        {
            var parameter = expander.TemplateInfo.GetParameter(Name);
            if (parameter == null)
            {
                return factory.CreateNextParameters(new[] {expander}, index + 1, context);
            }

            var boolOptions = new Dictionary<string, string>() {{"true","Include"}, {"false","Exclude"}};
            
            var options = new List<RdProjectTemplateGroupOption>();
            foreach (var boolOption in boolOptions)
            {
                var content = factory.CreateNextParameters(new[] {expander}, index + 1, context);
                options.Add(new RdProjectTemplateGroupOption(boolOption.Key, boolOption.Value, null, content));
            }
            return new RdProjectTemplateGroupParameter(Name,PresentableName, parameter.DefaultValue, Tooltip, options);
            
        }
    }

}