using System;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.UI.RichText;
using JetBrains.Util.Media;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;
using ReSharperPlugin.ReqnrollRiderPlugin.Utils.TestOutput;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Daemon.ExecutionFailedStep;

[StaticSeverityHighlighting(
    Severity.INFO,
    typeof(ExecutionFailedStepGutterMarks),
    AttributeId = "Reqnroll Failed Step",
    OverlapResolve = OverlapResolveKind.NONE,
    ShowToolTipInStatusBar = false
)]
public class ExecutionFailedStepHighlighting(GherkinStep gherkinStep, StepTestOutput stepTestOutput) : IRichTextToolTipHighlighting
{
    public string ToolTip => BuildTooltip();

    private string BuildTooltip()
    {
        if (!string.IsNullOrWhiteSpace(stepTestOutput.ErrorOutput))
            return stepTestOutput.StatusLine + Environment.NewLine + Environment.NewLine + stepTestOutput.ErrorOutput;

        return stepTestOutput.StatusLine;
    }

    public string ErrorStripeToolTip => ToolTip;

    public bool IsValid()
    {
        return true;
    }

    public RichTextBlock TryGetTooltip(HighlighterTooltipKind where)
    {
        var richTextBlock = new RichTextBlock();

        switch (stepTestOutput.Status)
        {
            case StepTestOutput.StepStatus.Failed:
                var statusLineText = new RichText(stepTestOutput.Status.ToString(), new TextStyle(JetFontStyles.Bold, JetRgbaColors.DarkRed))
                    .Append(new RichText(" - "))
                    .Append(new RichText(stepTestOutput.StatusLine.Replace("<", "&lt;")));
                richTextBlock.Add(statusLineText);
                break;
            default:
                richTextBlock.Add(new RichText(stepTestOutput.StatusLine.Replace("<", "&lt;")));
                break;
        }

        if (!string.IsNullOrWhiteSpace(stepTestOutput.ErrorOutput))
        {
            richTextBlock.Add(new RichText("---------------------"));
            richTextBlock.Add(new RichText(stepTestOutput.ErrorOutput.Replace("<", "&lt;")));
        }

        return richTextBlock;
    }

    public DocumentRange CalculateRange()
    {
        return gherkinStep.GetDocumentRange();
    }
}