using System.Linq;
using JetBrains.Collections;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.UI.ThemedIcons;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.SpecflowRiderPlugin.Caching.StepsDefinitions.AssemblyStepDefinitions;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.CompletionProviders
{
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
            if (!(context.NodeUnderCursor is GherkinStep selectedStep))
                return false;

            var specflowStepsDefinitionsCache = context.BasicContext.PsiServices.GetComponent<SpecflowStepsDefinitionsCache>();
            var assemblyStepDefinitionCache = context.BasicContext.PsiServices.GetComponent<AssemblyStepDefinitionCache>();
            var stepPatternUtil = context.BasicContext.PsiServices.GetComponent<IStepPatternUtil>();

            var psiModule = context.BasicContext.File.GetPsiModule();
            var selectedStepKind = selectedStep.GetStepKind();
            var partialStepText = selectedStep.GetStepTextBeforeCaret(context.BasicContext.CaretDocumentOffset);
            var fullStepText = selectedStep.GetStepText();

            foreach (var stepDefinitionInfo in specflowStepsDefinitionsCache.GetStepAccessibleForModule(psiModule, selectedStepKind).Concat(assemblyStepDefinitionCache.GetStepAccessibleForModule(psiModule, selectedStepKind)))
            {
                if (stepDefinitionInfo.RegexForPartialMatch == null)
                    continue;

                foreach (var stepVariation in stepPatternUtil.ExpandMatchingStepPatternWithAllPossibleParameter(stepDefinitionInfo, partialStepText, fullStepText))
                {
                    var lookupItem = new CompletionStepLookupItem(stepVariation, SpecFlowThemedIcons.Specflow.Id);
                    lookupItem.InitializeRanges(context.Ranges, context.BasicContext);

                    collector.Add(lookupItem);
                }
            }

            return true;
        }

        protected override TextLookupRanges GetDefaultRanges(GherkinSpecificCodeCompletionContext context)
        {
            return context.Ranges;
        }
    }
}