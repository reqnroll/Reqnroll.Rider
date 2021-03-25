using System.Collections.Generic;
using System.Linq;
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

namespace ReSharperPlugin.SpecflowRiderPlugin.CompletionProviders
{
    public sealed class CompletionStepLookupItem : TextLookupItemBase
    {
        public override IconId Image { get; }

        public CompletionStepLookupItem(string text, string typeText, bool isDynamic = false)
            : base(isDynamic)
        {
            DisplayTypeName = LookupUtil.FormatTypeString(typeText);
            Text = text;
        }

        public CompletionStepLookupItem(string text, IconId image, bool isDynamic = false)
            : this(text, string.Empty, isDynamic)
            => Image = image;

        public override void Accept(ITextControl textControl, DocumentRange nameRange, LookupItemInsertType insertType, Suffix suffix, ISolution solution, bool keepCaretStill)
        {
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

        public IEnumerable<HotspotInfo> BuildHotspotInfos()
        {
            if (Ranges == null)
                yield break;

            var openPosition = -1;
            var previous = '\0';
            var hotstpotIndex = 0;
            for (int i = 0; i < Text.Length; i++)
            {
                var c = Text[i];
                if (c == '(' && previous != '\\')
                    openPosition = i;
                if (c == ')' && previous != '\\' && openPosition >= 0)
                {
                    var startOffset = Ranges.InsertRange.StartOffset;
                    var range = new DocumentRange(startOffset.Shift(openPosition), startOffset.Shift(i + 1));
                    yield return new HotspotInfo(new TemplateField(hotstpotIndex.ToString(), new TextHotspotExpression(new List<string> {Text.Substring(openPosition + 1, i - openPosition - 1)}), 0), range);
                    hotstpotIndex++;
                    openPosition = -1;
                }
                previous = c;
            }
        }
    }
}