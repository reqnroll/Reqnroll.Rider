using System;
using System.Drawing;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.UI.RichText;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;
using ReSharperPlugin.SpecflowRiderPlugin.Utils.TestOutput;

namespace ReSharperPlugin.SpecflowRiderPlugin.Daemon.ExecutionFailedStep
{
    [StaticSeverityHighlighting(
        Severity.INFO,
        typeof(ExecutionFailedStepGutterMarks),
        AttributeId = "SpecFlow Failed Step",
        OverlapResolve = OverlapResolveKind.NONE,
        ShowToolTipInStatusBar = false
    )]
    public class ExecutionFailedStepHighlighting : IRichTextToolTipHighlighting
    {
        public string ToolTip => BuildTooltip();

        private string BuildTooltip()
        {
            if (!string.IsNullOrWhiteSpace(_stepTestOutput.ErrorOutput))
                return _stepTestOutput.StatusLine + Environment.NewLine + Environment.NewLine + _stepTestOutput.ErrorOutput;

            return _stepTestOutput.StatusLine;
        }

        public string ErrorStripeToolTip => ToolTip;

        private readonly GherkinStep _gherkinStep;
        private readonly StepTestOutput _stepTestOutput;

        public ExecutionFailedStepHighlighting(GherkinStep gherkinStep, StepTestOutput stepTestOutput)
        {
            _gherkinStep = gherkinStep;
            _stepTestOutput = stepTestOutput;
        }

        public bool IsValid()
        {
            return true;
        }

        public RichTextBlock TryGetTooltip(HighlighterTooltipKind where)
        {
            var richTextBlock = new RichTextBlock();

            switch (_stepTestOutput.Status)
            {
                case StepTestOutput.StepStatus.Failed:
                    var statusLineText = new RichText(_stepTestOutput.Status.ToString(), new TextStyle(FontStyle.Bold, Color.DarkRed))
                        .Append(new RichText(" - "))
                        .Append(new RichText(_stepTestOutput.StatusLine));
                    richTextBlock.Add(statusLineText);
                    break;
                default:
                    richTextBlock.Add(new RichText(_stepTestOutput.StatusLine));
                    break;
            }

            if (!string.IsNullOrWhiteSpace(_stepTestOutput.ErrorOutput))
            {
                richTextBlock.Add(new RichText("---------------------"));
                richTextBlock.Add(new RichText(_stepTestOutput.ErrorOutput));
            }

            return richTextBlock;
        }

        public DocumentRange CalculateRange()
        {
            return _gherkinStep.GetDocumentRange();
        }
    }
}