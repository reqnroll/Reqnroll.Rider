using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Format;
using JetBrains.ReSharper.Psi.Impl.CodeStyle;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util.Text;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Formatting;

[Language(typeof(GherkinLanguage))]
public class GherkinCodeFormatter(PsiLanguageType languageType, CodeFormatterRequirements requirements, GherkinFormatterInfoProvider formatterInfoProvider)
    : CodeFormatterBase<GherkinFormatSettingsKey>(languageType, requirements)
{

    protected override CodeFormattingContext CreateFormatterContext(
        AdditionalFormatterParameters parameters,
        ICustomFormatterInfoProvider provider,
        int tabWidth,
        SingleLangChangeAccu changeAccu,
        FormatTask[] formatTasks)
    {
        return new CodeFormattingContext(this, FormatterLoggerProvider.FormatterLogger, parameters, tabWidth, changeAccu, formatTasks);
    }

    public override MinimalSeparatorType GetMinimalSeparatorByNodeTypes(TokenNodeType leftToken, TokenNodeType rightToken)
    {
        if (leftToken == GherkinTokenTypes.TAG && rightToken != GherkinTokenTypes.TAG)
            return MinimalSeparatorType.NewLine;
        if (leftToken != GherkinTokenTypes.TAG && rightToken == GherkinTokenTypes.TAG)
            return MinimalSeparatorType.NewLine;
        if (leftToken == GherkinTokenTypes.TABLE_CELL && rightToken == GherkinTokenTypes.PIPE)
            return MinimalSeparatorType.Space;
        if (rightToken == GherkinTokenTypes.TABLE_CELL && leftToken == GherkinTokenTypes.PIPE)
            return MinimalSeparatorType.Space;

        return MinimalSeparatorType.NotRequired;
    }

    public override ITreeNode CreateSpace(string indent, NodeType replacedOrLeftSiblingType)
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

        var formatterSettings = GetFormattingSettings(task.FirstElement, parameters, formatterInfoProvider);
        formatterSettings.Settings.SetValue(key => key.WRAP_LINES, false);

        DoDeclarativeFormat(formatterSettings, formatterInfoProvider, null, [task], parameters, null, null);

        return new TreeRange(firstElement, lastElement);
    }

    public override void FormatInsertedNodes(ITreeNode nodeFirst, ITreeNode nodeLast, bool formatSurround, bool indentSurround = false)
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