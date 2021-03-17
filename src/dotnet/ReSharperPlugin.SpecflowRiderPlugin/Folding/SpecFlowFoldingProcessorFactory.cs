using JetBrains.ReSharper.Daemon.CodeFolding;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Folding
{
    [Language(typeof(GherkinLanguage))]
    public class SpecFlowFoldingProcessorFactory : ICodeFoldingProcessorFactory
    {
        public ICodeFoldingProcessor CreateProcessor()
        {
            return new SpecFlowFoldingProcessor();
        }
    }
}
