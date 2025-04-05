using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharperPlugin.ReqnrollRiderPlugin.Daemon.Errors;

namespace ReSharperPlugin.ReqnrollRiderPlugin.QuickFixes;

[QuickFix]
public class MethodNameMismatchPatternQuickFix(MethodNameMismatchPatternInfo warning) : QuickFixBase
{

    public override string Text => "Rename to " + warning.ExpectedName;

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
        CSharpImplUtil.ReplaceIdentifier(warning.Method.NameIdentifier, warning.ExpectedName);
        return _ => { };
    }

    public override bool IsAvailable(IUserDataHolder cache)
    {
        return true;
    }
}