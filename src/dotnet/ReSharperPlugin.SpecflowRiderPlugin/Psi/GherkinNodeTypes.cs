using JetBrains.Diagnostics;
using JetBrains.Rd.Impl;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public static class GherkinNodeTypes
    {
        public static GherkinNodeType FILE = new GherkinFileNodeType("FILE", 3001);
        public static GherkinNodeType TAG = new GherkinTagNodeType("TAG", 3002);

        class GherkinFileNodeType : GherkinNodeType
        {
            public GherkinFileNodeType(string name, int index) : base(name, index)
            {
            }

            public override CompositeElement Create()
            {
                return new GherkinFile();
            }
        }
        
        class GherkinTagNodeType : GherkinNodeType
        {
            public GherkinTagNodeType(string name, int index) : base(name, index)
            {
            }

            public override CompositeElement Create()
            {
                Protocol.TraceLogger.Log(LoggingLevel.INFO, $"Created tag element");

                return new GherkinTag();
            }
        }
        
    }
}