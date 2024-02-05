using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon.Syntax;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.ReqnrollRiderPlugin.SyntaxHighlighting
{
    public class GherkinSyntaxHighlightingStageProcess : SyntaxHighlightingStageProcess
    {
        public GherkinSyntaxHighlightingStageProcess([NotNull] IDaemonProcess process,
                                                     [NotNull] IContextBoundSettingsStore settingsStore,
                                                     [NotNull] IFile file,
                                                     SyntaxHighlightingProcessor processor) : base(process, settingsStore, file, processor)
        {
        }
    }
}