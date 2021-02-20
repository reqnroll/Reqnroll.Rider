using System.Collections.Generic;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public interface IGherkinScenario : ITreeNode
    {
        bool IsBackground();
        string GetScenarioText();
        IEnumerable<GherkinStep> GetSteps();
    }

    public class GherkinScenario : GherkinElement, IGherkinScenario
    {
        public GherkinScenario() : base(GherkinNodeTypes.SCENARIO)
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
                return "GherkinScenario(Background):";

            return $"GherkinScenario: {GetScenarioText()}";
        }
    }
}