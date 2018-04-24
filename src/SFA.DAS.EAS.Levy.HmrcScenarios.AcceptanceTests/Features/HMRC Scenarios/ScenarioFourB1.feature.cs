﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.1.0.0
//      SpecFlow Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.EAS.Levy.HmrcScenarios.AcceptanceTests2.Features.HMRCScenarios
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("ScenarioFourB - In-year adjustments - no change to current balance")]
    public partial class ScenarioFourB_In_YearAdjustments_NoChangeToCurrentBalanceFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ScenarioFourB.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ScenarioFourB - In-year adjustments - no change to current balance", null, ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Late submissions")]
        public virtual void LateSubmissions()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Late submissions", ((string[])(null)));
#line 3
this.ScenarioSetup(scenarioInfo);
#line 4
 testRunner.Given("I have an account", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Paye_scheme",
                        "LevyDueYtd",
                        "Payroll_Year",
                        "Payroll_Month",
                        "English_Fraction",
                        "SubmissionDate",
                        "CreatedDate"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "10000",
                        "17-18",
                        "1",
                        "1",
                        "2017-05-15",
                        "2017-05-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "20000",
                        "17-18",
                        "2",
                        "1",
                        "2017-06-15",
                        "2017-06-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "30000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-15",
                        "2017-07-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "40000",
                        "17-18",
                        "4",
                        "1",
                        "2017-08-15",
                        "2017-08-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "50000",
                        "17-18",
                        "5",
                        "1",
                        "2017-09-15",
                        "2017-09-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "21250",
                        "17-18",
                        "2",
                        "1",
                        "2017-09-15",
                        "2017-09-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "60000",
                        "17-18",
                        "6",
                        "1",
                        "2017-10-15",
                        "2017-10-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "70000",
                        "17-18",
                        "7",
                        "1",
                        "2017-11-15",
                        "2017-11-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "80000",
                        "17-18",
                        "8",
                        "1",
                        "2017-12-15",
                        "2017-12-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "90000",
                        "17-18",
                        "9",
                        "1",
                        "2018-01-15",
                        "2018-01-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "100000",
                        "17-18",
                        "10",
                        "1",
                        "2018-02-15",
                        "2018-02-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "110000",
                        "17-18",
                        "11",
                        "1",
                        "2018-03-15",
                        "2018-03-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "120000",
                        "17-18",
                        "12",
                        "1",
                        "2018-04-15",
                        "2018-04-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "98000",
                        "17-18",
                        "10",
                        "1",
                        "2018-04-15",
                        "2018-04-23"});
#line 5
 testRunner.When("I have the following submissions", ((string)(null)), table1, "When ");
#line 21
 testRunner.Then("the balance on 04/2018 should be 132000 on the screen", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 22
 testRunner.And("the total levy shown for month 06/2017 should be 11000", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 23
 testRunner.And("the total levy shown for month 02/2018 should be 11000", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 24
 testRunner.And("For month 06/2017 the levy declared should be 10000 and the topup should be 1000", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 25
 testRunner.And("For month 02/2018 the levy declared should be 10000 and the topup should be 1000", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion