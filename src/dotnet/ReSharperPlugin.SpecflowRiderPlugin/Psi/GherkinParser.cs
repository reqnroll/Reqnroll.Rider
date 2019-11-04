using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.TreeBuilder;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinParser : IParser
    {
        // ReSharper disable once InconsistentNaming
        private static readonly NodeTypeSet SCENARIO_END_TOKENS = new NodeTypeSet(
            GherkinTokenTypes.BACKGROUND_KEYWORD,
            GherkinTokenTypes.SCENARIO_KEYWORD,
            GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD,
            GherkinTokenTypes.RULE_KEYWORD,
            GherkinTokenTypes.FEATURE_KEYWORD);

        private readonly IPsiSourceFile _sourceFile;
        private readonly PsiBuilder _builder;

        public GherkinParser(ILexer lexer, IPsiSourceFile sourceFile)
        {
            _sourceFile = sourceFile;
            _builder = new PsiBuilder(lexer, GherkinNodeTypes.FILE, null, Lifetime.Eternal);
        }

        public IFile ParseFile()
        {
            var fileMarker = _builder.Mark();

            while (!_builder.Eof())
            {
                var tokenType = _builder.GetTokenType();

                if (tokenType == GherkinTokenTypes.FEATURE_KEYWORD)
                    ParseFeature(_builder);
                else if (tokenType == GherkinTokenTypes.TAG)
                    ParseTags(_builder);
                else
                    _builder.AdvanceLexer();
            }

            _builder.Done(fileMarker, GherkinNodeTypes.FILE, _sourceFile.Name);
            var resultTree = (GherkinFile) _builder.BuildTree();

            return resultTree;
        }

        private static void ParseTags(PsiBuilder builder)
        {
            while (builder.GetTokenType() == GherkinTokenTypes.TAG)
            {
                var tagMarker = builder.Mark();
                builder.AdvanceLexer();
                builder.Done(tagMarker, GherkinNodeTypes.TAG, null);

                if (builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE)
                    builder.AdvanceLexer();
            }
        }

        private static void ParseFeature(PsiBuilder builder)
        {
            var featureMarker = builder.Mark();

            Assertion.Assert(builder.GetTokenType() == GherkinTokenTypes.FEATURE_KEYWORD,
                "_builder.GetTokenType() == GherkinTokenTypes.FEATURE_KEYWORD");

            int? descMarker = null;
            bool wasLineBreak = false;
            do
            {
                builder.AdvanceLexer();

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
                        builder.Done(descMarker.Value, GherkinNodeTypes.FEATURE_HEADER, null);
                        descMarker = null;
                    }

                    ParseFeatureElements(builder);
                }

                wasLineBreak = IsLineBreak(builder);
            } while (builder.GetTokenType() != GherkinTokenTypes.FEATURE_KEYWORD && !builder.Eof());

            if (descMarker != null)
                builder.Done(descMarker.Value, GherkinNodeTypes.FEATURE_HEADER, null);

            builder.Done(featureMarker, GherkinNodeTypes.FEATURE, null);
        }

        private static void ParseFeatureElements(PsiBuilder builder)
        {
            int? ruleMarker = null;
            while (builder.GetTokenType() != GherkinTokenTypes.FEATURE_KEYWORD && !builder.Eof())
            {
                if (builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE)
                {
                    builder.AdvanceLexer();
                    continue;
                }
                
                ruleMarker = ParseRule(builder, ruleMarker);
                ParseScenario(builder);
            }

            if (ruleMarker.HasValue)
                builder.Done(ruleMarker.Value, GherkinNodeTypes.RULE, null);
        }

        private static int? ParseRule(PsiBuilder builder, int? ruleMarker)
        {
            if (builder.GetTokenType() == GherkinTokenTypes.RULE_KEYWORD)
            {
                if (ruleMarker != null)
                    builder.Done(ruleMarker.Value, GherkinNodeTypes.RULE, null);

                ruleMarker = builder.Mark();
                builder.AdvanceLexer();
                if (builder.GetTokenType() == GherkinTokenTypes.COLON)
                    builder.AdvanceLexer();
                else
                    return ruleMarker;

                while (builder.GetTokenType() == GherkinTokenTypes.TEXT ||
                       builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE)
                    builder.AdvanceLexer();
            }

            return ruleMarker;
        }

        private static void ParseScenario(PsiBuilder builder)
        {
            var scenarioMarker = builder.Mark();
            ParseTags(builder);

            // scenarios
            var startTokenType = builder.GetTokenType();
            var outline = startTokenType == GherkinTokenTypes.SCENARIO_OUTLINE_KEYWORD;
            builder.AdvanceLexer();

            while (!AtScenarioEnd(builder))
            {
                ParseTags(builder);
                
                if (ParseStepParameter(builder))
                    continue;

                if (builder.GetTokenType() == GherkinTokenTypes.STEP_KEYWORD)
                    ParseStep(builder);
                else if (builder.GetTokenType() == GherkinTokenTypes.EXAMPLES_KEYWORD)
                    ParseExamplesBlock(builder);
                else
                    builder.AdvanceLexer();
            }
            
            builder.Done(scenarioMarker, outline ? GherkinNodeTypes.SCENARIO_OUTLINE : GherkinNodeTypes.SCENARIO, null);
        }

        private static void ParseStep(PsiBuilder builder)
        {
            var marker = builder.Mark();
            builder.AdvanceLexer();
            while (builder.GetTokenType() == GherkinTokenTypes.TEXT ||
                   builder.GetTokenType() == GherkinTokenTypes.STEP_PARAMETER_BRACE ||
                   builder.GetTokenType() == GherkinTokenTypes.STEP_PARAMETER_TEXT ||
                   builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE)
            {
                if (IsLineBreak(builder))
                    break;

                if (!ParseStepParameter(builder))
                    builder.AdvanceLexer();
            }

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

            builder.Done(marker, GherkinNodeTypes.STEP, null);
        }

        private static void ParseExamplesBlock(PsiBuilder builder)
        {
            var marker = builder.Mark();
            builder.AdvanceLexer();
            if (builder.GetTokenType() == GherkinTokenTypes.COLON)
                builder.AdvanceLexer();

            while (builder.GetTokenType() == GherkinTokenTypes.TEXT ||
                   builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE)
                builder.AdvanceLexer();

            if (builder.GetTokenType() == GherkinTokenTypes.PIPE)
                ParseTable(builder);

            builder.Done(marker, GherkinNodeTypes.EXAMPLES_BLOCK, null);
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
                   builder.GetTokenType() == GherkinTokenTypes.WHITE_SPACE)
            {
                var tokenType = builder.GetTokenType();
                if (tokenType == GherkinTokenTypes.TABLE_CELL && cellMarker == null)
                {
                    cellMarker = builder.Mark();
                }
                else if (tokenType != GherkinTokenTypes.TABLE_CELL && cellMarker != null)
                {
                    builder.Done(cellMarker.Value, GherkinNodeTypes.TABLE_CELL, null);
                    cellMarker = null;
                    possibleEmptyCell = false;
                }
                
                if (tokenType == GherkinTokenTypes.PIPE)
                {
                    if (wasLineBreak)
                    {
                        possibleEmptyCell = true;
                        builder.Done(rowMarker, headerNodeType, null);
                        headerNodeType = GherkinNodeTypes.TABLE_ROW;
                        rowMarker = builder.Mark();
                    }
                    else
                    {
                        if (possibleEmptyCell)
                        {
                            cellMarker = builder.Mark();
                            builder.Done(cellMarker.Value, GherkinNodeTypes.TABLE_CELL, null);
                            cellMarker = null;
                        }

                        possibleEmptyCell = true;
                    }
                }

                wasLineBreak = IsLineBreak(builder);
                builder.AdvanceLexer();
            }

            if (cellMarker.HasValue)
                builder.Done(cellMarker.Value, GherkinNodeTypes.TABLE_CELL, null);

            builder.Done(rowMarker, headerNodeType, null);
            builder.Done(marker, GherkinNodeTypes.TABLE, null);
        }

        private static void ParsePystring(PsiBuilder builder)
        {
            if (!builder.Eof())
            {
                var marker = builder.Mark();
                builder.AdvanceLexer();
                while (!builder.Eof() && builder.GetTokenType() != GherkinTokenTypes.PYSTRING)
                {
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
                   builder.GetTokenType(i) == GherkinTokenTypes.WHITE_SPACE)
                i++;

            var tokenType = builder.GetTokenType(i);
            return tokenType == null || SCENARIO_END_TOKENS[tokenType];
        }

        private static bool IsLineBreak(PsiBuilder builder)
        {
            if (builder.GetTokenType() != GherkinTokenTypes.WHITE_SPACE)
                return false;
            
            return builder.GetTokenText()?.Contains("\n") == true;
        }
    }
}