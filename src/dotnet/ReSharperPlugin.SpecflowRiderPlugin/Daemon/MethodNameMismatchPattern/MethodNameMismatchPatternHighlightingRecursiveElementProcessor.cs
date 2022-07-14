using System;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Naming.Extentions;
using JetBrains.ReSharper.Psi.Naming.Impl;
using JetBrains.ReSharper.Psi.Naming.Settings;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.TestRunner.Abstractions.Extensions;
using JetBrains.Util;
using ReSharperPlugin.SpecflowRiderPlugin.Daemon.Errors;
using ReSharperPlugin.SpecflowRiderPlugin.Helpers;
using ReSharperPlugin.SpecflowRiderPlugin.Utils.Steps;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.MethodNameMismatchPattern
{
    internal class MethodNameMismatchPatternHighlightingRecursiveElementProcessor : IRecursiveElementProcessor<FilteringHighlightingConsumer>
    {
        private readonly IDaemonProcess _daemonProcess;
        private readonly IStepDefinitionBuilder _stepDefinitionBuilder;

        public MethodNameMismatchPatternHighlightingRecursiveElementProcessor(IDaemonProcess daemonProcess, IStepDefinitionBuilder stepDefinitionBuilder)
        {
            _daemonProcess = daemonProcess;
            _stepDefinitionBuilder = stepDefinitionBuilder;
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

                if (constantValue.Kind is not ConstantValueKind.String)
                    continue;

                if (method.DeclaredElement == null)
                    continue;

                var expectedMethodName = _stepDefinitionBuilder.GetStepDefinitionMethodNameFromPattern(stepKind.Value, constantValue.StringValue, method.DeclaredElement.Parameters.SelectNotNull(x => x.ShortName).ToArray());
                expectedMethodName = psiServices.Naming.Suggestion.GetDerivedName(expectedMethodName, NamedElementKinds.Method, ScopeKind.Common, CSharpLanguage.Instance.NotNull(), new SuggestionOptions(), _daemonProcess.SourceFile);

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
                context.AddHighlighting(new MethodNameMismatchPatternInfo(method, expectedName));
            }
        }

        public void ProcessAfterInterior(ITreeNode element, FilteringHighlightingConsumer context)
        {
        }
    }
}