using System.Text;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.Rd.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.TreeBuilder;
using JetBrains.Text;

namespace ReSharperPlugin.SpecflowRiderPlugin.Psi
{
    public class GherkinParser : IParser
    {
        private readonly IPsiModule _module;
        private readonly IPsiSourceFile _sourceFile;
        private readonly PsiBuilder _builder;

        public GherkinParser(ILexer lexer, IPsiModule module, IPsiSourceFile sourceFile)
        {
            _module = module;
            _sourceFile = sourceFile;
            _builder = new PsiBuilder(lexer, GherkinNodeTypes.FILE, null, Lifetime.Eternal);
        }

        public IFile ParseFile()
        {
            Protocol.TraceLogger.Log(LoggingLevel.INFO, $"Started parsing.");
            var fileMarker = _builder.Mark();
            
            while (!_builder.Eof())
            {
                var tokenType = _builder.GetTokenType();
                var token = _builder.GetToken();
                
//                Protocol.TraceLogger.Log(LoggingLevel.INFO,
//                                         $"{tokenType}[{token.Start} - {token.End}]: {_builder.GetTokenText()}");
                if (tokenType == GherkinTokenTypes.FEATURE_KEYWORD)
                {
                    ParseFeature();
                }
                else if (tokenType == GherkinTokenTypes.TAG)
                {
                    ParseTags();
                }
                else
                {
                    _builder.AdvanceLexer();
                }
            }

            _builder.Done(fileMarker, GherkinNodeTypes.FILE, null);
            var resultTree = _builder.BuildTree();

            Protocol.TraceLogger.Log(LoggingLevel.INFO, $"Finished parsing: {resultTree}. Content:");

//            var sb = new StringBuilder();
//            foreach (var descendant in resultTree.Descendants())
//                sb.AppendLine(descendant.ToString());
//            
//            Protocol.TraceLogger.Log(LoggingLevel.INFO, $"{sb}");

            return (IFile) resultTree;
        }

        private void ParseTags()
        {
            while (_builder.GetTokenType() == GherkinTokenTypes.TAG)
            {
                var tagMarker = _builder.Mark();
                _builder.AdvanceLexer();
                _builder.Done(tagMarker, GherkinNodeTypes.TAG, null);
            }
        }

        private void ParseFeature()
        {
            var token = _builder.GetToken();

            Protocol.TraceLogger.Log(LoggingLevel.INFO,
                                     $"Should parse feature: [{token.Start} - {token.End}]: {_builder.GetTokenText()}");

            _builder.AdvanceLexer();
//            throw new System.NotImplementedException();
        }
    }
}