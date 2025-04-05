using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public interface IGherkinScenario : ITreeNode
{
    bool IsBackground();
    string GetScenarioText();
    IEnumerable<GherkinStep> GetSteps();
}

public class GherkinScenario() : GherkinElement(GherkinNodeTypes.SCENARIO), IGherkinScenario
{
    [CanBeNull] private IList<string> _cachedTags;

    public bool IsBackground()
    {
        var firstNonCommentAndWhitespaceChild = FirstChild;
        while (firstNonCommentAndWhitespaceChild != null)
        {
            if (firstNonCommentAndWhitespaceChild is not GherkinComment
                && !firstNonCommentAndWhitespaceChild.IsWhitespaceToken())
                break;

            firstNonCommentAndWhitespaceChild = firstNonCommentAndWhitespaceChild.NextSibling;
        }

        return firstNonCommentAndWhitespaceChild?.NodeType == GherkinTokenTypes.BACKGROUND_KEYWORD;
    }

    public string GetScenarioText()
    {
        return this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TEXT)?.GetText();
    }

    public IList<string> GetTags()
    {
        return _cachedTags ??= ListTags().ToList();
    }

    private IEnumerable<string> ListTags()
    {
        var node = FirstChild;
        while (node != null)
        {
            if (node is GherkinTag tag)
                yield return tag.GetTagText();
            else if (!node.IsWhitespaceToken() && node is not GherkinLanguageComment)
                break;
            node = node.NextSibling;
        }
    }

    public IEnumerable<GherkinStep> GetSteps()
    {
        return this.Children<GherkinStep>();
    }

    public override string ToString()
    {
        if (IsBackground())
            return "GherkinScenario(Background):";

        return $"GherkinScenario: {GetScenarioText()}";
    }
}