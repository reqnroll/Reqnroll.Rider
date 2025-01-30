using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Diagnostics;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems.Impl;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Hotspots;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.LiveTemplates;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Templates;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.TextControl;
using JetBrains.UI.Icons;

namespace ReSharperPlugin.ReqnrollRiderPlugin.CompletionProviders
{
    public sealed class CompletionStepLookupItem : TextLookupItemBase
    {
        private readonly string _originalText;
        public override IconId Image { get; }
        private List<(int position, string value, string type)> _replacements;

        public CompletionStepLookupItem(string text, string typeText, bool isDynamic = false)
            : base(isDynamic)
        {
            DisplayTypeName = LookupUtil.FormatTypeString(typeText);
            _originalText = text;
            Text = text;
        }

        public CompletionStepLookupItem(string text, IconId image, bool isDynamic = false)
            : this(text, string.Empty, isDynamic)
            => Image = image;

        public override void Accept(ITextControl textControl, DocumentRange nameRange, LookupItemInsertType insertType, Suffix suffix, ISolution solution, bool keepCaretStill)
        {
            // Process cucumber expressions only when accepting the item
            Text = ProcessCucumberExpressions(_originalText);
            base.Accept(textControl, nameRange, insertType, suffix, solution, keepCaretStill);

            var templatesManager = LiveTemplatesManager.Instance;
            Ranges.NotNull();
            var endCaretPosition = new DocumentOffset(nameRange.Document, textControl.Caret.Offset());
            var hotspotInfos = BuildHotspotInfos().ToArray();
            if (hotspotInfos.Length > 0)
            {
                templatesManager
                    .CreateHotspotSessionAtopExistingText(solution, endCaretPosition, textControl, LiveTemplatesManager.EscapeAction.LeaveTextAndCaret, hotspotInfos)
                    .ExecuteAndForget();
            }
        }

        private string ProcessCucumberExpressions(string text)
        {
            var result = new StringBuilder();
            var lastPos = 0;
            var openPosition = -1;
            _replacements = new List<(int position, string value, string type)>();

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '{')
                {
                    openPosition = i;
                }
                else if (c == '}' && openPosition >= 0)
                {
                    var paramType = text.Substring(openPosition + 1, i - openPosition - 1);
                    result.Append(text.Substring(lastPos, openPosition - lastPos));
                    var defaultValue = GetDefaultValueForType(paramType);
                    result.Append(defaultValue);
                    _replacements.Add((result.Length - defaultValue.Length, defaultValue, paramType));
                    lastPos = i + 1;
                    openPosition = -1;
                }
            }

            result.Append(text.Substring(lastPos));
            Text = result.ToString();
            return result.ToString();
        }

        public IEnumerable<HotspotInfo> BuildHotspotInfos()
        {
            if (Ranges == null || _replacements == null)
                yield break;

            var startOffset = Ranges.InsertRange.StartOffset;
            var hotstpotIndex = 0;

            // First handle cucumber expression replacements
            foreach (var (position, value, type) in _replacements)
            {
                if (type == "string")
                {
                    // For strings, highlight only the content between quotes
                    var range = new DocumentRange(
                        startOffset.Shift(position + 1), // +1 to skip the opening quote
                        startOffset.Shift(position + value.Length - 1) // -1 to skip the closing quote
                    );
                    yield return new HotspotInfo(new TemplateField(hotstpotIndex.ToString(),
                        new TextHotspotExpression(new List<string> { "text" }), 0), range);
                }
                else
                {
                    // For all other types, highlight the entire value
                    var range = new DocumentRange(
                        startOffset.Shift(position),
                        startOffset.Shift(position + value.Length)
                    );
                    yield return new HotspotInfo(new TemplateField(hotstpotIndex.ToString(),
                        new TextHotspotExpression(new List<string> { value }), 0), range);
                }
                hotstpotIndex++;
            }

            // Then handle regex patterns
            var openPosition = -1;
            var previous = '\0';

            for (var i = 0; i < Text.Length; i++)
            {
                var c = Text[i];
                if (c == '(' && previous != '\\')
                {
                    openPosition = i;
                }
                else if (c == ')' && previous != '\\' && openPosition >= 0)
                {
                    var range = new DocumentRange(startOffset.Shift(openPosition), startOffset.Shift(i + 1));
                    yield return new HotspotInfo(new TemplateField(hotstpotIndex.ToString(),
                        new TextHotspotExpression(new List<string> { Text.Substring(openPosition + 1, i - openPosition - 1) }), 0), range);
                    hotstpotIndex++;
                    openPosition = -1;
                }
                previous = c;
            }
        }

        private string GetDefaultValueForType(string paramType)
        {
            return paramType switch
            {
                "string" => "\"text\"",
                "int" => "0",
                "decimal" => "0.0",
                "double" => "0.0",
                "float" => "0.0",
                "datetime" => "2025-01-01",
                "word" => "word",
                _ => paramType
            };
        }
    }
}