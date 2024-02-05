using JetBrains.ReSharper.Daemon.CodeFolding;
using JetBrains.ReSharper.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Folding
{
    [Language(typeof(GherkinLanguage))]
    public class ReqnrollFoldingProcessorFactory : ICodeFoldingProcessorFactory
    {
        public ICodeFoldingProcessor CreateProcessor()
        {
            return new ReqnrollFoldingProcessor();
        }
    }
}
