using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Plugins.Unity.CSharp.Daemon.Errors;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharperPlugin.SpecflowRiderPlugin.QuickFixes
{
    [QuickFix]
    public class MethodNameMismatchPatternQuickFix : QuickFixBase
    {
        private readonly MethodNameMismatchPatternWarning _warning;

        public override string Text => "Rename to " + _warning.ExpectedName;

        public MethodNameMismatchPatternQuickFix(MethodNameMismatchPatternWarning warning)
        {
            _warning = warning;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            CSharpImplUtil.ReplaceIdentifier(_warning.Method.NameIdentifier, _warning.ExpectedName);
            return _ => { };
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return true;
        }
    }
}