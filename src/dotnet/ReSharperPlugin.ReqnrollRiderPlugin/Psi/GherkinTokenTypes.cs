using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
// ReSharper disable InconsistentNaming

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public static class GherkinTokenTypes
{
    public static readonly GherkinTokenType WHITE_SPACE = new GherkinWhitespaceTokenType("WHITE_SPACE", 1001);
    public static readonly GherkinTokenType COMMENT = new GherkinTokenType("COMMENT", 1002);
    public static readonly GherkinTokenType TEXT = new GherkinTokenType("TEXT", 1003);
    public static readonly GherkinTokenType EXAMPLES_KEYWORD = new GherkinTokenType("EXAMPLES_KEYWORD", 1004);
    public static readonly GherkinTokenType FEATURE_KEYWORD = new GherkinTokenType("FEATURE_KEYWORD", 1005);
    public static readonly GherkinTokenType RULE_KEYWORD = new GherkinTokenType("RULE_KEYWORD", 1006);
    public static readonly GherkinTokenType BACKGROUND_KEYWORD = new GherkinTokenType("BACKGROUND_KEYWORD", 1007);
    public static readonly GherkinTokenType SCENARIO_KEYWORD = new GherkinTokenType("SCENARIO_KEYWORD", 1008);
    public static readonly GherkinTokenType EXAMPLE_KEYWORD = new GherkinTokenType("EXAMPLE_KEYWORD", 1009);
    public static readonly GherkinTokenType SCENARIO_OUTLINE_KEYWORD = new GherkinTokenType("SCENARIO_OUTLINE_KEYWORD", 1010);
    public static readonly GherkinTokenType STEP_KEYWORD = new GherkinTokenType("STEP_KEYWORD", 1011);
    public static readonly GherkinTokenType STEP_PARAMETER_BRACE = new GherkinTokenType("STEP_PARAMETER_BRACE", 1012);
    public static readonly GherkinTokenType STEP_PARAMETER_TEXT = new GherkinTokenType("STEP_PARAMETER_TEXT", 1013);
    public static readonly GherkinTokenType COLON = new GherkinTokenType("COLON", 1014);
    public static readonly GherkinTokenType TAG = new GherkinTokenType("TAG", 1015);
    public static readonly GherkinTokenType PYSTRING = new GherkinTokenType("PYSTRING_QUOTES", 1016);
    public static readonly GherkinTokenType PYSTRING_TEXT = new GherkinTokenType("PYSTRING_TEXT", 1017);
    public static readonly GherkinTokenType PIPE = new GherkinTokenType("PIPE", 1018);
    public static readonly GherkinTokenType TABLE_CELL = new GherkinTokenType("TABLE_CELL", 1019);
    public static readonly GherkinTokenType NEW_LINE = new GherkinWhitespaceTokenType("NEW_LINE", 1020);

    public static readonly NodeTypeSet KEYWORDS = new NodeTypeSet(FEATURE_KEYWORD, RULE_KEYWORD, EXAMPLE_KEYWORD,
        BACKGROUND_KEYWORD, SCENARIO_KEYWORD, SCENARIO_OUTLINE_KEYWORD,
        EXAMPLES_KEYWORD, EXAMPLES_KEYWORD,
        STEP_KEYWORD);

    public static readonly NodeTypeSet SCENARIOS_KEYWORDS = new NodeTypeSet(SCENARIO_KEYWORD, SCENARIO_OUTLINE_KEYWORD, EXAMPLE_KEYWORD);

    public class GherkinWhitespaceTokenType(string s, int index) : GherkinTokenType(s, index)
    {

        public override bool IsWhitespace => true;
    }
}