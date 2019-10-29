namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinTokenTypes
    {
        public static GherkinTokenType WHITE_SPACE = new GherkinTokenType("WHITE_SPACE", 1001);
        public static GherkinTokenType COMMENT = new GherkinTokenType("COMMENT", 1002);
        public static GherkinTokenType TEXT = new GherkinTokenType("TEXT", 1003);
        public static GherkinTokenType EXAMPLES_KEYWORD = new GherkinTokenType("EXAMPLES_KEYWORD", 1004);
        public static GherkinTokenType FEATURE_KEYWORD = new GherkinTokenType("FEATURE_KEYWORD", 1005);
        public static GherkinTokenType RULE_KEYWORD = new GherkinTokenType("RULE_KEYWORD", 1006);
        public static GherkinTokenType BACKGROUND_KEYWORD = new GherkinTokenType("BACKGROUND_KEYWORD", 1007);
        public static GherkinTokenType SCENARIO_KEYWORD = new GherkinTokenType("SCENARIO_KEYWORD", 1008);
        public static GherkinTokenType EXAMPLE_KEYWORD = new GherkinTokenType("EXAMPLE_KEYWORD", 1009);
        public static GherkinTokenType SCENARIO_OUTLINE_KEYWORD = new GherkinTokenType("SCENARIO_OUTLINE_KEYWORD", 1010);
        public static GherkinTokenType STEP_KEYWORD = new GherkinTokenType("STEP_KEYWORD", 1011);
        public static GherkinTokenType STEP_PARAMETER_BRACE = new GherkinTokenType("STEP_PARAMETER_BRACE", 1012);
        public static GherkinTokenType STEP_PARAMETER_TEXT = new GherkinTokenType("STEP_PARAMETER_TEXT", 1013);
        public static GherkinTokenType COLON = new GherkinTokenType("COLON", 1014);
        public static GherkinTokenType TAG = new GherkinTokenType("TAG", 1015);
        public static GherkinTokenType PYSTRING = new GherkinTokenType("PYSTRING_QUOTES", 1016);
        public static GherkinTokenType PYSTRING_TEXT = new GherkinTokenType("PYSTRING_TEXT", 1017);
        public static GherkinTokenType PIPE = new GherkinTokenType("PIPE", 1018);
        public static GherkinTokenType TABLE_CELL = new GherkinTokenType("TABLE_CELL", 1019);

//        TokenSet KEYWORDS = TokenSet.create(FEATURE_KEYWORD, RULE_KEYWORD, EXAMPLE_KEYWORD,
//                                            BACKGROUND_KEYWORD, SCENARIO_KEYWORD, SCENARIO_OUTLINE_KEYWORD,
//                                            EXAMPLES_KEYWORD, EXAMPLES_KEYWORD,
//                                            STEP_KEYWORD);

//        TokenSet SCENARIOS_KEYWORDS = TokenSet.create(SCENARIO_KEYWORD, SCENARIO_OUTLINE_KEYWORD, EXAMPLE_KEYWORD);
    }
}