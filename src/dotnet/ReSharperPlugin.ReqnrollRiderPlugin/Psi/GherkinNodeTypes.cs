using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
// ReSharper disable InconsistentNaming

namespace ReSharperPlugin.ReqnrollRiderPlugin.Psi;

public static class GherkinNodeTypes
{
    private static int _lastNodeId = 3000;
    private static int NextId => ++_lastNodeId;
        
    public static readonly GherkinNodeType FILE = new GherkinFileNodeType("FILE", NextId);
    public static readonly GherkinNodeType TAG = new GherkinTagNodeType("TAG", NextId);
    public static readonly GherkinNodeType FEATURE_HEADER = new GherkinFeatureHeaderNodeType("FEATURE_HEADER", NextId);
    public static readonly GherkinNodeType FEATURE = new GherkinFeatureNodeType("FEATURE", NextId);
    public static readonly GherkinNodeType SCENARIO = new GherkinScenarioNodeType("SCENARIO", NextId);
    public static readonly GherkinNodeType SCENARIO_OUTLINE = new GherkinScenarioOutlineNodeType("SCENARIO_OUTLINE", NextId);
    public static readonly GherkinNodeType STEP = new GherkinStepNodeType("STEP", NextId);
    public static readonly GherkinNodeType STEP_PARAMETER = new GherkinStepParameterNodeType("STEP_PARAMETER", NextId);
    public static readonly GherkinNodeType PYSTRING = new GherkinPystringNodeType("PYSTRING", NextId);
    public static readonly GherkinNodeType EXAMPLES_BLOCK = new GherkinExamplesBlockNodeType("EXAMPLES_BLOCK", NextId);
    public static readonly GherkinNodeType TABLE_CELL = new GherkinTableCellNodeType("TABLE_CELL", NextId);
    public static readonly GherkinNodeType TABLE_HEADER_ROW = new GherkinTableHeaderRowNodeType("TABLE_HEADER_ROW", NextId);
    public static readonly GherkinNodeType TABLE_ROW = new GherkinTableRowNodeType("TABLE_ROW", NextId);
    public static readonly GherkinNodeType TABLE = new GherkinTableNodeType("TABLE", NextId);
    public static readonly GherkinNodeType RULE = new GherkinRuleNodeType("RULE", NextId);
    public static readonly GherkinNodeType LANGUAGE_COMMENT = new GherkinLanguageCommentNodeType("LANGUAGE_COMMENT", NextId);
    public static readonly GherkinNodeType COMMENT = new GherkinCommentNodeType("COMMENT", NextId);

    private class GherkinFileNodeType(string name, int index) : GherkinNodeType(name, index)
    {

        public override CompositeElement Create(object userData)
        {
            return new GherkinFile((GherkinFile.FileMetadata) userData);
        }
    }
        
    private class GherkinTagNodeType(string name, int index) : GherkinNodeType<GherkinTag>(name, index);
        
    private class GherkinFeatureNodeType(string name, int index) : GherkinNodeType<GherkinFeature>(name, index);
        
    private class GherkinFeatureHeaderNodeType(string name, int index) : GherkinNodeType<GherkinFeatureHeader>(name, index);
        
    private class GherkinScenarioNodeType(string name, int index) : GherkinNodeType<GherkinScenario>(name, index);
        
    private class GherkinScenarioOutlineNodeType(string name, int index) : GherkinNodeType<GherkinScenarioOutline>(name, index);
        
    private class GherkinStepNodeType(string name, int index) : GherkinNodeType(name, index)
    {

        public override CompositeElement Create(object userData)
        {
            var (stepKind, effectiveStepKind) = ((GherkinStepKind stepKind, GherkinStepKind effectiveStepKind)) userData;
            return new GherkinStep(stepKind, effectiveStepKind);
        }
    }
        
    private class GherkinStepParameterNodeType(string name, int index) : GherkinNodeType<GherkinStepParameter>(name, index);
        
    private class GherkinPystringNodeType(string name, int index) : GherkinNodeType<GherkinPystring>(name, index);
        
    private class GherkinExamplesBlockNodeType(string name, int index) : GherkinNodeType<GherkinExamplesBlock>(name, index);
        
    private class GherkinTableCellNodeType(string name, int index) : GherkinNodeType<GherkinTableCell>(name, index);
        
    private class GherkinTableHeaderRowNodeType(string name, int index) : GherkinNodeType<GherkinTableHeaderRow>(name, index);
        
    private class GherkinTableRowNodeType(string name, int index) : GherkinNodeType<GherkinTableRow>(name, index);
        
    private class GherkinTableNodeType(string name, int index) : GherkinNodeType<GherkinTable>(name, index);
        
    private class GherkinRuleNodeType(string name, int index) : GherkinNodeType<GherkinRule>(name, index);

    private class GherkinLanguageCommentNodeType(string name, int index) : GherkinNodeType(name, index)
    {

        public override CompositeElement Create(object userData)
        {
            return new GherkinLanguageComment(userData as string);
        }
    }

    private class GherkinCommentNodeType(string name, int index) : GherkinNodeType(name, index)
    {

        public override CompositeElement Create(object userData)
        {
            return new GherkinComment(userData as string);
        }
    }
}