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
            }
            if (IsScenarioToken(token) && element.Parent is not IGherkinScenario)
                context.AddHighlighting(new GherkinSyntaxScenarioNotInFeatureError(token));
            if (token.GetTokenType() == GherkinTokenTypes.TEXT && element.Parent is IGherkinScenario && !IsInScenarioDescription(token))
                context.AddHighlighting(new GherkinSyntaxError(token));
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