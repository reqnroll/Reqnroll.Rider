using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Impl.CodeStyle;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util.Text;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Formatting
{
    [Language(typeof(GherkinLanguage))]
    public class GherkinCodeFormatter : CodeFormatterBase<GherkinFormatSettingsKey>
    {
        private readonly GherkinFormatterInfoProvider _formatterInfoProvider;

        public GherkinCodeFormatter(PsiLanguageType languageType, CodeFormatterRequirements requirements, GherkinFormatterInfoProvider formatterInfoProvider)
            : base(languageType, requirements)
        {
            _formatterInfoProvider = formatterInfoProvider;
        }

        protected override CodeFormattingContext CreateFormatterContext(CodeFormatProfile profile, ITreeNode firstNode, ITreeNode lastNode, AdditionalFormatterParameters parameters, ICustomFormatterInfoProvider provider)
        {
            return new CodeFormattingContext(this, firstNode, lastNode, FormatterLoggerProvider.FormatterLogger, parameters);
        }

        public override MinimalSeparatorType GetMinimalSeparatorByNodeTypes(TokenNodeType leftToken, TokenNodeType rightToken)
        {
            if (leftToken == GherkinTokenTypes.TAG)
                return MinimalSeparatorType.NewLine;
            if (rightToken == GherkinTokenTypes.TAG)
                return MinimalSeparatorType.NewLine;

            return MinimalSeparatorType.NotRequired;
        }

        public override ITreeNode CreateSpace(string indent, ITreeNode replacedSpace)
        {
            return GherkinTokenTypes.WHITE_SPACE.CreateLeafElement(indent);
        }

        public override ITreeNode CreateNewLine(LineEnding lineEnding, NodeType lineBreakType = null)
        {
            return GherkinTokenTypes.NEW_LINE.CreateLeafElement(lineEnding.GetPresentation());
        }

        public override ITreeRange Format(ITreeNode firstElement, ITreeNode lastElement, CodeFormatProfile profile, AdditionalFormatterParameters parameters = null)
        {
            parameters ??= AdditionalFormatterParameters.Empty;

            var task = new FormatTask(firstElement, lastElement, profile);
            task.Adjust(this);

            if (task.FirstElement == null)
                return new TreeRange(firstElement, lastElement);

            var formatterSettings = GetFormattingSettings(task.FirstElement, parameters, _formatterInfoProvider);
            formatterSettings.Settings.SetValue(key => key.WRAP_LINES, false);

            DoDeclarativeFormat(formatterSettings, _formatterInfoProvider, null, new[] {task},
                parameters, null, null, null, false);

            return new TreeRange(firstElement, lastElement);
        }

        public override void FormatInsertedNodes(ITreeNode nodeFirst, ITreeNode nodeLast, bool formatSurround)
        {
        }

        public override ITreeRange FormatInsertedRange(ITreeNode nodeFirst, ITreeNode nodeLast, ITreeRange origin)
        {
            return new TreeRange(nodeFirst, nodeLast);
        }

        public override void FormatReplacedNode(ITreeNode oldNode, ITreeNode newNode)
        {
        }

        public override void FormatReplacedRange(ITreeNode first, ITreeNode last, ITreeRange oldNodes)
        {
        }

        public override void FormatDeletedNodes(ITreeNode parent, ITreeNode prevNode, ITreeNode nextNode)
        {
        }
    }
}