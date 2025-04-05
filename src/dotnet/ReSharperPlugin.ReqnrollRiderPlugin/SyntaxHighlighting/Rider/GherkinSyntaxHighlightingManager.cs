using JetBrains.Application.Settings;
using JetBrains.Diagnostics;
using JetBrains.Rd.Impl;
using JetBrains.ReSharper.Daemon.Syntax;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.SyntaxHighlighting.Rider;

[Language(typeof(GherkinLanguage))]
public class GherkinSyntaxHighlightingManager : SyntaxHighlightingManager
{
    public GherkinSyntaxHighlightingManager()
    {
        Protocol.Logger.Log(LoggingLevel.INFO, $"GherkinSyntaxHighlightingManager");
    }

    public override SyntaxHighlightingStageProcess CreateProcess(IDaemonProcess process, IContextBoundSettingsStore settings, IFile getPrimaryPsiFile)
    {
        Protocol.Logger.Log(LoggingLevel.INFO, $"GherkinSyntaxHighlightingManager:CreateProcess - {getPrimaryPsiFile.GetSourceFile().NotNull().Name}");

        return new GherkinSyntaxHighlightingStageProcess(process, getPrimaryPsiFile, new GherkinSyntaxHighlightingProcessor());
    }
}