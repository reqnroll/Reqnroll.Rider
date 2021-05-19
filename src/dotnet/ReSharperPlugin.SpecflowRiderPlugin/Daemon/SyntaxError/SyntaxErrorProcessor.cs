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
            if (token.GetTokenType() == GherkinTokenTypes.SCENARIO_KEYWORD && element.Parent is not GherkinScenario)
                context.AddHighlighting(new GherkinSyntaxScenarioNotInFeatureError(token));
            if (token.GetTokenType() == GherkinTokenTypes.TEXT && IsAtStartOfLine(token) && element.Parent is not GherkinPystring)
                context.AddHighlighting(new GherkinSyntaxError(token));
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