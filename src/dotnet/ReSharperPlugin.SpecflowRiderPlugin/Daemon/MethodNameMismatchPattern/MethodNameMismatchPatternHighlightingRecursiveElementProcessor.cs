using System;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Plugins.Unity.CSharp.Daemon.Errors;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Helpers;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.MethodNameMismatchPattern
{
    internal class MethodNameMismatchPatternHighlightingRecursiveElementProcessor : IRecursiveElementProcessor<FilteringHighlightingConsumer>
    {
        private readonly IDaemonProcess _daemonProcess;

        public MethodNameMismatchPatternHighlightingRecursiveElementProcessor(IDaemonProcess daemonProcess)
        {
            _daemonProcess = daemonProcess;
        }

        public bool InteriorShouldBeProcessed(ITreeNode element, FilteringHighlightingConsumer context)
        {
            if (element is ITypeDeclaration)
                return true;
            if (element is INamespaceDeclaration)
                return true;
            if (element is INamespaceBody)
                return true;
            if (element is IClassBody)
                return true;

            return false;
        }

        public bool IsProcessingFinished(FilteringHighlightingConsumer context)
        {
            return _daemonProcess.InterruptFlag;
        }

        public void ProcessBeforeInterior(ITreeNode element, FilteringHighlightingConsumer context)
        {
            if (!(element is IMethodDeclaration method))
                return;

            var psiServices = method.GetPsiServices();
            var invalidNames = new LocalList<string>();
            foreach (var attribute in method.Attributes)
            {
                var stepKind = SpecflowAttributeHelper.GetAttributeStepKind(attribute.GetAttributeType().GetClrName());
                if (stepKind == null)
                    continue;

                var attributeInstance = attribute.GetAttributeInstance();
                if (attributeInstance.PositionParameterCount < 1)
                    continue;

                var constantValue = attributeInstance.PositionParameter(0).ConstantValue;
                if (!constantValue.IsValid())
                    continue;

                if (!(constantValue.Value is string stepText))
                    continue;

                var expectedMethodName = SpecflowStepHelper.GetMethodNameAndParameterFromStepPattern(stepKind.Value, stepText, psiServices, _daemonProcess.SourceFile, method.Params.ParameterDeclarations.Select(x => x.DeclaredName).ToList());

                if (string.Equals(method.DeclaredName, expectedMethodName, StringComparison.InvariantCultureIgnoreCase))
                    return;

                if (method.DeclaredName.ToLowerInvariant().StartsWith(stepKind.ToString().ToLowerInvariant()))
                    invalidNames.Insert(0, expectedMethodName);
                else
                    invalidNames.Add(expectedMethodName);
            }

            if (invalidNames.Count > 0)
            {
                var expectedName = invalidNames.FirstOrDefault();
                context.AddHighlighting(new MethodNameMismatchPatternWarning(method, expectedName));
            }
        }

        public void ProcessAfterInterior(ITreeNode element, FilteringHighlightingConsumer context)
        {
        }
    }
}