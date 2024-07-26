using JetBrains.Annotations;
using JetBrains.ReSharper.Daemon.Syntax;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.ReqnrollRiderPlugin.SyntaxHighlighting;

public class GherkinSyntaxHighlightingStageProcess(
    [NotNull] IDaemonProcess process,
    [NotNull] IFile file,
    SyntaxHighlightingProcessor processor
) : SyntaxHighlightingStageProcess(process, file, processor);