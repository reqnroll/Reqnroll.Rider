using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.StepsDefinitions;
using ReSharperPlugin.ReqnrollRiderPlugin.References;
using ReSharperPlugin.ReqnrollRiderPlugin.Utils.TestOutput;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi
{
    public class GherkinStep : GherkinElement
    {
        public GherkinStepKind StepKind { get; }
        public GherkinStepKind EffectiveStepKind { get; }
        private ReqnrollStepDeclarationReference _reference;

        public GherkinStep(GherkinStepKind stepKind, GherkinStepKind effectiveStepKind) : base(GherkinNodeTypes.STEP)
        {
            StepKind = stepKind;
            EffectiveStepKind = effectiveStepKind;
        }

        protected override void PreInit()
        {
            base.PreInit();
            _reference = new ReqnrollStepDeclarationReference(this);
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

            var element = (TreeElement)FirstChild;
            if (!withStepKeyWord)
            {
                // Skip keyword and white space at tbe begining
                for (; element != null && (element.NodeType == GherkinTokenTypes.STEP_KEYWORD || element.NodeType == GherkinTokenTypes.WHITE_SPACE); element = element.nextSibling)
                    element = element.nextSibling;
            }

            var eol = false;
            for (; element != null && !eol; element = element.nextSibling)
            {
                switch (element)
                {
                    case GherkinStepParameter p:
                        sb.Append(p.GetText());
                        break;
                    case GherkinToken token:
                    {
                        if (token.IsWhitespaceToken() && element.nextSibling?.IsWhitespaceToken() == false)
                            sb.Append(token.GetText());
                        else if (!token.IsWhitespaceToken())
                            sb.Append(token.GetText());
                        if (token.NodeType == GherkinTokenTypes.NEW_LINE)
                            eol = true;
                        break;
                    }
                }
            }
            return sb.ToString();
        }

        public string GetStepTextForExample(IDictionary<string, string> exampleData)
        {
            var sb = new StringBuilder();
            var previousTokenWasAParameter = false;
            // Skip keyword and white space at tbe begining
            var element = (TreeElement)FirstChild;
            for (; element != null && (element.NodeType == GherkinTokenTypes.STEP_KEYWORD || element.NodeType == GherkinTokenTypes.WHITE_SPACE); element = element.nextSibling)
                element = element.nextSibling;
            var eol = false;
            for (; element != null && !eol; element = element.nextSibling)
            {
                switch (element)
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

                    case GherkinToken token:
                        // Remove `>`

                        if (token.IsWhitespaceToken() && element.nextSibling?.IsWhitespaceToken() == false)
                        {
                            sb.Append(token.GetText());
                            if (previousTokenWasAParameter)
                                sb.Length--;
                        }
                        else if (!token.IsWhitespaceToken())
                        {
                            sb.Append(token.GetText());
                            if (previousTokenWasAParameter)
                                sb.Length--;
                        }
                        if (token.NodeType == GherkinTokenTypes.NEW_LINE)
                            eol = true;
                        previousTokenWasAParameter = false;
                        break;
                }
            }
            return sb.ToString();
        }

        public ReqnrollStepDeclarationReference GetStepReference()
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
            for (var te = (TreeElement)FirstChild; te != null; te = te.nextSibling)
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

        public bool MatchScope([CanBeNull] IReadOnlyList<ReqnrollStepScope> scopes)
        {
            if (scopes == null)
                return true;

            foreach (var scope in scopes)
            {
                if (scope.Scenario is not null)
                {
                    var matchScenario = GetScenarioText() == scope.Scenario;
                    if (!matchScenario)
                        continue;
                }

                if (scope.Feature is not null)
                {
                    var matchFeature = GetFeatureText() == scope.Feature;
                    if (!matchFeature)
                        continue;
                }

                if (scope.Tag is not null)
                {
                    var matchTag = GetEffectiveTags().Contains(scope.Tag);
                    if (!matchTag)
                        continue;
                }

                return true;
            }
            return false;
        }

        [CanBeNull]
        public string GetFeatureText()
        {
            return GetContainingNode<GherkinFeature>()?.GetFeatureText();
        }

        [CanBeNull]
        public string GetScenarioText()
        {
            return GetContainingNode<GherkinScenario>()?.GetScenarioText();
        }
    }
}