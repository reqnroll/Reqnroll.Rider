using System;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Debugger;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util.dataStructures.TypedIntrinsics;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Debugger
{
    [Language(typeof(GherkinLanguage))]
    public class GherkinBreakpointVariantsProvider : IBreakpointVariantsProvider
    {
        public List<IBreakpoint> GetBreakpointVariants(IProjectFile file, int line, ISolution solution)
        {
            if (file.GetPrimaryPsiFile() is not GherkinFile gherkinFile)
                return null;
            var psiSourceFile = solution.PsiModules().GetPsiSourceFilesFor(file).FirstOrDefault();
            if (psiSourceFile == null)
                return null;

            var breakpointList = new List<IBreakpoint>();
            var lineCount = ((IConvertible) psiSourceFile.Document.GetLineCount()).ToInt32(CultureInfo.InvariantCulture);
            if (line > lineCount)
                return breakpointList;

            foreach (var gherkinStep in FindStepsAtLine(line, psiSourceFile, gherkinFile))
            {
                var (startOffset, endOffset) = gherkinStep.GetTreeTextRange();
                breakpointList.Add(new TextRangeBreakpoint(startOffset.Offset, endOffset.Offset, gherkinStep.GetText(), null));
            }

            return breakpointList;
        }

        private static IEnumerable<GherkinStep> FindStepsAtLine(int line, IPsiSourceFile psiSourceFile, GherkinFile gherkinFile)
        {
            var startLineOffset = psiSourceFile.Document.GetLineStartOffset((Int32<DocLine>) line);
            var endLineOffset = psiSourceFile.Document.GetLineEndOffsetWithLineBreak((Int32<DocLine>) line);

            return FindChildrenInRange<GherkinStep>(gherkinFile, startLineOffset, endLineOffset);
        }

        private static IEnumerable<TNode> FindChildrenInRange<TNode>(ITreeNode node, int startLineOffset, int endLineOffset)
            where TNode : class, ITreeNode
        {
            var child = node.FirstChild;
            while (child != null)
            {
                var (startOffset, endOffset) = child.GetDocumentRange();
                if (endOffset.Offset < startLineOffset)
                {
                    child = child.NextSibling;
                    continue;
                }
                if (startOffset.Offset > endLineOffset)
                    break;
                if (child is TNode matchingNode)
                    yield return matchingNode;
                else
                    foreach (var treeNode in FindChildrenInRange<TNode>(child, startLineOffset, endLineOffset))
                        yield return treeNode;
                child = child.NextSibling;
            }
        }

        public List<string> GetSupportedFileExtensions()
        {
            return new List<string>
            {
                GherkinProjectFileType.FEATURE_EXTENSION
            };
        }
    }
}