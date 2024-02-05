using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.TextControl.DocumentMarkup;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.ExecutionFailedStep
{
    [RegisterHighlighter("Reqnroll Failed Step", EffectType = EffectType.GUTTER_MARK, GutterMarkType = typeof(ExecutionFailedStepGutterMarkType))]
    [RegisterStaticHighlightingsGroup("Reqnroll Failed Step Test Gutter Marks", false)]
    public class ExecutionFailedStepGutterMarks;
}