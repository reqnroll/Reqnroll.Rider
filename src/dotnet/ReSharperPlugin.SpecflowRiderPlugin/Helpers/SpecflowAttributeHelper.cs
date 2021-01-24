using System;
using JetBrains.Metadata.Reader.Impl;
using ReSharperPlugin.SpecflowRiderPlugin.Psi;

namespace ReSharperPlugin.SpecflowRiderPlugin.Helpers
{
    public class SpecflowAttributeHelper
    {
        public static readonly ClrTypeName GivenAttribute = new ClrTypeName("TechTalk.SpecFlow.GivenAttribute");
        public static readonly ClrTypeName WhenAttribute = new ClrTypeName("TechTalk.SpecFlow.WhenAttribute");
        public static readonly ClrTypeName ThenAttribute = new ClrTypeName("TechTalk.SpecFlow.ThenAttribute");
        public static string GetAttributeClrName(GherkinStepKind stepKind)
        {
            switch (stepKind)
            {
                case GherkinStepKind.Given:
                    return GivenAttribute.FullName;
                case GherkinStepKind.When:
                    return WhenAttribute.FullName;
                case GherkinStepKind.Then:
                    return ThenAttribute.FullName;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stepKind), stepKind, null);
            }
        }
    }
}