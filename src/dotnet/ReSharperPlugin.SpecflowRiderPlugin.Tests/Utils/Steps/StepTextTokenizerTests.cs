using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using ReSharperPlugin.SpecflowRiderPlugin.Utils.Steps;

namespace ReSharperPlugin.SpecflowRiderPlugin.Tests.Utils.Steps
{
    public class StepTextTokenizerTests
    {
        private StepTextTokenizer _tokenizer;

        [SetUp]
        public void SetUp()
        {
            _tokenizer = new StepTextTokenizer();
        }

        private static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData("", new (StepTokenType, string)[0]);
            yield return new TestCaseData(" ", new (StepTokenType, string)[0]);
            yield return new TestCaseData("simple", new[] {(StepTokenType.Text, "simple")});
            yield return new TestCaseData(" simple ", new[] {(StepTokenType.Text, "simple")});
            yield return new TestCaseData("Some simple step", new[] {(StepTokenType.Text, "Some"), (StepTokenType.Text, "simple"), (StepTokenType.Text, "step")});
            yield return new TestCaseData("Some   simple", new[] {(StepTokenType.Text, "Some"), (StepTokenType.Text, "simple")});
            yield return new TestCaseData("\"Param\"", new[] {(StepTokenType.Parameter, "Param")});
            yield return new TestCaseData("\"\"", new[] {(StepTokenType.Parameter, "")});
            yield return new TestCaseData("\"Param", new[] {(StepTokenType.Parameter, "Param")});
            yield return new TestCaseData("<Param>", new[] {(StepTokenType.OutlineParameter, "Param")});
            yield return new TestCaseData("<>", new[] {(StepTokenType.OutlineParameter, "")});
            yield return new TestCaseData("<Param", new[] {(StepTokenType.OutlineParameter, "Param")});
            yield return new TestCaseData("Some <Param>", new[] {(StepTokenType.Text, "Some"), (StepTokenType.OutlineParameter, "Param")});
            yield return new TestCaseData("Some \"Param\"", new[] {(StepTokenType.Text, "Some"), (StepTokenType.Parameter, "Param")});
        }

        [Test]
        [TestCaseSource(nameof(TestCases))]
        public void TokenizeStepText_ShouldSplitWords(string stepText, (StepTokenType, string)[] expected)
        {
            var actual = _tokenizer.TokenizeStepText(stepText, true);
            actual.Should().BeEquivalentTo(expected);
        }
    }
}