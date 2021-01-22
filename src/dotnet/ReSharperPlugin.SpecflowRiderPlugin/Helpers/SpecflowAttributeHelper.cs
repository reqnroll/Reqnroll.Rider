using JetBrains.Metadata.Reader.Impl;

namespace ReSharperPlugin.SpecflowRiderPlugin.Helpers
{
    public class SpecflowAttributeHelper
    {
        public static readonly ClrTypeName GivenAttribute = new ClrTypeName("TechTalk.SpecFlow.GivenAttribute");
        public static readonly ClrTypeName WhenAttribute = new ClrTypeName("TechTalk.SpecFlow.WhenAttribute");
        public static readonly ClrTypeName ThenAttribute = new ClrTypeName("TechTalk.SpecFlow.ThenAttribute");
    }
}