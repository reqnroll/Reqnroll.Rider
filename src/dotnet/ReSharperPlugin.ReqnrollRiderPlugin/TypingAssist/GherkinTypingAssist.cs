using System;
using System.Text;
using JetBrains.Annotations;
using JetBrains.Application.CommandProcessing;
using JetBrains.Application.Parts;
using JetBrains.Application.UI.ActionSystem.Text;
using JetBrains.Diagnostics;
using JetBrains.DocumentModel;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Options;
using JetBrains.ReSharper.Feature.Services.TypingAssist;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.Format;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Extensions;
using ReSharperPlugin.ReqnrollRiderPlugin.Formatting;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.TypingAssist;

[SolutionComponent(InstantiationEx.LegacyDefault)]
public class GherkinTypingAssist : TypingAssistLanguageBase<GherkinLanguage>, ITypingHandler
{
    public GherkinTypingAssist(
        Lifetime lifetime,
        [NotNull] TypingAssistDependencies typingAssistDependencies
    )
        : base(typingAssistDependencies)
    {
        typingAssistDependencies.TypingAssistManager.AddActionHandler(lifetime, TextControlActions.ActionIds.Enter, this, HandleEnter, IsActionHandlerAvailable);
        typingAssistDependencies.TypingAssistManager.AddTypingHandler(lifetime, '|', this, HandleTableCellClosing, IsTypingHandlerAvailable);
    }

    protected override bool IsSupported(ITextControl textControl) => true;
    public bool QuickCheckAvailability(ITextControl textControl, IPsiSourceFile psiSourceFile) => psiSourceFile.LanguageType.Is<GherkinProjectFileType>();

    private bool HandleEnter([NotNull] IActionContext context)
    {
        if (context.EnsureWritable() != EnsureWritableResult.SUCCESS)
            return false;

        var textControl = context.TextControl;
        var cachingLexer = GetCachingLexer(textControl);
        if (cachingLexer == null)
            return false;

        if (GetTypingAssistOption(textControl, TypingAssistOptions.SmartIndentOnEnterExpression))
        {
            using (CommandProcessor.UsingCommand("Smart Enter"))
            {
                if (textControl.Selection.OneDocRangeWithCaret().Length > 0)
                    return false;

                var caret = textControl.Caret.DocumentOffset();
                if (caret.Offset == 0)
                    return false;

                var lastKeywordToken = FindLastKeywordToken(cachingLexer, caret.Offset, out var inTable);
                if (lastKeywordToken == null)
                    return false;

                var extraIdentSize = 0;
                var extraIdent = "";

                if (lastKeywordToken == GherkinTokenTypes.FEATURE_KEYWORD)
                    extraIdentSize = GetFormatSettingsKey(textControl).ScenarioIndentSize;
                else if (lastKeywordToken == GherkinTokenTypes.RULE_KEYWORD)
                    extraIdentSize = GetFormatSettingsKey(textControl).ScenarioIndentSize;
                else if (lastKeywordToken == GherkinTokenTypes.EXAMPLE_KEYWORD)
                    extraIdentSize = GetFormatSettingsKey(textControl).TableIndentSize;
                else if (lastKeywordToken == GherkinTokenTypes.BACKGROUND_KEYWORD)
                    extraIdentSize = GetFormatSettingsKey(textControl).StepIndentSize;
                else if (lastKeywordToken == GherkinTokenTypes.SCENARIO_KEYWORD)
                    extraIdentSize = GetFormatSettingsKey(textControl).StepIndentSize;
                else if (lastKeywordToken == GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD)
                    extraIdentSize = GetFormatSettingsKey(textControl).StepIndentSize;
                else if (lastKeywordToken == GherkinTokenTypes.EXAMPLES_KEYWORD)
                    extraIdentSize = GetFormatSettingsKey(textControl).TableIndentSize;
                else if (lastKeywordToken == GherkinTokenTypes.EXAMPLES_KEYWORD)
                    extraIdentSize = GetFormatSettingsKey(textControl).StepIndentSize;
                else if (lastKeywordToken == GherkinTokenTypes.STEP_KEYWORD)
                    extraIdentSize = 0;

                if (inTable)
                {
                    var indentSizeBasedOnPreviousRow = FindPreviousRowFirstPipeIndent(cachingLexer, caret.Offset);
                    var indentText = GetTableIndentText(indentSizeBasedOnPreviousRow);
                    textControl.Document.InsertText(caret, GetNewLineText(textControl) + indentText);
                    return true;
                }

                var currentIndent = ComputeIndentOfCurrentKeyword(cachingLexer) + GetIndentText(textControl, extraIdentSize) + extraIdent;
                textControl.Document.InsertText(caret, GetNewLineText(textControl) + currentIndent);
                return true;
            }
        }

        return false;
    }

    private static string GetTableIndentText(int indent)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < indent; i++)
        {
            sb.Append(" ");
        }
        return sb.ToString();
    }

    private bool HandleTableCellClosing([NotNull] ITypingContext context)
    {
        var textControl = context.TextControl;

        textControl.Document.InsertText(context.TextControl.Caret.DocumentOffset(), "|");
        CommitPsiOnlyAndProceedWithDirtyCaches(textControl, (Func<IFile, object>) (file =>
        {
            var tokenNode = file.FindNodeAt(textControl.Caret.DocumentOffset());
            var parentTable = tokenNode?.GetPreviousToken()?.GetParentOfType<GherkinTable>();
            if (tokenNode == null || parentTable == null)
                return null;

            PsiServices.Transactions.Execute("Format code", () =>
            {
                GetCodeFormatter(tokenNode).Format(parentTable.firstChild.FirstChild, parentTable.lastChild.LastChild, CodeFormatProfile.SOFT, new AdditionalFormatterParameters(TreatTextAfterLastNodeAsIncorrect: false));
            });
            return null;
        }));
        return true;
    }

    private string GetNewLineText(ITextControl textControl)
    {
        return GetNewLineText(textControl.Document.GetPsiSourceFile(Solution));
    }

    private string GetIndentText(ITextControl textControl, int indentSize)
    {
        if (indentSize == 0)
            return string.Empty;

        var sb = new StringBuilder();
        switch (GetIndentType(textControl))
        {
            case IndentStyle.Tab:
                for (var i = 0; i < indentSize; i++)
                    sb.Append("\t");
                break;
            case IndentStyle.Space:
                var configIndentSize = GetIndentSize(textControl);
                for (var i = 0; i < indentSize; i++)
                for (int j = 0; j < configIndentSize; j++)
                    sb.Append(" ");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return sb.ToString();
    }

    private string ComputeIndentOfCurrentKeyword(CachingLexer cachingLexer)
    {
        if (cachingLexer.CurrentPosition == 0)
            return string.Empty;
        cachingLexer.SetCurrentToken(cachingLexer.CurrentPosition - 1);
        if (cachingLexer.TokenType == GherkinTokenTypes.WHITE_SPACE)
            return cachingLexer.GetTokenText();
        return string.Empty;
    }

    private TokenNodeType FindLastKeywordToken(CachingLexer cachingLexer, int caret, out bool inTable)
    {
        cachingLexer.FindTokenAt(caret - 1);
        inTable = false;
        while (!GherkinTokenTypes.KEYWORDS[cachingLexer.TokenType])
        {
            if (cachingLexer.TokenType == GherkinTokenTypes.TABLE_CELL)
                inTable = true;
            if (cachingLexer.CurrentPosition == 0)
                return null;
            cachingLexer.SetCurrentToken(cachingLexer.CurrentPosition - 1);
        }
        return cachingLexer.TokenType;
    }

    private int FindPreviousRowFirstPipeIndent(CachingLexer cachingLexer, int caret)
    {
        //find previous enter
        cachingLexer.FindTokenAt(caret - 1);
        while (GherkinTokenTypes.NEW_LINE != cachingLexer.TokenType)
        {
            if (cachingLexer.TokenType == null)
                return 0;
            cachingLexer.SetCurrentToken(cachingLexer.CurrentPosition - 1);
        }

        var enterStart = cachingLexer.TokenStart;

        cachingLexer.SetCurrentToken(cachingLexer.CurrentPosition + 1);

        //find the first pipe after the enter
        while (GherkinTokenTypes.PIPE != cachingLexer.TokenType)
        {
            if (cachingLexer.TokenType == null || GherkinTokenTypes.NEW_LINE == cachingLexer.TokenType)
                return 0;
            cachingLexer.SetCurrentToken(cachingLexer.CurrentPosition + 1);
        }

        var pipeStart = cachingLexer.TokenStart;

        return pipeStart - enterStart -1;
    }


    private IndentStyle GetIndentType(ITextControl textControl)
    {
        return GetFormatSettingsKey(textControl).INDENT_STYLE;
    }

    private int GetIndentSize(ITextControl textControl)
    {
        return GetFormatSettingsKey(textControl).INDENT_SIZE;
    }

    private GherkinFormatSettingsKey GetFormatSettingsKey(ITextControl textControl)
    {
        var document = textControl.Document;
        var sourceFile = document.GetPsiSourceFile(Solution).NotNull("psiSourceFile is null for {0}", document);
        var formatSettingsKeyBase = sourceFile.GetFormatterSettings(sourceFile.PrimaryPsiLanguage);
        return (GherkinFormatSettingsKey) formatSettingsKeyBase;
    }
}