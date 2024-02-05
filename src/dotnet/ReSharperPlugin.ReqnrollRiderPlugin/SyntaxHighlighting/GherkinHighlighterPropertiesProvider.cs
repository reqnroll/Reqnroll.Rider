//using JetBrains.Application;
//using JetBrains.ReSharper.Host.Features.Daemon;
//using JetBrains.ReSharper.Host.Features.Daemon.Registration;
//using JetBrains.Rider.Model.HighlighterRegistration;
//
//namespace ReSharperPlugin.ReqnrollRiderPlugin.SyntaxHighlighting
//{
//    [ShellComponent]
//    public class GherkinHighlighterPropertiesProvider : IRiderHighlighterPropertiesProvider
//    {
//        public bool Applicable(RiderHighlighterDescription description)
//        {
//            return description.AttributeId == GherkinHighlightingAttributeIds.TAG;
//        }
//
//        public HighlighterProperties GetProperties(RiderHighlighterDescription description)
//        {
//            return new HighlighterProperties(description.Kind.ToModel(), !description.NotRecyclable,
//                                             GreedySide.NONE, false, false, false);
//        }
//
//        public int Priority => 0;
//    }
//}