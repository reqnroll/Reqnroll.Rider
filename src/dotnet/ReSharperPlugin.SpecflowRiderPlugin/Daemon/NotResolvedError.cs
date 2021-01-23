using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon
{
    [StaticSeverityHighlighting(
        Severity.ERROR,
        typeof(HighlightingGroupIds.CodeInsights),
        OverlapResolve = OverlapResolveKind.ERROR,
        OverloadResolvePriority = 0,
        ToolTipFormatString = Message)]
    public class NotResolvedError : IHighlighting
    {
        private readonly GherkinStep _gherkinStep;
        public const string SeverityId = nameof(NotResolvedError);
        public const string Message = "Unresolved step";
        public string ToolTip => Message;
        public string ErrorStripeToolTip => "Hello";

        public NotResolvedError(GherkinStep gherkinStep)
        {
            _gherkinStep = gherkinStep;
        }

        public bool IsValid()
        {
            return _gherkinStep.IsValid();
        }

        public DocumentRange CalculateRange()
        {
            return _gherkinStep.GetDocumentRange();
        }
    }
}