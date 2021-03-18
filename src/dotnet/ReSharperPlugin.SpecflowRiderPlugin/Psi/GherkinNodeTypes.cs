using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
// ReSharper disable InconsistentNaming

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
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

        private class GherkinFileNodeType : GherkinNodeType
        {
            public GherkinFileNodeType(string name, int index) : base(name, index)
            {
            }

            public override CompositeElement Create(object userData)
            {
                return new GherkinFile((GherkinFile.FileMetadata) userData);
            }
        }
        
        private class GherkinTagNodeType : GherkinNodeType<GherkinTag>
        {
            public GherkinTagNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
        private class GherkinFeatureNodeType : GherkinNodeType<GherkinFeature>
        {
            public GherkinFeatureNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
        private class GherkinFeatureHeaderNodeType : GherkinNodeType<GherkinFeatureHeader>
        {
            public GherkinFeatureHeaderNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
        private class GherkinScenarioNodeType : GherkinNodeType<GherkinScenario>
        {
            public GherkinScenarioNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
        private class GherkinScenarioOutlineNodeType : GherkinNodeType<GherkinScenarioOutline>
        {
            public GherkinScenarioOutlineNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
        private class GherkinStepNodeType : GherkinNodeType
        {
            public GherkinStepNodeType(string name, int index) : base(name, index)
            {
            }

            public override CompositeElement Create(object userData)
            {
                var (stepKind, effectiveStepKind) = ((GherkinStepKind stepKind, GherkinStepKind effectiveStepKind)) userData;
                return new GherkinStep(stepKind, effectiveStepKind);
            }
        }
        
        private class GherkinStepParameterNodeType : GherkinNodeType<GherkinStepParameter>
        {
            public GherkinStepParameterNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
        private class GherkinPystringNodeType : GherkinNodeType<GherkinPystring>
        {
            public GherkinPystringNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
        private class GherkinExamplesBlockNodeType : GherkinNodeType<GherkinExamplesBlock>
        {
            public GherkinExamplesBlockNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
        private class GherkinTableCellNodeType : GherkinNodeType<GherkinTableCell>
        {
            public GherkinTableCellNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
        private class GherkinTableHeaderRowNodeType : GherkinNodeType<GherkinTableHeaderRow>
        {
            public GherkinTableHeaderRowNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
        private class GherkinTableRowNodeType : GherkinNodeType<GherkinTableRow>
        {
            public GherkinTableRowNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
        private class GherkinTableNodeType : GherkinNodeType<GherkinTable>
        {
            public GherkinTableNodeType(string name, int index) : base(name, index)
            {
            }
        }
        
        private class GherkinRuleNodeType : GherkinNodeType<GherkinRule>
        {
            public GherkinRuleNodeType(string name, int index) : base(name, index)
            {
            }
        }

        private class GherkinLanguageCommentNodeType : GherkinNodeType
        {
            public GherkinLanguageCommentNodeType(string name, int index) : base(name, index)
            {
            }

            public override CompositeElement Create(object userData)
            {
                return new GherkinLanguageComment(userData as string);
            }
        }
    }
}