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
        public bool IsProcessingFinished(IHighlightingConsumer context) => false;

        public void ProcessBeforeInterior(ITreeNode element, IHighlightingConsumer context)
        {
            if (element is not GherkinToken token)
                return;
            if (IsScenarioToken(token))
            {
                var scenario = element.Parent as IGherkinScenario;
                var title = scenario?.GetScenarioText();
                if(title == null)
                    context.AddHighlighting(new GherkinScenarioHasNoTitleError(token));

                var hasSameTitle = scenario?.Parent is GherkinFeature feature && feature.GetScenarios().Count(sc => sc.GetScenarioText() == title) > 1;
                if(hasSameTitle)
                    context.AddHighlighting(new GherkinScenarioWithSameTitleError(token));
            }
            if (IsScenarioToken(token) && element.Parent is not IGherkinScenario)
                context.AddHighlighting(new GherkinSyntaxScenarioNotInFeatureError(token));

            if (token.GetTokenType() == GherkinTokenTypes.TEXT && element.Parent is IGherkinScenario && !IsInScenarioDescription(token))
                context.AddHighlighting(new GherkinSyntaxError(token));

            if (token.GetTokenType() == GherkinTokenTypes.STEP_KEYWORD || token.GetTokenType() == GherkinTokenTypes.EXAMPLES_KEYWORD)
            {
                var table = element.Parent?.Children<GherkinTable>().FirstOrDefault();
                var inconsistentCellCount =  CheckInconsistentCellCount(table);
                if (inconsistentCellCount)
                    context.AddHighlighting(new InconsistentCellCountWithinTheTableError(table));
            }
        }

        private static bool CheckInconsistentCellCount(GherkinTable table)
        {  
            if (table != null)
            {
                var headerRow = table.FirstChild;
                var cellCount = headerRow?.Children<GherkinTableCell>().Count();
                var gherkinTableRows = table.Children<GherkinTableRow>();
               return gherkinTableRows.Any(row => row.Children<GherkinTableCell>().Count() != cellCount);
            }
            return false;
        }

        private bool IsScenarioToken(ITreeNode token)
        {
            return token.GetTokenType() == GherkinTokenTypes.SCENARIO_KEYWORD || token.GetTokenType() == GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD;
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

        private bool IsAtStartOfLine(GherkinToken token)
        {
            return token.PrevSibling?.GetTokenType() == GherkinTokenTypes.NEW_LINE
                   || token.PrevSibling?.GetTokenType() == GherkinTokenTypes.WHITE_SPACE && token.PrevSibling?.PrevSibling?.GetTokenType() == GherkinTokenTypes.NEW_LINE;
        }

        public void ProcessAfterInterior(ITreeNode element, IHighlightingConsumer context)
        {
        }
    }
}