using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Utils.TestOutput
{
    public class OutputTestParser : IDisposable
    {
        private readonly GherkinKeywordList _keywordList;
        private readonly IEnumerator<string> _enumerator;
        private string _currentLine;
        private bool _eof;

        public OutputTestParser(IEnumerable<string> lines, GherkinKeywordList keywordList)
        {
            _keywordList = keywordList;
            _enumerator = lines.GetEnumerator();
        }

        public IEnumerable<StepTestOutput> ParseOutput()
        {
            Advance();
            var stepKeywords = _keywordList.GetStepKeywords().ToList();
            do
            {
                if (_eof)
                    break;
                foreach (var stepKeyword in stepKeywords)
                {
                    if (_currentLine.StartsWith(stepKeyword))
                    {
                        yield return ParseStep();
                        break;
                    }
                }
            }
            while (Advance());
        }

        private StepTestOutput ParseStep()
        {
            var firstStepLine = _currentLine;
            string tableContent = null;
            string multilineArgument = null;
            var output = new StringBuilder();
            Advance();

            while (!_currentLine.StartsWith("->")  && !_eof)
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

        private static string GetStatus(string status)
        {
            var colonIndex = status.IndexOf(':');
            if (colonIndex == -1)
                return status;
            return status.Substring(0, colonIndex);
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

        private bool Advance()
        {
            bool hasNext = _enumerator.MoveNext();
            if (hasNext)
                _currentLine = _enumerator.Current;
            else
            {
                _currentLine = null;
                _eof = true;
            }
            return hasNext;
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }
}