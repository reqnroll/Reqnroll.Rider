using System.Text.RegularExpressions;
using JetBrains.Annotations;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.TreeBuilder;
using ReSharperPlugin.ReqnrollRiderPlugin.Caching.ReqnrollJsonSettings;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public class GherkinParser : IParser
{
    // ReSharper disable once InconsistentNaming
    private static readonly NodeTypeSet SCENARIO_END_TOKENS = new NodeTypeSet(GherkinTokenTypes.BACKGROUND_KEYWORD,
        GherkinTokenTypes.SCENARIO_KEYWORD,
        GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD,
        GherkinTokenTypes.RULE_KEYWORD,
        GherkinTokenTypes.FEATURE_KEYWORD);
    private static readonly Regex LanguagePattern = new Regex ("^\\s*#\\s*language\\s*:\\s*(?<lang>[a-zA-Z\\-_]+)\\s*$", RegexOptions.Compiled);

    private readonly ILexer _lexer;
    private readonly IPsiSourceFile _sourceFile;
    private readonly GherkinKeywordProvider _keywordProvider;
    private GherkinStepKind _lastStepKind = GherkinStepKind.Given;
    [CanBeNull] private string _lang;

    // sourceFile is null in the preview of the code style (Settings)
    public GherkinParser(ILexer lexer, [CanBeNull] IPsiSourceFile sourceFile, ReqnrollSettingsProvider settingsProvider, GherkinKeywordProvider keywordProvider)
    {
        var settings = settingsProvider.GetSettings(sourceFile?.GetProject());
        _lang = settings.Language.NeutralFeature;
        _lexer = lexer;
        _sourceFile = sourceFile;
        _keywordProvider = keywordProvider;
    }

    public IFile ParseFile()
    {
        using (var lifetimeDefinition = Lifetime.Define())
        {
            var builder = new PsiBuilder(_lexer, GherkinNodeTypes.FILE, null, lifetimeDefinition.Lifetime);
            var fileMarker = builder.Mark();

            while (!builder.Eof())
            {
                var tokenType = builder.GetTokenType();

                if (tokenType == GherkinTokenTypes.FEATURE_KEYWORD)
                    ParseFeature(builder);
                else if (tokenType == GherkinTokenTypes.TAG)
                    ParseTags(builder);
                else if (tokenType == GherkinTokenTypes.COMMENT)
                    ParseTopComments(builder);
                else
                    builder.AdvanceLexer();
            }

            builder.Done(fileMarker, GherkinNodeTypes.FILE, new GherkinFile.FileMetadata(_sourceFile?.Name, _lang));
            var resultTree = (GherkinFile) builder.BuildTree();

            return resultTree;
        }
    }

    private void ParseTopComments(PsiBuilder builder)
    {
        while (builder.GetTokenType() == GherkinTokenTypes.COMMENT)
        {
            var commentMarker = builder.Mark();
            var commentText = builder.GetTokenText();
            builder.AdvanceLexer();
            var match = LanguagePattern.Match(commentText);
            if (match.Success)
            {
                _lang = match.Groups["lang"].Value;
                builder.DoneBeforeWhitespaces(commentMarker, GherkinNodeTypes.LANGUAGE_COMMENT, _lang);
            }
            else
                builder.DoneBeforeWhitespaces(commentMarker, GherkinNodeTypes.COMMENT, commentText);

            if (builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE
                || builder.GetTokenType() == GherkinTokenTypes.NEW_LINE)
                builder.AdvanceLexer();
        }
    }

    private static void ParseComment(PsiBuilder builder)
    {
        while (builder.GetTokenType() == GherkinTokenTypes.COMMENT)
        {
            var commentMarker = builder.Mark();
            var commentText = builder.GetTokenText();
            builder.AdvanceLexer();
            builder.DoneBeforeWhitespaces(commentMarker, GherkinNodeTypes.COMMENT, commentText);
            SkipWhitespace(builder);
        }
    }

    private void ParseTags(PsiBuilder builder)
    {
        while (builder.GetTokenType() == GherkinTokenTypes.TAG)
        {
            var tagMarker = builder.Mark();
            builder.AdvanceLexer();
            builder.DoneBeforeWhitespaces(tagMarker, GherkinNodeTypes.TAG, null);

            SkipWhitespace(builder);
            ParseComment(builder);
        }
    }

    private void ParseFeature(PsiBuilder builder)
    {
        var featureMarker = builder.Mark();

        Assertion.Assert(builder.GetTokenType() == GherkinTokenTypes.FEATURE_KEYWORD,
            "_builder.GetTokenType() == GherkinTokenTypes.FEATURE_KEYWORD");

        int? descMarker = null;
        bool wasLineBreak = false;
        do
        {
            builder.AdvanceLexer();
            ParseComment(builder);

            var tokenType = builder.GetTokenType();
            if (tokenType == GherkinTokenTypes.TEXT && descMarker == null)
            {
                if (wasLineBreak)
                    descMarker = builder.Mark();
            }

            if (GherkinTokenTypes.SCENARIOS_KEYWORDS[tokenType] ||
                tokenType == GherkinTokenTypes.RULE_KEYWORD ||
                tokenType == GherkinTokenTypes.BACKGROUND_KEYWORD ||
                tokenType == GherkinTokenTypes.TAG)
            {
                if (descMarker != null)
                {
                    builder.DoneBeforeWhitespaces(descMarker.Value, GherkinNodeTypes.FEATURE_HEADER, null);
                    descMarker = null;
                }

                ParseFeatureElements(builder);
            }

            wasLineBreak = IsLineBreak(builder);
            if (wasLineBreak)
                SkipGroupedWhiteSpaces(builder);
        } while (builder.GetTokenType() != GherkinTokenTypes.FEATURE_KEYWORD && !builder.Eof());

        if (descMarker != null)
            builder.DoneBeforeWhitespaces(descMarker.Value, GherkinNodeTypes.FEATURE_HEADER, null);

        builder.DoneBeforeWhitespaces(featureMarker, GherkinNodeTypes.FEATURE, null);
    }

    private void ParseFeatureElements(PsiBuilder builder)
    {
        int? ruleMarker = null;
        while (builder.GetTokenType() != GherkinTokenTypes.FEATURE_KEYWORD && !builder.Eof())
        {
            if (builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE
                || builder.GetTokenType() == GherkinTokenTypes.NEW_LINE)
            {
                builder.AdvanceLexer();
                continue;
            }
            ParseComment(builder);

            ruleMarker = ParseRule(builder, ruleMarker);
            ParseScenario(builder);
        }

        if (ruleMarker.HasValue)
            builder.DoneBeforeWhitespaces(ruleMarker.Value, GherkinNodeTypes.RULE, null);
    }

    private static int? ParseRule(PsiBuilder builder, int? ruleMarker)
    {
        if (builder.GetTokenType() == GherkinTokenTypes.RULE_KEYWORD)
        {
            if (ruleMarker != null)
                builder.DoneBeforeWhitespaces(ruleMarker.Value, GherkinNodeTypes.RULE, null);

            ruleMarker = builder.Mark();
            builder.AdvanceLexer();
            if (builder.GetTokenType() == GherkinTokenTypes.COLON)
                builder.AdvanceLexer();
            else
                return ruleMarker;

            while (builder.GetTokenType() == GherkinTokenTypes.TEXT ||
                   builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE ||
                   builder.GetTokenType() == GherkinTokenTypes.NEW_LINE)
                builder.AdvanceLexer();
        }

        return ruleMarker;
    }

    private void ParseScenario(PsiBuilder builder)
    {
        SkipWhitespace(builder);
        if (builder.Eof())
            return;
        var scenarioMarker = builder.Mark();
        ParseTags(builder);
        ParseComment(builder);

        // scenarios
        var startTokenType = builder.GetTokenType();
        var outline = startTokenType == GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD;

        if (!builder.Eof())
            builder.AdvanceLexer();

        while (!AtScenarioEnd(builder))
        {
            SkipWhitespace(builder);
            ParseTags(builder);
            ParseComment(builder);

            if (ParseStepParameter(builder))
                continue;

            if (builder.GetTokenType() == GherkinTokenTypes.STEP_KEYWORD)
                ParseStep(builder);
            else if (builder.GetTokenType() == GherkinTokenTypes.EXAMPLES_KEYWORD)
                ParseExamplesBlock(builder);
            else
                builder.AdvanceLexer();
        }

        builder.DoneBeforeWhitespaces(scenarioMarker, outline ? GherkinNodeTypes.SCENARIO_OUTLINE : GherkinNodeTypes.SCENARIO, null);
    }

    private void ParseStep(PsiBuilder builder)
    {
        var marker = builder.Mark();
        var keywordText = builder.GetTokenText();
        builder.AdvanceLexer();
        while (builder.GetTokenType() == GherkinTokenTypes.TEXT ||
               builder.GetTokenType() == GherkinTokenTypes.STEP_PARAMETER_BRACE ||
               builder.GetTokenType() == GherkinTokenTypes.STEP_PARAMETER_TEXT ||
               builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE ||
               builder.GetTokenType() == GherkinTokenTypes.NEW_LINE)
        {
            if (IsLineBreak(builder))
                break;

            if (!ParseStepParameter(builder))
                builder.AdvanceLexer();
        }

        SkipGroupedWhiteSpaces(builder);

        var nextToken = builder.GetTokenType(1);
        if (nextToken == GherkinTokenTypes.PIPE)
        {
            builder.AdvanceLexer();
            ParseTable(builder);
        }
        else if (nextToken == GherkinTokenTypes.PYSTRING)
        {
            builder.AdvanceLexer();
            ParsePystring(builder);
        }

        var stepKind = _keywordProvider.GetStepKind(_lang, keywordText);
        var effectiveStepKind = stepKind;
        if (stepKind == GherkinStepKind.And)
            effectiveStepKind = _lastStepKind;
        else if (stepKind == GherkinStepKind.Given || stepKind == GherkinStepKind.When || stepKind == GherkinStepKind.Then)
            _lastStepKind = effectiveStepKind;
        builder.DoneBeforeWhitespaces(marker, GherkinNodeTypes.STEP, (stepKind, effectiveStepKind));
    }

    private static void ParseExamplesBlock(PsiBuilder builder)
    {
        var marker = builder.Mark();
        builder.AdvanceLexer();
        if (builder.GetTokenType() == GherkinTokenTypes.COLON)
            builder.AdvanceLexer();

        while (builder.GetTokenType() == GherkinTokenTypes.TEXT ||
               builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE ||
               builder.GetTokenType() == GherkinTokenTypes.NEW_LINE)
            builder.AdvanceLexer();

        if (builder.GetTokenType() == GherkinTokenTypes.PIPE)
            ParseTable(builder);

        builder.DoneBeforeWhitespaces(marker, GherkinNodeTypes.EXAMPLES_BLOCK, null);
    }

    private static void ParseTable(PsiBuilder builder)
    {
        var marker = builder.Mark();
        var rowMarker = builder.Mark();
        var headerNodeType = GherkinNodeTypes.TABLE_HEADER_ROW;
        int? cellMarker = null;

        var wasLineBreak = false;
        var possibleEmptyCell = false;
        while (builder.GetTokenType() == GherkinTokenTypes.PIPE ||
               builder.GetTokenType() == GherkinTokenTypes.TABLE_CELL ||
               builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE ||
               builder.GetTokenType() == GherkinTokenTypes.COMMENT ||
               builder.GetTokenType() == GherkinTokenTypes.NEW_LINE)
        {
            var tokenType = builder.GetTokenType();
            if (tokenType == GherkinTokenTypes.TABLE_CELL && cellMarker == null)
            {
                cellMarker = builder.Mark();
            }
            else if (tokenType != GherkinTokenTypes.TABLE_CELL && cellMarker != null)
            {
                builder.DoneBeforeWhitespaces(cellMarker.Value, GherkinNodeTypes.TABLE_CELL, null);
                cellMarker = null;
                possibleEmptyCell = false;
            }

            if (tokenType == GherkinTokenTypes.PIPE)
            {
                if (wasLineBreak)
                {
                    possibleEmptyCell = true;
                    builder.DoneBeforeWhitespaces(rowMarker, headerNodeType, null);
                    headerNodeType = GherkinNodeTypes.TABLE_ROW;
                    rowMarker = builder.Mark();
                }
                else
                {
                    if (possibleEmptyCell)
                    {
                        cellMarker = builder.Mark();
                        builder.DoneBeforeWhitespaces(cellMarker.Value, GherkinNodeTypes.TABLE_CELL, null);
                        cellMarker = null;
                    }

                    possibleEmptyCell = true;
                }
            }

            wasLineBreak = IsLineBreak(builder);
            if (wasLineBreak)
                SkipGroupedWhiteSpaces(builder);
            builder.AdvanceLexer();
        }

        if (cellMarker.HasValue)
            builder.DoneBeforeWhitespaces(cellMarker.Value, GherkinNodeTypes.TABLE_CELL, null);

        builder.DoneBeforeWhitespaces(rowMarker, headerNodeType, null);
        builder.DoneBeforeWhitespaces(marker, GherkinNodeTypes.TABLE, null);
    }

    private static void ParsePystring(PsiBuilder builder)
    {
        if (!builder.Eof())
        {
            var marker = builder.Mark();
            builder.AdvanceLexer();
            var nonSpaceFoundAfterNewLine = true;
            while (!builder.Eof())
            {
                if (!nonSpaceFoundAfterNewLine && builder.GetTokenType() == GherkinTokenTypes.PYSTRING)
                    break;
                if (builder.GetTokenType() == GherkinTokenTypes.NEW_LINE)
                    nonSpaceFoundAfterNewLine = false;
                else if (builder.GetTokenType() != GherkinTokenTypes.WHITE_SPACE)
                    nonSpaceFoundAfterNewLine = true;

                if (!ParseStepParameter(builder))
                    builder.AdvanceLexer();
            }

            if (!builder.Eof())
                builder.AdvanceLexer();

            builder.Done(marker, GherkinNodeTypes.PYSTRING, null);
        }
    }

    private static bool ParseStepParameter(PsiBuilder builder)
    {
        if (builder.GetTokenType() != GherkinTokenTypes.STEP_PARAMETER_TEXT)
            return false;

        var stepParameterMarker = builder.Mark();
        builder.AdvanceLexer();
        builder.Done(stepParameterMarker, GherkinNodeTypes.STEP_PARAMETER, null);
        return true;
    }

    private static bool AtScenarioEnd(PsiBuilder builder)
    {
        var i = 0;
        while (builder.GetTokenType(i) == GherkinTokenTypes.TAG ||
               builder.GetTokenType(i) == GherkinTokenTypes.WHITE_SPACE ||
               builder.GetTokenType(i) == GherkinTokenTypes.COMMENT ||
               builder.GetTokenType(i) == GherkinTokenTypes.NEW_LINE)
            i++;

        var tokenType = builder.GetTokenType(i);
        return tokenType == null || SCENARIO_END_TOKENS[tokenType];
    }

    private static bool IsLineBreak(PsiBuilder builder)
    {
        return builder.GetTokenType() == GherkinTokenTypes.NEW_LINE;
    }

    private static void SkipGroupedWhiteSpaces(PsiBuilder builder)
    {
        while (builder.GetTokenType(1) == GherkinTokenTypes.WHITE_SPACE
               || builder.GetTokenType(1) == GherkinTokenTypes.NEW_LINE)
        {
            builder.AdvanceLexer();
        }
    }

    private static void SkipWhitespace(PsiBuilder builder)
    {
        while (builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE
               || builder.GetTokenType() == GherkinTokenTypes.NEW_LINE)
        {
            builder.AdvanceLexer();
        }
    }
}