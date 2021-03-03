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
            var stepPatternUtil = context.BasicContext.PsiServices.GetComponent<IStepPatternUtil>();

            foreach (var (stepSourceFile, stepDefinitions) in specflowStepsDefinitionsCache.AllStepsPerFiles)
            {
                if (!ReferenceEquals(context.BasicContext.File.GetPsiModule(), stepSourceFile.PsiModule) && !context.BasicContext.File.GetPsiModule().References(stepSourceFile.PsiModule))
                    continue;
                foreach (var stepDefinitionInfo in stepDefinitions.Where(x => x.StepKind == selectedStep.GetStepKind()))
                {
                    if (stepDefinitionInfo.RegexForPartialMatch == null)
                        continue;

                    var partialStepText = selectedStep.GetStepTextBeforeCaret(context.BasicContext.CaretDocumentOffset);
                    var fullStepText = selectedStep.GetStepText();

                    foreach (var stepVariation in stepPatternUtil.ExpandMatchingStepPatternWithAllPossibleParameter(stepDefinitionInfo, partialStepText, fullStepText))
                    {
                        var lookupItem = new TextLookupItem(stepVariation, SpecFlowThemedIcons.Specflow.Id);
                        lookupItem.InitializeRanges(context.Ranges, context.BasicContext);
                        collector.Add(lookupItem);
                    }
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