// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by Reqnroll (https://www.reqnroll.net/).
//      Reqnroll Version:3.8.0.0
//      Reqnroll Generator Version:3.8.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace ClassicXUnit
{
    using Reqnroll;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "3.8.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class AdvancedExamplesMultipleTestcasesFeature : object, Xunit.IClassFixture<AdvancedExamplesMultipleTestcasesFeature.FixtureData>, System.IDisposable
    {
        
        private static Reqnroll.ITestRunner testRunner;
        
        private string[] _featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "Examples.feature"
#line hidden
        
        public AdvancedExamplesMultipleTestcasesFeature(AdvancedExamplesMultipleTestcasesFeature.FixtureData fixtureData, ClassicXUnit_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = Reqnroll.TestRunnerManager.GetTestRunner();
            Reqnroll.FeatureInfo featureInfo = new Reqnroll.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "Advanced Examples: multiple \"testcases\"", "Simple calculator for adding two numbers", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void TestInitialize()
        {
        }
        
        public virtual void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(Reqnroll.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableTheoryAttribute(DisplayName="TestCase: Add two numbers")]
        [Xunit.TraitAttribute("FeatureTitle", "Advanced Examples: multiple \"testcases\"")]
        [Xunit.TraitAttribute("Description", "TestCase: Add two numbers")]
        [Xunit.TraitAttribute("Category", "mytag")]
        [Xunit.InlineDataAttribute("50", "70", "120", new string[0])]
        [Xunit.InlineDataAttribute("30", "40", "70", new string[0])]
        [Xunit.InlineDataAttribute("60", "30", "90", new string[0])]
        public virtual void TestCaseAddTwoNumbers(string first, string second, string result, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "mytag"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            string[] tagsOfScenario = @__tags;
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            argumentsOfScenario.Add("First", first);
            argumentsOfScenario.Add("Second", second);
            argumentsOfScenario.Add("Result", result);
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("TestCase: Add two numbers", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 5
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 6
        testRunner.Given(string.Format("I have entered {0} in the calculator", first), ((string)(null)), ((Reqnroll.Table)(null)), "Given ");
#line hidden
#line 7
            testRunner.And(string.Format("I have entered {0} into the calculator", second), ((string)(null)), ((Reqnroll.Table)(null)), "And ");
#line hidden
#line 8
        testRunner.When("I press add", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
#line 9
        testRunner.Then(string.Format("the result should be {0} on the screen", result), ((string)(null)), ((Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "3.8.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                AdvancedExamplesMultipleTestcasesFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                AdvancedExamplesMultipleTestcasesFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
