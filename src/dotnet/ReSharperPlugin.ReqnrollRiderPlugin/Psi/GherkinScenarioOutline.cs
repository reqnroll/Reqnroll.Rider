using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi
{
    public class GherkinScenarioOutline : GherkinElement, IGherkinScenario
    {
        public GherkinScenarioOutline() : base(GherkinNodeTypes.SCENARIO_OUTLINE)
        {
        }

        public bool IsBackground()
        {
            return FirstChild?.NodeType == GherkinTokenTypes.BACKGROUND_KEYWORD;
        }

        public string GetScenarioText()
        {
            return this.FindChild<GherkinToken>(o => o.NodeType == GherkinTokenTypes.TEXT)?.GetText();
        }

        public IEnumerable<GherkinStep> GetSteps()
        {
            return this.Children<GherkinStep>();
        }

        public override string ToString()
        {
            if (IsBackground())
                return "GherkinScenarioOutline(Background):";

            return $"GherkinScenarioOutline: {GetScenarioText()}";
        }

        public GherkinExamplesBlock GetExamplesBlock()
        {
            return this.Children<GherkinExamplesBlock>().FirstOrDefault();
        }

        public IDictionary<string, string> GetExampleData(int exampleIndex)
        {
            return GetExamplesBlock()?.GetExampleData(exampleIndex) ?? ImmutableDictionary<string, string>.Empty;
        }
    }
}