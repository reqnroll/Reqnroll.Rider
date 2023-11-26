using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.TextControl.DocumentMarkup;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.ExecutionFailedStep
{
    [RegisterHighlighter("SpecFlow Failed Step", EffectType = EffectType.GUTTER_MARK, GutterMarkType = typeof(ExecutionFailedStepGutterMarkType))]
    [RegisterStaticHighlightingsGroup("SpecFlow Failed Step Test Gutter Marks", false)]
    public class ExecutionFailedStepGutterMarks;
}