using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.SpecflowRiderPlugin.Daemon.Errors;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.SyntaxError
{
    public class SyntaxErrorProcessor : IRecursiveElementProcessor<IHighlightingConsumer>
    {
        public bool InteriorShouldBeProcessed(ITreeNode element, IHighlightingConsumer context) => true;

        public bool IsProcessingFinished(IHighlightingConsumer context)
        {
            return context.Highlightings.Select(h => h.Highlighting).OfType<GherkinHasNoFeatureError>().Any();
        }

        public void ProcessBeforeInterior(ITreeNode element, IHighlightingConsumer context)
        {
            if (element is not GherkinToken token)
                return;

            if (token.Parent is GherkinFile gherkinFile)
            {
                var feature = gherkinFile.Children<GherkinFeature>().FirstOrDefault();
                if (feature == null)
                    context.AddHighlighting(new GherkinHasNoFeatureError(gherkinFile));
            }

            if (IsScenarioToken(token))
            {
                if (element.Parent is IGherkinScenario scenario && !scenario.IsBackground())
                {
                    var title = scenario.GetScenarioText();
                    if (title == null)
                        context.AddHighlighting(new GherkinScenarioHasNoTitleError(token));

                    var hasSameTitle = scenario.Parent is GherkinFeature feature && feature.GetScenarios()
                        .Where(s => !s.IsBackground()).Count(sc => sc.GetScenarioText() == title) > 1;
                    if (hasSameTitle)
                        context.AddHighlighting(new GherkinScenarioWithSameTitleError(token));
                }
            }

            if (IsScenarioToken(token) && element.Parent is not IGherkinScenario)
                context.AddHighlighting(new GherkinSyntaxScenarioNotInFeatureError(token));

            if (token.GetTokenType() == GherkinTokenTypes.TEXT && element.Parent is IGherkinScenario && !IsInScenarioDescription(token))
                context.AddHighlighting(new GherkinSyntaxError(token));

            if (token.GetTokenType() == GherkinTokenTypes.STEP_KEYWORD || token.GetTokenType() == GherkinTokenTypes.EXAMPLES_KEYWORD)
            {
                var table = element.Parent?.Children<GherkinTable>().FirstOrDefault();
                var inconsistentRows = GetInconsistentRows(table);
                foreach (var gherkinTableRow in inconsistentRows)
                {
                    context.AddHighlighting(new InconsistentCellCountWithinTheTableError(gherkinTableRow));
                }
            }
        }

        private static IEnumerable<GherkinTableRow> GetInconsistentRows(GherkinTable table)
        {
            if (table != null)
            {
                var headerRow = table.FirstChild;
                var cellCount = headerRow?.Children<GherkinTableCell>().Count();
                var pipeCount = headerRow?.Children<GherkinToken>().Count(g => g.GetTokenType() == GherkinTokenTypes.PIPE);
                var gherkinTableRows = table.Children<GherkinTableRow>();
                return gherkinTableRows.Where(row =>
                    row.Children<GherkinTableCell>().Count() != cellCount
                    ||
                    row.Children<GherkinToken>().Count(g => g.GetTokenType() == GherkinTokenTypes.PIPE) != pipeCount);
            }
            return Enumerable.Empty<GherkinTableRow>();
        }

        private bool IsScenarioToken(ITreeNode token)
        {
            return token.GetTokenType() == GherkinTokenTypes.SCENARIO_KEYWORD || token.GetTokenType() == GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD ||
                   token.GetTokenType() == GherkinTokenTypes.BACKGROUND_KEYWORD;
        }

        private bool IsInScenarioDescription(GherkinToken token)
        {
            var current = token.PrevSibling;
            var hasScenarioKeyword = false;
            while (current != null)
            {
                if (IsScenarioToken(current))
                {
                    hasScenarioKeyword = true;
                }
                if (current is GherkinStep)
                {
                    return false;
                }
                current = current.PrevSibling;
            }
            return hasScenarioKeyword;
        }

        public void ProcessAfterInterior(ITreeNode element, IHighlightingConsumer context)
        {
        }
    }
}