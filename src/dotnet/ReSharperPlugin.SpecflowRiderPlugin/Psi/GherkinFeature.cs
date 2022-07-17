using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinFeature : GherkinElement
    {
        [CanBeNull] private IList<string> _cachedTags;

        public GherkinFeature() : base(GherkinNodeTypes.FEATURE)
        {
        }

        public string GetFeatureText()
        {
            return this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TEXT)?.GetText();
        }

        [CanBeNull]
        public GherkinScenario GetScenario(string scenarioText)
        {
            return this.FindChild<GherkinScenario>(o => o.GetScenarioText() == scenarioText);
        }

        public IEnumerable<IGherkinScenario> GetScenarios()
        {
            return this.Children<GherkinScenario>().Cast<IGherkinScenario>()
                .Concat(this.Children<GherkinScenarioOutline>())
                .Concat(this.Children<GherkinRule>().SelectMany(x => x.Children<GherkinScenario>()));
        }

        public IList<string> GetTags()
        {
            return _cachedTags ??= ListTags().ToList();

        }

        private IEnumerable<string> ListTags()
        {
            var node = PrevSibling;
            while (node != null)
            {
                if (node is GherkinTag tag)
                    yield return tag.GetTagText();
                else if (!node.IsWhitespaceToken() && node is not GherkinLanguageComment)
                    break;
                node = node.PrevSibling;
            }
        }

    }
}