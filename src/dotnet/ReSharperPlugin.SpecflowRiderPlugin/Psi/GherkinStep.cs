using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.IL.Resources;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.SpecflowRiderPlugin.References;
using ReSharperPlugin.SpecflowRiderPlugin.Utils.TestOutput;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinStep : GherkinElement
    {
        public GherkinStepKind StepKind { get; }
        public GherkinStepKind EffectiveStepKind { get; }
        private SpecflowStepDeclarationReference _reference;

        public GherkinStep(GherkinStepKind stepKind, GherkinStepKind effectiveStepKind) : base(GherkinNodeTypes.STEP)
        {
            StepKind = stepKind;
            EffectiveStepKind = effectiveStepKind;
        }

        protected override void PreInit()
        {
            base.PreInit();
            _reference = new SpecflowStepDeclarationReference(this);
        }

        public DocumentRange GetStepTextRange()
        {
            var token = GetFirstTextToken();
            if (token == null)
                return new DocumentRange(LastChild.GetDocumentEndOffset(), LastChild.GetDocumentEndOffset());
            return new DocumentRange(token.GetDocumentStartOffset(), LastChild.GetDocumentEndOffset());
        }

        public IEnumerable<string> GetEffectiveTags()
        {
            var gherkinScenario = GetContainingNode<GherkinScenario>();
            if (gherkinScenario == null)
                return Enumerable.Empty<string>();
            var gherkinFeature = gherkinScenario.GetContainingNode<GherkinFeature>();
            if (gherkinFeature == null)
                return gherkinScenario.GetTags();
            return gherkinScenario.GetTags().Concat(gherkinFeature.GetTags());
        }

        private ITreeNode GetFirstTextToken()
        {
            for (var node = FirstChild; node != null; node = node.NextSibling)
            {
                if (node is GherkinToken token)
                {
                    if (token.NodeType == GherkinTokenTypes.STEP_KEYWORD)
                        continue;
                    if (token.NodeType == GherkinTokenTypes.WHITE_SPACE)
                        continue;
                }
                return node;
            }
            return null;
        }

        public string GetStepTextBeforeCaret(DocumentOffset caretLocation)
        {
            var sb = new StringBuilder();
            for (var te = GetFirstTextToken(); te != null; te = te.NextSibling)
            {
                if (te.GetDocumentStartOffset() > caretLocation)
                    break;
                var truncateTextSize = 0;
                if (te.GetDocumentEndOffset() > caretLocation)
                {
                    truncateTextSize = te.GetDocumentEndOffset().Offset - caretLocation.Offset;
                }
                switch (te)
                {
                    case GherkinStepParameter p:
                        sb.Append(p.GetText());
                        break;
                    case GherkinToken token:
                        if (token.NodeType != GherkinTokenTypes.STEP_KEYWORD)
                            sb.Append(token.GetText());
                        break;
                }
                if (truncateTextSize >= sb.Length)
                    return string.Empty;
                sb.Length -= truncateTextSize;
            }
            return sb.ToString().Trim();
        }

        public string GetStepText(bool withStepKeyWord = false)
        {
            var sb = new StringBuilder();
            for (var te = (TreeElement) FirstChild; te != null; te = te.nextSibling)
            {
                switch (te)
                {
                    case GherkinStepParameter p:
                        sb.Append(p.GetText());
                        break;
                    case GherkinToken token:
                    {
                        if (withStepKeyWord)
                        {
                            sb.Append(token.GetText());
                        }
                        else
                        {
                            if (token.NodeType != GherkinTokenTypes.STEP_KEYWORD)
                            {
                                sb.Append(token.GetText());
                            }
                        }

                        break;
                    }
                }
            }
            return sb.ToString().Trim();
        }

        public string GetStepTextForExample(IDictionary<string, string> exampleData)
        {
            var sb = new StringBuilder();
            var previousTokenWasAParameter = false;
            for (var te = (TreeElement) FirstChild; te != null; te = te.nextSibling)
            {
                switch (te)
                {
                    case GherkinStepParameter p:
                        if (exampleData.TryGetValue(p.GetParameterName(), out var value))
                        {
                            previousTokenWasAParameter = true;
                            sb.Length--; // Remove `<`
                            sb.Append(value);
                        }
                        else
                        {
                            sb.Append(p.GetText());
                        }

                        break;
                    case GherkinToken token when token.NodeType != GherkinTokenTypes.STEP_KEYWORD:
                        sb.Append(token.GetText());

                        // Remove `>`
                        if (previousTokenWasAParameter)
                            sb.Length--;
                        previousTokenWasAParameter = false;
                        break;
                }
            }
            return sb.ToString().Trim();
        }

        public SpecflowStepDeclarationReference GetStepReference()
        {
            return _reference;
        }

        public override ReferenceCollection GetFirstClassReferences()
        {
            return new ReferenceCollection(_reference);
        }

        public string GetFirstLineText()
        {
            var sb = new StringBuilder();
            for (var te = (TreeElement) FirstChild; te != null; te = te.nextSibling)
            {
                if (te.GetTokenType() == GherkinTokenTypes.NEW_LINE)
                    break;

                sb.Append(te.GetText());
            }
            return sb.ToString().Trim();
        }

        public bool Match(StepTestOutput failedStepStepsOutput)
        {
            return GetFirstLineText() == failedStepStepsOutput.FirstLine;
        }
    }
}