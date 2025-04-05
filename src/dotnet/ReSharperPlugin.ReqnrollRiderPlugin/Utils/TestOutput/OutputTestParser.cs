using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using ReSharperPlugin.ReqnrollRiderPlugin.Psi;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Utils.TestOutput;

public class OutputTestParser(IEnumerable<string> lines, GherkinKeywordList keywordList) : IDisposable
{
    private readonly List<string> _stepKeywords = keywordList.GetStepKeywords().ToList();
    private readonly IEnumerator<string> _enumerator = lines.GetEnumerator();
    private string _currentLine;
    private bool _eof;

    public IEnumerable<StepTestOutput> ParseOutput()
    {
        Advance();
        while (!_eof)
        {
            if (LineIsAStep(_currentLine))
                yield return ParseStep();
            else
                Advance();
        }
    }

    private bool LineIsAStep([CanBeNull] string line)
    {
        return _stepKeywords.Any(keyword => line?.StartsWith(keyword) == true);
    }

    private StepTestOutput ParseStep()
    {
        var firstStepLine = _currentLine;
        string tableContent = null;
        string multilineArgument = null;
        var output = new StringBuilder();
        Advance();

        while (!_currentLine.StartsWith("->") && !_eof)
        {
            if (_currentLine.StartsWith("  --- table step argument ---"))
                tableContent = ParseMultilineContent();
            else if (_currentLine.StartsWith("  --- multiline step argument ---"))
                multilineArgument = ParseMultilineContent();
            else
            {
                output.AppendLine(_currentLine);
                Advance();
            }
        }

        if (_currentLine == null)
            return null;

        var statusLine = _currentLine.Substring(3);
        Advance();

        while (!_eof && !LineIsAStep(_currentLine) && !_currentLine.StartsWith("-> "))
        {
            statusLine += Environment.NewLine + _currentLine;
            Advance();
        }

        return new StepTestOutput
        {
            Status = GetStatus(statusLine),
            StatusLine = statusLine,
            FirstLine = firstStepLine,
            Table = tableContent,
            MultiLineArgument = multilineArgument,
            ErrorOutput = output.ToString()
        };
    }

    private static StepTestOutput.StepStatus GetStatus(string statusLine)
    {
        if (statusLine == "skipped because of previous errors")
            return StepTestOutput.StepStatus.Skipped;
        if (statusLine.StartsWith("No matching step definition found for the step"))
            return StepTestOutput.StepStatus.NotImplemented;

        var colonIndex = statusLine.IndexOf(':');
        if (colonIndex == -1)
            return StepTestOutput.StepStatus.Done;

        var status = statusLine.Substring(0, colonIndex);
        return status switch
        {
            "done" => StepTestOutput.StepStatus.Done,
            "duration" => StepTestOutput.StepStatus.Done,
            "error" => StepTestOutput.StepStatus.Failed,
            "binding error" => StepTestOutput.StepStatus.BindingError,
            _ => StepTestOutput.StepStatus.Done
        };
    }

    private string ParseMultilineContent()
    {
        Advance();
        var content = new StringBuilder();
        while (_currentLine.StartsWith("  ") && !_eof)
        {
            content.AppendLine(_currentLine.Substring(2));
            Advance();
        }
        return content.ToString();
    }

    private void Advance()
    {
        bool hasNext = _enumerator.MoveNext();
        if (hasNext)
            _currentLine = _enumerator.Current;
        else
        {
            _currentLine = null;
            _eof = true;
        }
    }

    public void Dispose()
    {
        _enumerator.Dispose();
    }
}