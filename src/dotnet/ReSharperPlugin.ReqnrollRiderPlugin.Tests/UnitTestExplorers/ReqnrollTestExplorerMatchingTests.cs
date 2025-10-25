using System;
using System.Reflection;
using System.Text;
using FluentAssertions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Elements;
using JetBrains.ReSharper.UnitTestProvider.nUnit.v30;
using Moq;
using NUnit.Framework;
using ReSharperPlugin.ReqnrollRiderPlugin.UnitTestExplorers;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Tests.UnitTestExplorers
{
    [TestFixture]
    public class ReqnrollTestExplorerMatchingTests
    {
        private static string Identifier(string text)
        {
            if (text == null) return string.Empty;
            var sb = new StringBuilder(text.Length + 4);
            bool upperNext = true;
            foreach (var ch in text)
            {
                // Remove single/double quotes
                if (ch == '\'' || ch == '"')
                    continue;

                // Map '.', '-' and newlines to underscore like Reqnroll's ToIdentifierPart
                if (ch == '.' || ch == '-' || ch == '\n')
                {
                    if (sb.Length == 0 || sb[sb.Length - 1] != '_')
                        sb.Append('_');
                    upperNext = true;
                    continue;
                }

                if (char.IsLetterOrDigit(ch) || ch == '_')
                {
                    if (upperNext && char.IsLetter(ch))
                        sb.Append(char.ToUpperInvariant(ch));
                    else
                        sb.Append(ch);
                    upperNext = false;
                }
                else
                {
                    // Other punctuation is removed but triggers capitalization of the next letter
                    upperNext = true;
                }
            }

            // Ensure first char is uppercase if letter
            if (sb.Length > 0 && char.IsLetter(sb[0]))
                sb[0] = char.ToUpperInvariant(sb[0]);

            // If first char is digit, prefix underscore like ToIdentifier
            if (sb.Length > 0 && char.IsDigit(sb[0]))
                sb.Insert(0, '_');
            return sb.ToString();
        }

        private static object CreateExplorerInstance()
        {
            // Create instance of the internal ReqnrollTestExplorer via reflection with non-public ctor
            var asm = typeof(ReqnrollUnitTestProvider).Assembly;
            var type = asm.GetType("ReSharperPlugin.ReqnrollRiderPlugin.UnitTestExplorers.ReqnrollTestExplorer", throwOnError: true);
            var instance = Activator.CreateInstance(
                type,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                binder: null,
                args: new object[] { null, null, null },
                culture: null);
            return instance;
        }

        private static bool InvokeCompare(object explorer, string scenarioText, IUnitTestElement relatedTest)
        {
            var method = explorer.GetType().GetMethod("CompareDescriptionWithShortName", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Should().NotBeNull("private method must exist and be discoverable");
            var result = method!.Invoke(explorer, new object[] { scenarioText, relatedTest });
            return (bool)result!;
        }

        private static IUnitTestElement MakeRelatedTest(string providerId, string shortName, string? declaredShortName)
        {
            var providerMock = new Mock<IUnitTestProvider>(MockBehavior.Strict);
            providerMock.SetupGet(p => p.ID).Returns(providerId);

            var declaredElementMock = new Mock<IDeclaredElement>(MockBehavior.Strict);
            declaredElementMock.SetupGet(d => d.ShortName).Returns(declaredShortName);

            var elementMock = new Mock<IUnitTestElement>(MockBehavior.Strict);
            elementMock.SetupGet(e => e.Provider).Returns(providerMock.Object);
            elementMock.SetupGet(e => e.ShortName).Returns(shortName);
            elementMock.Setup(e => e.GetDeclaredElement()).Returns(declaredElementMock.Object);

            return elementMock.Object;
        }

        [Test]
        public void DeclaredElementShortName_matches_scenario_identifier_any_framework()
        {
            var explorer = CreateExplorerInstance();
            var scenarioText = "Add two numbers"; // identifier: AddTwoNumbers
            var related = MakeRelatedTest(providerId: "MSTest", shortName: "Anything", declaredShortName: Identifier(scenarioText));

            var matched = InvokeCompare(explorer, scenarioText, related);
            matched.Should().BeTrue();
        }

        [Test]
        public void NUnit_identifier_compares_against_ShortName_when_declared_not_available()
        {
            var explorer = CreateExplorerInstance();
            var scenarioText = "Add two numbers"; // identifier: AddTwoNumbers
            var related = MakeRelatedTest(providerId: NUnitTestProvider.PROVIDER_ID, shortName: Identifier(scenarioText), declaredShortName: null);

            var matched = InvokeCompare(explorer, scenarioText, related);
            matched.Should().BeTrue();
        }

        [Test]
        public void MSTest_identifier_compares_against_ShortName_when_declared_not_available()
        {
            var explorer = CreateExplorerInstance();
            var scenarioText = "Add two numbers"; // identifier: AddTwoNumbers
            var related = MakeRelatedTest(providerId: "MSTest", shortName: Identifier(scenarioText), declaredShortName: null);

            var matched = InvokeCompare(explorer, scenarioText, related);
            matched.Should().BeTrue();
        }

        [Test]
        public void XUnit_exact_display_name_match_succeeds()
        {
            var explorer = CreateExplorerInstance();
            var scenarioText = "Title with spaces and : punctuation";
            var related = MakeRelatedTest(providerId: "xUnit", shortName: scenarioText, declaredShortName: null);

            var matched = InvokeCompare(explorer, scenarioText, related);
            matched.Should().BeTrue();
        }

        [Test]
        public void XUnit_identifier_fallback_works_when_display_name_differs()
        {
            var explorer = CreateExplorerInstance();
            var scenarioText = "Title with special chars: star *, colon:, etc.";
            var identifier = Identifier(scenarioText);
            var related = MakeRelatedTest(providerId: "xUnit", shortName: identifier, declaredShortName: null);

            var matched = InvokeCompare(explorer, scenarioText, related);
            matched.Should().BeTrue();
        }

        [Test]
        public void Mismatch_returns_false()
        {
            var explorer = CreateExplorerInstance();
            var scenarioText = "One name";
            var related = MakeRelatedTest(providerId: NUnitTestProvider.PROVIDER_ID, shortName: "DifferentName", declaredShortName: null);

            var matched = InvokeCompare(explorer, scenarioText, related);
            matched.Should().BeFalse();
        }
    }
}