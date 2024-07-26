using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems.Impl;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.TextControl;
using JetBrains.UI.Icons;

namespace ReSharperPlugin.ReqnrollRiderPlugin.CompletionProviders
{
    public class CompletionKeywordLookupItem : TextLookupItem
    {
        public CompletionKeywordLookupItem(string text, IconId image, bool isDynamic = false) : base(text, image, isDynamic)
        {
        }

        public override void Accept(ITextControl textControl, DocumentRange nameRange, LookupItemInsertType insertType, Suffix suffix, ISolution solution, bool keepCaretStill)
        {
            base.Accept(textControl, nameRange, insertType, suffix, solution, keepCaretStill);

            textControl.Document.InsertText(textControl.Caret.DocumentOffset(), " ");
        }
    }
}