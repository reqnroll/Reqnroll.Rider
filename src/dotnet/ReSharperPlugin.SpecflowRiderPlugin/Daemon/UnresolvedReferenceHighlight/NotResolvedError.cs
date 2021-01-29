using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.UnresolvedReferenceHighlight
{
    [StaticSeverityHighlighting(
        Severity.ERROR,
        typeof(HighlightingGroupIds.CodeInsights),
        OverlapResolve = OverlapResolveKind.ERROR,
        OverloadResolvePriority = 0,
        ToolTipFormatString = Message)]
    public class NotResolvedError : IHighlighting
    {
        public GherkinStep GherkinStep { get; }
        public const string Message = "Unresolved step";
        public string ToolTip => Message;
        public string ErrorStripeToolTip => ToolTip;

        public NotResolvedError(GherkinStep gherkinStep)
        {
            GherkinStep = gherkinStep;
        }

        public bool IsValid()
        {
            return GherkinStep.IsValid();
        }

        public DocumentRange CalculateRange()
        {
            return GherkinStep.GetDocumentRange();
        }
    }
}