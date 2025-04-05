using System;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions.AssemblyStepDefinitions;
using ReSharperPlugin.ReqnrollRiderPlugin.Icons;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.CompletionProviders;

[Language(typeof(GherkinLanguage))]
public class GherkinStepCompletionProvider : ItemsProviderOfSpecificContext<GherkinSpecificCodeCompletionContext>
{
    protected override bool IsAvailable(GherkinSpecificCodeCompletionContext context)
    {
        var codeCompletionType = context.BasicContext.CodeCompletionType;
        return codeCompletionType == CodeCompletionType.BasicCompletion || codeCompletionType == CodeCompletionType.SmartCompletion;
    }

    protected override bool AddLookupItems(GherkinSpecificCodeCompletionContext context, IItemsCollector collector)
    {
        if (context.NodeUnderCursor is not GherkinStep selectedStep)
            return false;

        var reqnrollStepsDefinitionsCache = context.BasicContext.PsiServices.GetComponent<ReqnrollStepsDefinitionsCache>();
        var assemblyStepDefinitionCache = context.BasicContext.PsiServices.GetComponent<AssemblyStepDefinitionCache>();
        var stepPatternUtil = context.BasicContext.PsiServices.GetComponent<IStepPatternUtil>();

        var psiModule = context.BasicContext.File.GetPsiModule();
        var selectedStepKind = selectedStep.EffectiveStepKind;
        var partialStepText = context.RelatedText;
        var fullStepText = selectedStep.GetStepText();

        foreach (var stepDefinitionInfo in reqnrollStepsDefinitionsCache.GetStepAccessibleForModule(psiModule, selectedStepKind).Concat(assemblyStepDefinitionCache.GetStepAccessibleForModule(psiModule, selectedStepKind)))
        {
            if (!selectedStep.MatchScope(stepDefinitionInfo.Scopes))
                continue;
            foreach (var stepVariation in stepPatternUtil.ExpandMatchingStepPatternWithAllPossibleParameter(stepDefinitionInfo, partialStepText, fullStepText))
            {
                var completionText = stepVariation;
                try
                {
                    completionText = Regex.Unescape(stepVariation);
                }
                catch (Exception)
                {
                    // Ignored
                }
                var lookupItem = new CompletionStepLookupItem(completionText, ReqnrollIcons.ReqnrollIcon);
                lookupItem.InitializeRanges(context.Ranges, context.BasicContext);

                collector.Add(lookupItem);
            }
        }

        return true;
    }

    protected override void TransformItems(GherkinSpecificCodeCompletionContext context, IItemsCollector collector)
    {
        if (context.NodeUnderCursor is not GherkinStep)
            return;

        // Filter out other completion suggestion from generic plugin like live template
        collector.RemoveWhere(x => x is not CompletionStepLookupItem);
    }

    protected override TextLookupRanges GetDefaultRanges(GherkinSpecificCodeCompletionContext context)
    {
        return context.Ranges;
    }
}