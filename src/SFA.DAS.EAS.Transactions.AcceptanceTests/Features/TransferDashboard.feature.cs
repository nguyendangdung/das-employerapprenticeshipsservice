﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.2.0.0
//      SpecFlow Generator Version:2.2.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.EAS.Transactions.AcceptanceTests.Features
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.2.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("TransferDashboard")]
    public partial class TransferDashboardFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "TransferDashboard.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "TransferDashboard", "\tIn order to review transfer details of my account\r\n\tI want view a summary of my " +
                    "account\'s transfers", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
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
        [NUnit.Framework.DescriptionAttribute("Transfer Allowance")]
        public virtual void TransferAllowance()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Transfer Allowance", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
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
                        "223/ABC",
                        "1000",
                        "16-17",
                        "10",
                        "1",
                        "2017-02-18",
                        "2017-02-23"});
            table1.AddRow(new string[] {
                        "223/ABC",
                        "2000",
                        "16-17",
                        "11",
                        "1",
                        "2017-03-18",
                        "2017-03-23"});
            table1.AddRow(new string[] {
                        "223/ABC",
                        "3000",
                        "16-17",
                        "12",
                        "1",
                        "2017-04-18",
                        "2017-04-23"});
            table1.AddRow(new string[] {
                        "223/ABC",
                        "1000",
                        "17-18",
                        "1",
                        "1",
                        "2017-05-18",
                        "2017-05-23"});
            table1.AddRow(new string[] {
                        "223/ABC",
                        "2000",
                        "17-18",
                        "2",
                        "1",
                        "2017-06-18",
                        "2017-06-23"});
            table1.AddRow(new string[] {
                        "223/ABC",
                        "3000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-18",
                        "2017-07-23"});
#line 8
 testRunner.When("I have the following submissions", ((string)(null)), table1, "When ");
#line 16
 testRunner.And("The transfer allowance ratio is 10 percent", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 17
 testRunner.Then("the transfer allowance should be 330 on the transfer dashboard screen", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
