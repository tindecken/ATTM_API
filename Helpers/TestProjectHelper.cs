using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using ATTM_API.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using CommonModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace ATTM_API.Helpers
{
    
    public class TestProjectHelper
    {

        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private static string sRootDLL = System.Reflection.Assembly.GetExecutingAssembly().Location;

        public static string sPatternStartSummary = "/// <summary>";
        public static string sPatternEndSummary = "/// </summary>";
        public static string sPatternParam = @"/// <param.*</param>$";
        public static string sPatternGroupKeyword = @"public void (?<KeywordName>.+)\((?<Params>.+)\).*";
        public static string sPatternGroupParam = "<param name=\"(?<ParamName>.+)\">(?<ParamDescription>.+)Exp:(?<ParamExampleValue>.+)</param>";
        private readonly IATTMAppSettings _appSettings;
        public TestProjectHelper(IATTMAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public async Task<JObject> GetKeywordsJson()
        {
            
            var result = new JObject();

            if (!Directory.Exists(_appSettings.TestProject)) {
                result.Add("result", "error");
                result.Add("data", null);
                result.Add("message", $"Not found Test Project folder: {_appSettings.TestProject}");
                return result;
            }

            Regex rgStartSummary = new Regex(sPatternStartSummary, RegexOptions.IgnoreCase);
            Regex rgEndSummary = new Regex(sPatternEndSummary);
            Regex rgParam = new Regex(sPatternParam);
            Regex rgGroupKeyword = new Regex(sPatternGroupKeyword);
            Regex rgGroupParam = new Regex(sPatternGroupParam);
            try
            {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                using (JsonTextWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.None;
                    writer.WriteStartObject();
                    writer.WritePropertyName("_id");
                    writer.WriteValue($"{ObjectId.GenerateNewId()}");
                    writer.WritePropertyName("refreshDate");
                    writer.WriteValue(DateTime.UtcNow);
                    writer.WritePropertyName("Categories");     //Start Categories
                    writer.WriteStartArray();

                    DirectoryInfo diKeywordFolder = new DirectoryInfo(Path.Combine(_appSettings.TestProject, "Keywords"));
                    foreach (DirectoryInfo diProduct in diKeywordFolder.GetDirectories())
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName("Name");
                        writer.WriteValue(diProduct.Name);
                        writer.WritePropertyName("Features");
                        writer.WriteStartArray();
                        foreach (FileInfo fiFeature in diProduct.GetFiles())
                        {
                            writer.WriteStartObject();
                            writer.WritePropertyName("Name");
                            writer.WriteValue(Path.GetFileNameWithoutExtension(fiFeature.Name));
                            writer.WritePropertyName("Keywords");
                            writer.WriteStartArray();
                            string[] lines = File.ReadAllLines(fiFeature.FullName);
                            int iLineIndex = 0;
                            foreach (string line in lines)
                            {
                                if (iLineIndex == lines.Length - 1) break;
                                Match mStartSummary = rgStartSummary.Match(lines[iLineIndex].Trim());
                                if (!mStartSummary.Success)
                                {
                                    iLineIndex++;
                                    continue;
                                };

                                //Get Block Information in < summary ></ summary >
                                ArrayList arrSummary = new ArrayList();
                                for (int i = iLineIndex + 1; i <= lines.Length; i++)
                                {
                                    iLineIndex++;
                                    Match mEndSummary = rgEndSummary.Match(lines[i].Trim());
                                    if (mEndSummary.Success) break;
                                    arrSummary.Add(lines[i].Replace(@"///", "").Trim());
                                }

                                //Get Information of all<param>
                                ArrayList arrParamsDescription = new ArrayList();
                                for (int j = iLineIndex + 1; j <= lines.Length; j++)
                                {
                                    iLineIndex++;
                                    Match mParam = rgParam.Match(lines[iLineIndex].Trim());
                                    if (mParam.Success)
                                    {
                                        arrParamsDescription.Add(lines[j].Replace(@"///", "").Trim());
                                    }
                                    else break;
                                }


                                //Method block
                                string sFullKW = lines[iLineIndex].Trim();

                                Match mKeyword = rgGroupKeyword.Match(sFullKW);
                                if (!mKeyword.Success)
                                {
                                    Logger.Error($"Line {iLineIndex + 1}, Keyword format should be [public void KeywordName(string param1, string param2, ..., string Optional)] - Current: {sFullKW}");
                                    Logger.Debug($"More information: Keyword should define in same line, not support on multiple lines");
                                }
                                string sKeywordName = mKeyword.Groups["KeywordName"].Value.Trim();
                                string sParamFull = mKeyword.Groups["Params"].Value.Trim();
                                string[] arrParam = sParamFull.Split(',');
                                ArrayList arrParamsKeyword = new ArrayList(arrParam);

                                //Verify number of param in keyword and description
                                if (arrParamsKeyword.Count != arrParamsDescription.Count)
                                {
                                    Logger.Error($"Keyword [{sKeywordName}] Feature [{Path.GetFileNameWithoutExtension(fiFeature.Name)}] has {arrParamsKeyword.Count} params, but in Comment has {arrParamsDescription.Count} params");
                                }

                                writer.WriteStartObject();
                                writer.WritePropertyName("Name");
                                writer.WriteValue(sKeywordName);

                                //Get Detail Summary Information and write into.xml file
                                //Prepare data for sKWDescriptions and sKWUpdateMessagesbefore before write to XML file(purpose: for support multiple line of Description and Update Message) and directly write to XML the other attribute(single line)
                                StringBuilder sKWUpdateMessages = new StringBuilder();
                                StringBuilder sKWDescriptions = new StringBuilder();
                                foreach (string item in arrSummary)
                                {
                                    try
                                    {
                                        var arrSummaryHeader = item.Split(':');
                                        if (arrSummaryHeader.Length > 1)
                                        {
                                            string k = item.Split(new[] { ':' }, 2)[0].Trim();
                                            string v = item.Split(new[] { ':' }, 2)[1].Trim();
                                            switch (k.ToUpper())
                                            {
                                                case "DESCRIPTION":
                                                case "DESC":
                                                    sKWDescriptions.AppendLine(v);
                                                    break;
                                                case "OWNER":
                                                case "AUTHOR":
                                                    writer.WritePropertyName("Owner");
                                                    writer.WriteValue(v);
                                                    break;
                                                case "CREATEDDATE":
                                                case "CREATEDATE":
                                                    writer.WritePropertyName("CreatedDate");
                                                    writer.WriteValue(v);
                                                    break;
                                                case "IMAGE":
                                                case "IMG":
                                                    writer.WritePropertyName("Image");
                                                    writer.WriteValue(v);
                                                    break;
                                                case "UPDATE":
                                                case "UPDATEMESSAGE":
                                                case "UPDATEDMESSAGE":
                                                    sKWUpdateMessages.AppendLine(v);
                                                    break;
                                                default:
                                                    Logger.Error($"Keyword [{sKeywordName}] Feature [{Path.GetFileNameWithoutExtension(fiFeature.Name)}] Category [{diProduct.Name}], has no description, desc, author, image in <summary><summary>");
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            sKWDescriptions.AppendLine(item);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error($"{ex}");
                                    }
                                }
                                //Write to Json object for sKWDescriptions and sKWUpdateMessages
                                writer.WritePropertyName("Description");
                                writer.WriteValue(sKWDescriptions.ToString());
                                writer.WritePropertyName("UpdatedMessage");
                                writer.WriteValue(sKWUpdateMessages.ToString());

                                //Get Detail Para Information and write into.xml file
                                if (arrParamsDescription.Count == 0) Logger.Error($"Keyword [{sKeywordName}] Feature [{Path.GetFileNameWithoutExtension(fiFeature.Name)}] Category [{diProduct.Name}], has no parameter, every keyword much have at least parameter [sOptional]");
                                writer.WritePropertyName("Params");
                                writer.WriteStartArray();
                                foreach (string param in arrParamsDescription)
                                {
                                    writer.WriteStartObject();
                                    Match mParam = rgGroupParam.Match(param);
                                    if (!mParam.Success)
                                    {
                                        Logger.Error($"Keyword [{sKeywordName}] Feature [{Path.GetFileNameWithoutExtension(fiFeature.Name)}] Category [{diProduct.Name}], Param should following format <param name=\"paramName\">Param Description. Exp: Example Value</param> - Current: {param}");
                                        Logger.Debug($"More information: One parameter must descriptive in only one line, not support on multiple line");
                                    }
                                    string sParamName = mParam.Groups["ParamName"].Value.Trim();
                                    string sParamDescription = mParam.Groups["ParamDescription"].Value.Trim();
                                    string sParamExmpaleValue = mParam.Groups["ParamExampleValue"].Value.Trim();
                                    writer.WritePropertyName("Name");
                                    writer.WriteValue(sParamName);
                                    writer.WritePropertyName("Description");
                                    writer.WriteValue(sParamDescription);
                                    writer.WritePropertyName("ExampleValue");
                                    writer.WriteValue(sParamExmpaleValue);

                                    writer.WriteEndObject();
                                }
                                writer.WriteEnd();
                                writer.WriteEndObject();

                            }
                            writer.WriteEnd(); ; //</Keyword>
                            writer.WriteEndObject();
                        }
                        writer.WriteEnd();
                        writer.WriteEndObject();
                    }

                    writer.WriteEnd(); //End Categories
                    writer.WriteEndObject();
                }
                result.Add("data", (JObject)JsonConvert.DeserializeObject(sb.ToString()));
                result.Add("result", "success");
                return result;
            }
            catch (Exception ex)
            {
                result.Add("result", "error");
                result.Add("data", null);
                throw new ApplicationException($"{ex}");
            }
        }

        public async Task<JObject> GenerateDevCode(List<TestCase> lstTestCases, IMongoCollection<Category> categories, IMongoCollection<TestSuite> testsuites, IMongoCollection<TestGroup> testgroups, IMongoCollection<TestAUT> testAUTs, IMongoCollection<Setting> settings)
        {
            JObject result = new JObject();
            JArray arrResult = new JArray();
            var TestProject = _appSettings.TestProject;
            var TestCasesFolder = Path.Combine(TestProject, "TestCases");
            var DefaultTestCaseTimeOutInMinus = _appSettings.DefaultTestCaseTimeOutInMinus;
            var MaximumTestCaseTimeOutInMinus = _appSettings.MaximumTestCaseTimeOutInMinus;
            var SupportedBrowsers = _appSettings.SupportedBrowsers;

            #region Delete item in file TestProject.csproj
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(_appSettings.TestProject, "TestProject.csproj"));
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
            string sXpath = $@"ns:Project/ns:ItemGroup/ns:Compile[@Include]";
            XmlNodeList nodes = doc.SelectNodes(sXpath, nsmgr);
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["Include"].Value.StartsWith("TestCaseIds") && !node.Attributes["Include"].Value.Contains(@"TestCaseIds\EXCLUDE"))
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
            doc.Save(Path.Combine(_appSettings.TestProject, "TestProject.csproj"));

            #endregion

            #region Delete all TestCaseIds Folder (but subfolder EXCLUDE) in TestProject

            if (Directory.Exists(TestCasesFolder))
            {
                DirectoryInfo diTestCases = new DirectoryInfo(TestCasesFolder);
                foreach (FileInfo fiItem in diTestCases.GetFiles())
                {
                    fiItem.Delete();
                }
                foreach (DirectoryInfo diItem in diTestCases.GetDirectories())
                {
                    if (!diItem.Name.ToUpper().Equals("EXCLUDE"))
                    {
                        diItem.Delete(true);
                    }
                }
            }
            else
            {
                Logger.Error($"There's no TestCaseIds Folder in TestProject, please check");
            }

            #endregion

            #region Generate Code

            //Get Import Block
            var importBlockSetting = await settings.Find(s => s.Name == "ImportBlock" && !s.IsDeleted).FirstOrDefaultAsync();
            var importBlockValue = string.Empty;
            if (importBlockSetting == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not found setting for importBlock");
                result.Add("data", null);
                return result;
            }

            importBlockValue = importBlockSetting.Value;
            var lstSupportBrowser = SupportedBrowsers.Split(",");

            List<string> lstDistinctTestSuites = new List<string>();

            foreach (var tcase in lstTestCases)
            {
                bool containsItem = lstDistinctTestSuites.Any(tsId => tsId.Equals(tcase.TestSuiteId));
                if (!containsItem) lstDistinctTestSuites.Add(tcase.TestSuiteId);
            }

            foreach (var tsId in lstDistinctTestSuites)
            {
                var testSuite = testsuites.Find<TestSuite>(ts => ts.Id == tsId).FirstOrDefault();
                foreach (var testcase in lstTestCases)
                {
                    if (!testcase.TestSuiteId.Equals(tsId)) continue;
                    var category = categories.Find<Category>(cat => cat.Id == testcase.CategoryId).FirstOrDefault();
                    var testGroup = testgroups.Find<TestGroup>(tg => tg.Id == testcase.TestGroupId).FirstOrDefault();
                    // Create Category folder if not exist
                    if (!Directory.Exists(Path.Combine(TestCasesFolder, category.Name)))
                    {
                        Directory.CreateDirectory(Path.Combine(TestCasesFolder, category.Name));
                    }

                    string tsCodeFile = Path.Combine(TestProject, "TestCases", category.Name, testSuite.CodeName + ".cs");

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(importBlockValue);

                    stringBuilder.AppendLine("");
                    stringBuilder.AppendLine($@"namespace TestProject.TestCases.{category.Name}");
                    stringBuilder.AppendLine(@"{");
                    stringBuilder.AppendLine("\t[TestFixture]");
                    stringBuilder.AppendLine($"\tclass {testSuite.CodeName} : SetupAndTearDown");
                    stringBuilder.AppendLine("\t{");
                    stringBuilder.AppendLine("");

                    //TestCase Block
                    int iOrder = 1;
                    foreach (TestCase tc in lstTestCases)
                    {
                        if (!tc.TestSuiteId.Equals(tsId)) continue;
                        bool isTearDownCommentAdded = false;
                        if (tc.TestSteps.Count == 0) continue;
                        if (tc.TimeOutInMinutes == 0)
                        {
                            stringBuilder.AppendLine($"\t\t[Test, Timeout({int.Parse(MaximumTestCaseTimeOutInMinus) * 60000}), Order({iOrder})]");
                        }
                        else
                        {
                            stringBuilder.AppendLine($"\t\t[Test, Timeout({tc.TimeOutInMinutes * 60000}), Order({iOrder})]");
                        }
                        stringBuilder.AppendLine($"\t\t[TestCaseId(\"{tc.Id}\")]");
                        stringBuilder.AppendLine($"\t\t[TestCaseCodeName(\"{tc.CodeName}\")]");
                        stringBuilder.AppendLine($"\t\t[TestCaseName(\"{tc.Name}\")]");
                        stringBuilder.AppendLine($"\t\t[Description(\"{tc.Description}\")]");
                        stringBuilder.AppendLine($"\t\t[Category(\"{category.Name}\")]");
                        stringBuilder.AppendLine($"\t\t[TestSuite(\"{testSuite.Name}\")]");
                        stringBuilder.AppendLine($"\t\t[TestGroup(\"{testGroup.Name}\")]");
                        stringBuilder.AppendLine($"\t\t[RunType(\"Dev\")]");
                        stringBuilder.AppendLine($"\t\t[Author(\"{tc.Owner}\")]");
                        stringBuilder.AppendLine($"\t\t[Team(\"{tc.Team}\")]");
                        stringBuilder.AppendLine($"\t\t[RunOwner(\"{Environment.MachineName}\")]");
                        stringBuilder.AppendLine($"\t\t[TestCaseType(\"{tc.TestCaseType}\")]");
                        List<string> lstDistinctAUTIds = new List<string>();
                        List<TestAUT> lstDistinctAUTs = new List<TestAUT>();
                        foreach (TestStep ts in tc.TestSteps)
                        {
                            if (ts.Keyword == null) continue; // Empty test step
                            if (string.IsNullOrEmpty(ts.Keyword.Name)) continue;
                            if (ts.IsDisabled || ts.IsDisabled || ts.Keyword.Name.ToUpper().Equals("CLEANUP")) continue;
                            bool containsItem = lstDistinctAUTIds.Any(item => item.ToUpper().Equals(ts.TestAUTId.ToUpper()));
                            if (!containsItem) lstDistinctAUTIds.Add(ts.TestAUTId);
                        }

                        foreach (string autId in lstDistinctAUTIds)
                        {
                            var testAUT = await testAUTs.Find<TestAUT>(aut => aut.Id == autId).FirstOrDefaultAsync();
                            lstDistinctAUTs.Add(testAUT);
                        }

                        foreach (TestAUT aut in lstDistinctAUTs)
                        {
                            if (lstSupportBrowser.Contains(aut.Name))
                            {
                                stringBuilder.AppendLine($"\t\t[WebDriver(\"{aut.Name}\")]");
                            }
                        }
                        stringBuilder.AppendLine($"\t\t[WorkItem(\"{tc.WorkItem}\")]");
                        stringBuilder.AppendLine($"\t\t// {tc.Name}");
                        stringBuilder.AppendLine($"\t\tpublic void {FirstLetterToUpper(tc.CodeName)}()");
                        stringBuilder.AppendLine("\t\t{");


                        foreach (TestAUT aut in lstDistinctAUTs)
                        {
                            if (lstSupportBrowser.Contains(aut.Name))
                            {
                                stringBuilder.AppendLine($"\t\t\tWebDriverFactory.InitBrowser(\"{aut.Name}\");");
                            }
                        }

                        //TestSteps block
                        List<TestStep> lstDistinctTestSteps = new List<TestStep>();
                        foreach (var testStep in tc.TestSteps)
                        {
                            if (string.IsNullOrEmpty(testStep.Keyword.Name)) continue; // Empty test step
                            if (testStep.IsDisabled || testStep.Keyword.Name.ToUpper().Equals("CLEANUP")) continue;
                            bool containsItem = lstDistinctTestSteps.Any(item => item.KWFeature == testStep.KWFeature && item.TestAUTId == testStep.TestAUTId);
                            if (!containsItem) lstDistinctTestSteps.Add(testStep);
                        }

                        foreach (TestStep testStep in lstDistinctTestSteps)
                        {
                            if (string.IsNullOrEmpty(testStep.Keyword.Name)) continue; // Empty test step
                            TestAUT aut = await testAUTs.Find<TestAUT>(aut => aut.Id == testStep.TestAUTId).FirstOrDefaultAsync();
                            if (lstSupportBrowser.Contains(aut.Name))
                            {
                                stringBuilder.Append($"\t\t\t{testStep.KWFeature} {aut.Name}_{testStep.KWFeature} = new {testStep.KWFeature}(WebDriverFactory.Driver");
                                stringBuilder.Append($"{aut.Name.Substring(aut.Name.Length - 1)}");
                                stringBuilder.AppendLine(");");
                            }
                            else
                            {
                                stringBuilder.AppendLine($"\t\t\t{testStep.KWFeature} {aut.Name}_{testStep.KWFeature} = new {testStep.KWFeature}();");
                            }
                        }

                        stringBuilder.AppendLine();

                        StringBuilder sBuilderCleanUpKeywords = new StringBuilder();
                        StringBuilder sBuilderKeywords = new StringBuilder();

                        //index of CleanUp Keyword
                        int indexCleanUp = tc.TestSteps.FindIndex(ts => ts.Keyword != null && ts.Keyword.Name.ToUpper().Equals("CLEANUP") && ts.IsDisabled == false);
                        Logger.Info(indexCleanUp == -1
                            ? $"Test case [{tc.Name}] has no CleanUp step"
                            : $"Test case [{tc.Name}] - CleanUp at index: {indexCleanUp}");
                        //Hardcoded indexCleanUp in case of user doesn't use CleanUp in the test case
                        if (indexCleanUp == -1) indexCleanUp = int.MaxValue;

                        // get devRunRecordId
                        stringBuilder.AppendLine("\t\t\tvar devRunRecordId = TestContext.CurrentContext.Test.Properties.Get(\"DevRunRecordId\")?.ToString();");

                        // TestSteps before cleanUp (main testSteps)
                        for (int i = 0; i < tc.TestSteps.Count; i++)
                        {
                            if (string.IsNullOrEmpty(tc.TestSteps[i].Keyword.Name)) continue; // Empty test step
                            if (tc.TestSteps[i].Keyword.Name.ToUpper().Equals("CLEANUP")) continue;
                            if (i >= indexCleanUp) continue;
                            // Keep TestStepID (to use for update TestStep: startDate, EndDate, status, ...) 
                            var setProperty = $"TestExecutionContext.CurrentContext.CurrentTest.Properties.Set(\"TestStepUUID\", \"{tc.TestSteps[i].UUID}\");";
                            if (tc.TestSteps[i].IsDisabled)
                            {
                                sBuilderKeywords.Append("\t\t\t// ");
                                sBuilderKeywords.AppendLine(setProperty);
                            }
                            else
                            {
                                sBuilderKeywords.AppendLine($"\t\t\t{setProperty}");

                            }

                            var updateStartTestStep =
                                $"MongoDBHelpers.StartRunningTestStepDev(devRunRecordId, \"{tc.TestSteps[i].UUID}\");";

                            sBuilderKeywords.AppendLine(tc.TestSteps[i].IsDisabled
                                ? $"\t\t\t// {updateStartTestStep}"
                                : $"\t\t\t{updateStartTestStep}");

                            TestAUT aut = await testAUTs.Find<TestAUT>(aut => aut.Id == tc.TestSteps[i].TestAUTId).FirstOrDefaultAsync();
                            if (!string.IsNullOrEmpty(tc.TestSteps[i].Description))
                            {
                                sBuilderKeywords.AppendLine($"\t\t\t// {tc.TestSteps[i].Description}");
                            }

                            var sKWwithParams = $"{aut.Name}_{tc.TestSteps[i].KWFeature}.{tc.TestSteps[i].Keyword.Name}(";

                            if (tc.TestSteps[i].IsDisabled)
                            {
                                sBuilderKeywords.Append($"\t\t\t// {sKWwithParams}");
                            }
                            else
                            {
                                sBuilderKeywords.Append($"\t\t\t{sKWwithParams}");
                            }
                            foreach (TestParam param in tc.TestSteps[i].Params)
                            {
                                if (tc.TestSteps[i].Params.IndexOf(param) == tc.TestSteps[i].Params.Count - 1)
                                {
                                    if (string.IsNullOrEmpty(param.Value))
                                    {
                                        sBuilderKeywords.Append($"null");
                                    }
                                    else
                                    {
                                        sBuilderKeywords.Append($"\"{param.Value}\"");
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(param.Value))
                                    {
                                        sBuilderKeywords.Append($"null, ");
                                    }
                                    else
                                    {
                                        sBuilderKeywords.Append($"\"{param.Value}\", ");
                                    }

                                }
                            }
                            sBuilderKeywords.AppendLine(");");

                            var updatePassedTestStep =
                                $"MongoDBHelpers.FinishRunningTestStepDev(devRunRecordId, \"{tc.TestSteps[i].UUID}\");";

                            sBuilderKeywords.AppendLine(tc.TestSteps[i].IsDisabled
                                ? $"\t\t\t// {updatePassedTestStep}"
                                : $"\t\t\t{updatePassedTestStep}");

                            sBuilderKeywords.AppendLine();
                        }


                        // TestSteps in cleanUp
                        if (indexCleanUp >= 0 && indexCleanUp != int.MaxValue)
                        {
                            sBuilderCleanUpKeywords.AppendLine("\t\t\t// Teardown");
                            sBuilderCleanUpKeywords.AppendLine("\t\t\tAdditionalTearDown(async () =>");
                            sBuilderCleanUpKeywords.AppendLine("\t\t\t{");
                            for (int i = 0; i < tc.TestSteps.Count; i++)
                            {
                                if (string.IsNullOrEmpty(tc.TestSteps[i].Keyword.Name)) continue; // Empty test step
                                if (tc.TestSteps[i].Keyword.Name.ToUpper().Equals("CLEANUP")) continue;
                                if (i < indexCleanUp) continue;

                                
                                TestAUT aut = await testAUTs.Find<TestAUT>(aut => aut.Id == tc.TestSteps[i].TestAUTId).FirstOrDefaultAsync();

                                var updateStartTestStep =
                                    $"await MongoDBHelpers.StartRunningTestStepDev(devRunRecordId, \"{tc.TestSteps[i].UUID}\");";

                                sBuilderCleanUpKeywords.AppendLine(tc.TestSteps[i].IsDisabled
                                    ? $"\t\t\t\t// TestExecutionContext.CurrentContext.CurrentTest.Properties.Set(\"TestStepUUID\", \"{tc.TestSteps[i].UUID}\");"
                                    : $"\t\t\t\tTestExecutionContext.CurrentContext.CurrentTest.Properties.Set(\"TestStepUUID\", \"{tc.TestSteps[i].UUID}\");");

                                sBuilderCleanUpKeywords.AppendLine(tc.TestSteps[i].IsDisabled
                                    ? $"\t\t\t\t// {updateStartTestStep}"
                                    : $"\t\t\t\t{updateStartTestStep}");

                                sBuilderCleanUpKeywords.Append(tc.TestSteps[i].IsDisabled
                                    ? $"\t\t\t\t// {aut.Name}_{tc.TestSteps[i].KWFeature}.{tc.TestSteps[i].Keyword.Name}("
                                    : $"\t\t\t\t{aut.Name}_{tc.TestSteps[i].KWFeature}.{tc.TestSteps[i].Keyword.Name}(");

                                foreach (TestParam param in tc.TestSteps[i].Params)
                                {
                                    if (tc.TestSteps[i].Params.IndexOf(param) == tc.TestSteps[i].Params.Count - 1)
                                    {
                                        if (string.IsNullOrEmpty(param.Value))
                                        {
                                            sBuilderCleanUpKeywords.Append($"null");
                                        }
                                        else
                                        {
                                            sBuilderCleanUpKeywords.Append($"\"{param.Value}\"");
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(param.Value))
                                        {
                                            sBuilderCleanUpKeywords.Append($"null, ");
                                        }
                                        else
                                        {
                                            sBuilderCleanUpKeywords.Append($"\"{param.Value}\", ");
                                        }
                                    }
                                }
                                sBuilderCleanUpKeywords.AppendLine(");");

                                var updatePassedTestStep =
                                    $"await MongoDBHelpers.FinishRunningTestStepDev(devRunRecordId, \"{tc.TestSteps[i].UUID}\");";

                                sBuilderCleanUpKeywords.AppendLine(tc.TestSteps[i].IsDisabled
                                    ? $"\t\t\t\t// {updatePassedTestStep}"
                                    : $"\t\t\t\t{updatePassedTestStep}");
                                sBuilderCleanUpKeywords.AppendLine();
                            }
                            sBuilderCleanUpKeywords.AppendLine("\t\t\t});");
                        }

                        stringBuilder.Append(sBuilderCleanUpKeywords);
                        stringBuilder.AppendLine();

                        stringBuilder.Append(sBuilderKeywords);
                        stringBuilder.AppendLine("\t\t}");
                        stringBuilder.AppendLine("");

                        iOrder++;
                    }
                    stringBuilder.AppendLine("\t}");
                    stringBuilder.AppendLine("}");

                    using (StreamWriter file = new StreamWriter(tsCodeFile))
                    {
                        file.WriteLine(stringBuilder.ToString());
                    }

                    JObject jObjectTestCase = new JObject();
                    jObjectTestCase.Add("testCase", testcase.CodeName);
                    jObjectTestCase.Add("category", category.Name);
                    jObjectTestCase.Add("testGroup", testGroup.CodeName);
                    jObjectTestCase.Add("testSuite", testSuite.CodeName);
                    jObjectTestCase.Add("testSuiteFile", tsCodeFile);
                    jObjectTestCase.Add("generatedCode", stringBuilder.ToString());
                    arrResult.Add(jObjectTestCase);
                }
            }
            result.Add("result", "success");
            result.Add("count", arrResult.Count);
            result.Add("message", arrResult);

            return result;
            #endregion
        }

        public async Task<JObject> GenerateRegressionCode(List<RegressionTest> lstRegressionTests, IMongoCollection<Category> categories, IMongoCollection<TestSuite> testsuites, IMongoCollection<TestGroup> testgroups, IMongoCollection<TestCase> testcases, IMongoCollection<TestAUT> testAUTs)
        {
            
            JObject result = new JObject();
            JArray arrResult = new JArray();
            var TestProject = _appSettings.TestProject;
            var TestCasesFolder = Path.Combine(TestProject, "TestCases");
            var TestProjectcsproj = Path.Combine(_appSettings.TestProject, "TestProject.csproj");            
            var DefaultTestCaseTimeOutInMinus = _appSettings.DefaultTestCaseTimeOutInMinus;
            var MaximumTestCaseTimeOutInMinus = _appSettings.MaximumTestCaseTimeOutInMinus;
            var SupportedBrowsers = _appSettings.SupportedBrowsers;

            #region Delete item in file TestProject.csproj
            XmlDocument doc = new XmlDocument();
            doc.Load(TestProjectcsproj);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
            string sXpath = $@"ns:Project/ns:ItemGroup/ns:Compile[@Include]";
            XmlNodeList nodes = doc.SelectNodes(sXpath, nsmgr);
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["Include"].Value.StartsWith("TestCaseIds") && !node.Attributes["Include"].Value.Contains(@"TestCaseIds\EXCLUDE"))
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
            doc.Save(TestProjectcsproj);

            #endregion

            #region Delete all TestCaseIds Folder (but subfolder EXCLUDE) in TestProject

            if (Directory.Exists(TestCasesFolder))
            {
                DirectoryInfo diTestCases = new DirectoryInfo(TestCasesFolder);
                foreach (FileInfo fiItem in diTestCases.GetFiles())
                {
                    fiItem.Delete();
                }
                foreach (DirectoryInfo diItem in diTestCases.GetDirectories())
                {
                    if (!diItem.Name.ToUpper().Equals("EXCLUDE"))
                    {
                        diItem.Delete(true);
                    }
                }
            }
            else
            {
                Logger.Error($"There's no TestCaseIds Folder in TestProject, please check");
            }

            #endregion

            #region Generate Code

            var lstSupportBrowser = SupportedBrowsers.Split(",");

            List<string> lstDistinctTestSuites = new List<string>();
            List<TestCaseExtend> lstTestCases = new List<TestCaseExtend>();
            foreach (var regTest in lstRegressionTests)
            {
                var tmpTestCase = await testcases.Find(tc => tc.Id == regTest.TestCaseId).FirstOrDefaultAsync();
                var tmpTestCaseExtend = new TestCaseExtend()
                {
                    RegressionTestId = regTest.Id,
                    Name = tmpTestCase.Name,
                    CodeName = tmpTestCase.CodeName,
                    TestCaseType = tmpTestCase.TestCaseType,
                    LastRunningStatus = tmpTestCase.LastRunningStatus,
                    IsPrimary = tmpTestCase.IsPrimary,
                    IsDisabled = tmpTestCase.IsDisabled,
                    IsDeleted = tmpTestCase.IsDeleted,
                    WorkItem = tmpTestCase.WorkItem,
                    Owner = tmpTestCase.Owner,
                    Team = tmpTestCase.Team,
                    Queue = tmpTestCase.Queue,
                    DontRunWithQueues = tmpTestCase.DontRunWithQueues,
                    CreatedDate = tmpTestCase.CreatedDate,
                    LastModifiedDate =  tmpTestCase.LastModifiedDate,
                    LastModifiedUser = tmpTestCase.LastModifiedUser,
                    Description = tmpTestCase.Description,
                    TimeOutInMinutes = tmpTestCase.TimeOutInMinutes,
                    DependOn =  tmpTestCase.DependOn,
                    CategoryId = tmpTestCase.CategoryId,
                    TestSuiteId = tmpTestCase.TestSuiteId,
                    TestGroupId = tmpTestCase.TestGroupId,
                    TestSteps = tmpTestCase.TestSteps,
                };
                if (tmpTestCase != null) lstTestCases.Add(tmpTestCaseExtend);
            }

            foreach (var tcase in lstTestCases)
            {
                bool containsItem = lstDistinctTestSuites.Any(tsId => tsId.Equals(tcase.TestSuiteId));
                if (!containsItem) lstDistinctTestSuites.Add(tcase.TestSuiteId);
            }

            foreach (var tsId in lstDistinctTestSuites)
            {
                var testSuite = testsuites.Find<TestSuite>(ts => ts.Id == tsId).FirstOrDefault();
                foreach (var testcase in lstTestCases)
                {
                    if (!testcase.TestSuiteId.Equals(tsId)) continue;
                    Logger.Debug($"TestCase: {JsonConvert.SerializeObject(testcase)}");
                    var category = categories.Find<Category>(cat => cat.Id == testcase.CategoryId).FirstOrDefault();
                    var testGroup = testgroups.Find<TestGroup>(tg => tg.Id == testcase.TestGroupId).FirstOrDefault();
                    // Create Category folder if not exist
                    if (!Directory.Exists(Path.Combine(TestCasesFolder, category.Name)))
                    {
                        Directory.CreateDirectory(Path.Combine(TestCasesFolder, category.Name));
                    }

                    string tsCodeFile = Path.Combine(TestProject, "TestCases", category.Name, testSuite.CodeName + ".cs");

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(@"using NUnit.Framework;");
                    stringBuilder.AppendLine(@"using NUnit.Framework.Internal;");
                    stringBuilder.AppendLine(@"using System;");
                    stringBuilder.AppendLine(@"using TestProject.Framework;");
                    stringBuilder.AppendLine(@"using TestProject.Framework.CustomAttributes;");
                    stringBuilder.AppendLine(@"using TestProject.Keywords;");
                    stringBuilder.AppendLine(@"using TestProject.Framework.WrapperFactory;");
                    stringBuilder.AppendLine(@"using TestProject.Keywords.DemoQA;");
                    stringBuilder.AppendLine("");
                    stringBuilder.AppendLine($@"namespace TestProject.TestCases.{category.Name}");
                    stringBuilder.AppendLine(@"{");
                    stringBuilder.AppendLine("\t[TestFixture]");
                    stringBuilder.AppendLine($"\tclass {testSuite.CodeName} : SetupAndTearDown");
                    stringBuilder.AppendLine("\t{");
                    stringBuilder.AppendLine("");

                    //TestCase Block
                    int iOrder = 1;
                    foreach (TestCaseExtend tc in lstTestCases)
                    {
                        if (!tc.TestSuiteId.Equals(tsId)) continue;
                        bool isTearDownCommentAdded = false;
                        if (tc.TestSteps.Count == 0) continue;
                        if (tc.TimeOutInMinutes == 0)
                        {
                            stringBuilder.AppendLine($"\t\t[Test, Timeout({int.Parse(MaximumTestCaseTimeOutInMinus) * 60000}), Order({iOrder})]");
                        }
                        else
                        {
                            stringBuilder.AppendLine($"\t\t[Test, Timeout({tc.TimeOutInMinutes * 60000}), Order({iOrder})]");
                        }
                        stringBuilder.AppendLine($"\t\t[TestCaseCodeName(\"{tc.CodeName}\")]");
                        stringBuilder.AppendLine($"\t\t[RegressionTestId(\"{tc.RegressionTestId}\")]");
                        stringBuilder.AppendLine($"\t\t[TestCaseName(\"{tc.Name}\")]");
                        stringBuilder.AppendLine($"\t\t[Description(\"{tc.Description}\")]");
                        stringBuilder.AppendLine($"\t\t[Category(\"{category.Name}\")]");
                        stringBuilder.AppendLine($"\t\t[TestSuite(\"{testSuite.Name}\")]");
                        stringBuilder.AppendLine($"\t\t[TestGroup(\"{testGroup.Name}\")]");
                        stringBuilder.AppendLine($"\t\t[RunType(\"Regression\")]");
                        stringBuilder.AppendLine($"\t\t[Author(\"{tc.Owner}\")]");
                        stringBuilder.AppendLine($"\t\t[Team(\"{tc.Team}\")]");
                        stringBuilder.AppendLine($"\t\t[RunOwner(\"{Environment.MachineName}\")]");
                        stringBuilder.AppendLine($"\t\t[TestCaseType(\"{tc.TestCaseType}\")]");
                        List<string> lstDistinctAUTIds = new List<string>();
                        List<TestAUT> lstDistinctAUTs = new List<TestAUT>();
                        foreach (TestStep ts in tc.TestSteps)
                        {
                            if (ts.Keyword == null) continue;
                            if (string.IsNullOrEmpty(ts.Keyword.Name)) continue;
                            if (ts.IsDisabled || ts.IsDisabled || ts.Keyword.Name.ToUpper().Equals("CLEANUP")) continue;
                            bool containsItem = lstDistinctAUTIds.Any(item => item.ToUpper().Equals(ts.TestAUTId.ToUpper()));
                            if (!containsItem) lstDistinctAUTIds.Add(ts.TestAUTId);
                        }

                        foreach (string autId in lstDistinctAUTIds)
                        {
                            var testAUT = await testAUTs.Find<TestAUT>(aut => aut.Id == autId).FirstOrDefaultAsync();
                            lstDistinctAUTs.Add(testAUT);
                        }

                        foreach (TestAUT aut in lstDistinctAUTs)
                        {
                            if (lstSupportBrowser.Contains(aut.Name))
                            {
                                stringBuilder.AppendLine($"\t\t[WebDriver(\"{aut.Name}\")]");
                            }
                        }
                        stringBuilder.AppendLine($"\t\t[WorkItem(\"{tc.WorkItem}\")]");
                        stringBuilder.AppendLine($"\t\t// {tc.Name}");
                        stringBuilder.AppendLine($"\t\tpublic void {FirstLetterToUpper(tc.CodeName)}()");
                        stringBuilder.AppendLine("\t\t{");


                        foreach (TestAUT aut in lstDistinctAUTs)
                        {
                            if (lstSupportBrowser.Contains(aut.Name))
                            {
                                stringBuilder.AppendLine($"\t\t\tWebDriverFactory.InitBrowser(\"{aut.Name}\");");
                            }
                        }

                        //TestSteps block
                        List<TestStep> lstDistinctTestSteps = new List<TestStep>();
                        foreach (var testStep in tc.TestSteps)
                        {
                            if (testStep.IsDisabled || testStep.Keyword.Name.ToUpper().Equals("CLEANUP")) continue;
                            bool containsItem = lstDistinctTestSteps.Any(item => item.KWFeature == testStep.KWFeature && item.TestAUTId == testStep.TestAUTId);
                            if (!containsItem) lstDistinctTestSteps.Add(testStep);
                        }

                        foreach (TestStep ts in lstDistinctTestSteps)
                        {
                            TestAUT aut = await testAUTs.Find<TestAUT>(aut => aut.Id == ts.TestAUTId).FirstOrDefaultAsync();
                            if (lstSupportBrowser.Contains(aut.Name))
                            {
                                stringBuilder.Append($"\t\t\t{ts.KWFeature} {aut.Name}_{ts.KWFeature} = new {ts.KWFeature}(WebDriverFactory.Driver");
                                stringBuilder.Append($"{aut.Name.Substring(aut.Name.Length - 1)}");
                                stringBuilder.AppendLine(");");
                            }
                            else
                            {
                                stringBuilder.AppendLine($"\t\t\t{ts.KWFeature} {aut.Name}_{ts.KWFeature} = new {ts.KWFeature}();");
                            }
                        }

                        stringBuilder.AppendLine("\t\t\t// ------------------------------------------------------");
                        stringBuilder.AppendLine();

                        StringBuilder sBuilderCleanUpKeywords = new StringBuilder();
                        StringBuilder sBuilderKeywords = new StringBuilder();

                        //index of CleanUp Keyword
                        int indexCleanUp = tc.TestSteps.FindIndex(ts => ts.Keyword.Name.ToUpper().Equals("CLEANUP"));
                        Logger.Info(indexCleanUp == -1
                            ? $"Test case [{tc.Name}] has no CleanUp step"
                            : $"Test case [{tc.Name}] - CleanUp at index: {indexCleanUp}");
                        //I do hardcoded indexCleanUp in case of user doesn't use CleanUp in the test case
                        if (indexCleanUp == -1) indexCleanUp = int.MaxValue;
                        for (int i = 0; i < tc.TestSteps.Count; i++)
                        {
                            if (tc.TestSteps[i].Keyword.Name.ToUpper().Equals("CLEANUP")) continue;
                            // BEFORE CLEANUP
                            if (i < indexCleanUp)
                            {
                                if (tc.TestSteps[i].IsDisabled)
                                {
                                    sBuilderKeywords.Append("\t\t\t// ");
                                }
                                else
                                {
                                    sBuilderKeywords.AppendLine($"\t\t\tTestExecutionContext.CurrentContext.CurrentTest.Properties.Set(\"TestStepUUID\", \"{tc.TestSteps[i].UUID}\");");
                                    sBuilderKeywords.Append("\t\t\t");
                                }
                                TestAUT aut = await testAUTs.Find<TestAUT>(aut => aut.Id == tc.TestSteps[i].TestAUTId).FirstOrDefaultAsync();
                                sBuilderKeywords.Append($"{aut.Name}_{tc.TestSteps[i].KWFeature}.{tc.TestSteps[i].Keyword.Name}(");
                                foreach (TestParam param in tc.TestSteps[i].Params)
                                {
                                    if (tc.TestSteps[i].Params.IndexOf(param) == tc.TestSteps[i].Params.Count - 1)
                                    {
                                        if (string.IsNullOrEmpty(param.Value))
                                        {
                                            sBuilderKeywords.Append($"null");
                                        }
                                        else
                                        {
                                            sBuilderKeywords.Append($"\"{param.Value}\"");
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(param.Value))
                                        {
                                            sBuilderKeywords.Append($"null, ");
                                        }
                                        else
                                        {
                                            sBuilderKeywords.Append($"\"{param.Value}\", ");
                                        }

                                    }
                                }
                                sBuilderKeywords.AppendLine(");");
                            }
                            //AFTER CLEANUP
                            else
                            {
                                if (!isTearDownCommentAdded)
                                {
                                    sBuilderCleanUpKeywords.AppendLine("\t\t\t// Teardown");
                                    isTearDownCommentAdded = true;
                                }

                                if (tc.TestSteps[i].IsDisabled)
                                {
                                    sBuilderCleanUpKeywords.Append("\t\t\t// ");
                                }
                                else
                                {
                                    sBuilderCleanUpKeywords.Append("\t\t\t");
                                }
                                TestAUT aut = await testAUTs.Find<TestAUT>(aut => aut.Id == tc.TestSteps[i].TestAUTId).FirstOrDefaultAsync();
                                sBuilderCleanUpKeywords.AppendLine($"AdditionalTearDown(() =>");
                                sBuilderCleanUpKeywords.AppendLine("\t\t\t{");
                                sBuilderCleanUpKeywords.AppendLine($"\t\t\t\tTestExecutionContext.CurrentContext.CurrentTest.Properties.Set(\"Keyword\", \"{tc.TestSteps[i].Keyword.Name}\");");
                                sBuilderCleanUpKeywords.Append($"\t\t\t\t{aut.Name}_{tc.TestSteps[i].KWFeature}.{tc.TestSteps[i].Keyword.Name}(");
                                foreach (TestParam param in tc.TestSteps[i].Params)
                                {
                                    if (tc.TestSteps[i].Params.IndexOf(param) == tc.TestSteps[i].Params.Count - 1)
                                    {
                                        if (string.IsNullOrEmpty(param.Value))
                                        {
                                            sBuilderCleanUpKeywords.Append($"null");
                                        }
                                        else
                                        {
                                            sBuilderCleanUpKeywords.Append($"\"{param.Value}\"");
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(param.Value))
                                        {
                                            sBuilderCleanUpKeywords.Append($"null, ");
                                        }
                                        else
                                        {
                                            sBuilderCleanUpKeywords.Append($"\"{param.Value}\", ");
                                        }
                                    }
                                }
                                sBuilderCleanUpKeywords.AppendLine(");");
                                sBuilderCleanUpKeywords.AppendLine("\t\t\t});");
                            }
                        }

                        stringBuilder.Append(sBuilderCleanUpKeywords);
                        stringBuilder.AppendLine("\t\t\t// ------------------------------------------------------");
                        stringBuilder.AppendLine();

                        stringBuilder.Append(sBuilderKeywords);
                        stringBuilder.AppendLine("\t\t}");
                        stringBuilder.AppendLine("");

                        iOrder++;
                    }
                    stringBuilder.AppendLine("\t}");
                    stringBuilder.AppendLine("}");

                    using (StreamWriter file = new StreamWriter(tsCodeFile))
                    {
                        file.WriteLine(stringBuilder.ToString());
                    }

                    JObject jObjectTestCase = new JObject();
                    jObjectTestCase.Add("testCase", testcase.CodeName);
                    jObjectTestCase.Add("category", category.Name);
                    jObjectTestCase.Add("testGroup", testGroup.CodeName);
                    jObjectTestCase.Add("testSuite", testSuite.CodeName);
                    jObjectTestCase.Add("testSuiteFile", tsCodeFile);
                    jObjectTestCase.Add("generatedCode", stringBuilder.ToString());
                    arrResult.Add(jObjectTestCase);
                }
            }
            result.Add("result", "success");
            result.Add("count", arrResult.Count);
            result.Add("message", arrResult);

            return result;
            #endregion
        }

        public static async Task<JObject> CreateDevQueue(List<TestCase> testCases, TestClient testClient, IMongoCollection<DevQueue> devqueues, IMongoCollection<Category> categories, IMongoCollection<TestSuite> testsuites)
        {
            JObject result = new JObject();
            JArray arrResult = new JArray();
            foreach (var tc in testCases)
            {
                JObject queueObject = new JObject();
                var category = await categories.Find<Category>(cat => cat.Id == tc.CategoryId).FirstOrDefaultAsync();
                var testsuite = await testsuites.Find<TestSuite>(ts => ts.Id == tc.TestSuiteId).FirstOrDefaultAsync();
                //upperCase first character of CodeName

                var CodeName = char.ToUpper(tc.CodeName[0]) + tc.CodeName.Substring(1);

                DevQueue devQueue = new DevQueue
                {
                    TestCaseId = tc.Id,
                    TestCaseCodeName = CodeName,
                    TestCaseName = tc.Name,
                    TestCaseFullName = $"TestProject.TestCases.{category.Name}.{testsuite.CodeName}.{CodeName}",
                    QueueStatus = TestStatus.InQueue,
                    QueueType = string.Empty,
                    CreateAt = DateTime.UtcNow,
                    ClientName = testClient.Name,
                    IsHighPriority = false,
                };
                var filters = Builders<DevQueue>.Filter.Eq("TestCaseId", tc.Id)
                              & Builders<DevQueue>.Filter.Eq("QueueStatus", "InQueue")
                              & Builders<DevQueue>.Filter.Eq("ClientName", testClient.Name);
                var found = devqueues.Find(filters).FirstOrDefault();
                if (found == null)
                {
                    await devqueues.InsertOneAsync(devQueue);
                    queueObject = (JObject)JToken.FromObject(devQueue);
                    arrResult.Add(queueObject);
                }
            }

            result.Add("result", "success");
            result.Add("count", arrResult.Count);
            result.Add("message", arrResult);

            return result;
        }

        public async Task<JObject> BuildProject()
        {
            var TestProjectcsproj = Path.Combine(_appSettings.TestProject, "TestProject.csproj");
            JObject result = new JObject();
            int intExitCode;
            Logger.Info("-- Start Build Project");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "dotnet.exe";
            startInfo.Arguments = $"build {TestProjectcsproj}";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            //
            // Start the process.
            //
            using (Process process = Process.Start(startInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string line = reader.ReadToEnd();
                    result.Add("buildMessage", line);
                    Logger.Debug(line);
                }
                process.WaitForExit();
                intExitCode = process.ExitCode;
            }
            Logger.Info("-- End Build Project");
            result.Add("result", intExitCode != 0 ? "error" : "success");

            return result;
        }

        public static async Task<JObject> GetLatestCode()
        {
            JObject result = new JObject();
            int intExitCode;
            Logger.Info("-- Start Get Latest Code");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "git.exe";
            startInfo.Arguments = $"pull";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            //
            // Start the process.
            //
            using (Process process = Process.Start(startInfo))
            {
                //
                // Read in all the text from the process with the StreamReader.
                //
                using (StreamReader reader = process.StandardOutput)
                {
                    string line = reader.ReadToEnd();
                    result.Add("message", line);
                    Logger.Debug(line);
                }
                process.WaitForExit();
                intExitCode = process.ExitCode;
            }
            Logger.Info("-- End Get Latest Code");
            result.Add("result", intExitCode != 0 ? "error" : "success");

            return result;
        }

        public static async Task<JObject> CopyCodeToClient(TestClient testClient, string type, IATTMAppSettings appSettings)
        {
            JObject result = new JObject();
            var sourceDir = appSettings.BuiltSource;
            var destDir = string.Empty;
            switch (type.ToLower())
            {
                case "dev":
                    destDir = $@"\\{testClient.IPAddress}\{testClient.DevelopFolder}";
                    break;
                case "develop":
                    destDir = $@"\\{testClient.IPAddress}\{testClient.DevelopFolder}";
                    break;
                case "reg":
                    destDir = $@"\\{testClient.IPAddress}\{testClient.RegressionFolder}";
                    break;
                case "regression":
                    destDir = $@"\\{testClient.IPAddress}\{testClient.RegressionFolder}";
                    break;
                default:
                    result.Add("result", "error");
                    result.Add("message", $"type should be dev, reg, or regression, your input is: {type}");
                    return result;
            }
            try
            {
                if (!Directory.Exists(destDir))
                {
                    result.Add("result", "error");
                    result.Add("message", $"Directory not found {destDir}");
                    return result;
                }
                foreach (var innerDir in Directory.GetDirectories(sourceDir, "*.*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(innerDir.Replace(sourceDir, destDir));
                }

                foreach (var file in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(file, file.Replace(sourceDir, destDir), true);
                }
            }
            catch (Exception e)
            {
                result.Add("result", "error");
                result.Add("message", e.Message);
                return result;
            }
            
            result.Add("result", "success");
            result.Add("message", $"Copied to client in ${destDir} !");
            return result;
        }

        public static async Task<JObject> RunAutoRunner(TestClient testClient, IATTMAppSettings appSettings)
        {
            JObject result = new JObject();

            int intExitCode;
            StringBuilder sbBuilder = new StringBuilder();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = appSettings.PSToolsFolder;
            startInfo.FileName = @"C:\pstools\psexec.exe";
            startInfo.Arguments = $@"\\{testClient.IPAddress} -accepteula -nobanner -u {testClient.User} -p {testClient.Password} -i 2 -d {testClient.RunnerFolder}";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            using (Process process = Process.Start(startInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string line = reader.ReadToEnd();
                    sbBuilder.AppendLine(line);
                    Logger.Debug(line);
                }
                using (StreamReader reader = process.StandardError)
                {
                    string line = reader.ReadToEnd();
                    sbBuilder.AppendLine(line);
                    Logger.Debug(line);
                }
                await process.WaitForExitAsync();
                intExitCode = process.ExitCode;
            }

            Logger.Info($"-- ExitCode: {intExitCode}");
            result.Add("result", intExitCode != 0 ? "error" : "success");
            result.Add("message", sbBuilder.ToString());

            return result;
        }

        public static async Task<JObject> CheckRunner(TestClient testClient, string processName, IATTMAppSettings appSettings)
        {
            JObject result = new JObject();

            var arguments = string.Empty;
            if (string.IsNullOrEmpty(testClient.User) || string.IsNullOrEmpty(testClient.Password))
            {
                arguments = $@"\\{testClient.IPAddress} -accepteula -nobanner -e {processName}";
            }
            else
            {
                arguments = $@"\\{testClient.IPAddress} -accepteula -nobanner -u {testClient.User} -p {testClient.Password} -e {processName}";
            }
            
            StringBuilder sbBuilder = new StringBuilder();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = $@"{appSettings.PSToolsFolder}\pslist.exe";
            startInfo.Arguments = arguments;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            using (Process process = Process.Start(startInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string line = reader.ReadToEnd();
                    sbBuilder.AppendLine(line);
                    Logger.Debug(line);
                }
                using (StreamReader reader = process.StandardError)
                {
                    string line = reader.ReadToEnd();
                    sbBuilder.AppendLine(line);
                    Logger.Debug(line);
                }
                await process.WaitForExitAsync();
            }
            if (sbBuilder.ToString().Contains($"Elapsed Time")) {
                result.Add("message", $"{processName} is running on {testClient.Name}!");
            }
            else if (sbBuilder.ToString().Contains($"process {processName} was not found"))
            {
                result.Add("message", $"{processName} is not running on client: {testClient.Name}!");
            } else
            {
                result.Add("message", $"{processName} is not running on client: {testClient.Name}!");
            }
            result.Add("result", "success");
            result.Add("data", sbBuilder.ToString());

            return result;
        }

        public static async Task<JObject> UpdateReleaseForClient(TestClient testClient, string ReleaseName)
        {
            JObject result = new JObject();
            int intExitCode;
            StringBuilder sbBuilder = new StringBuilder();
            Logger.Info("-- Start updating Release for client");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "powershell.exe";
            startInfo.Arguments = $@"-NoProfile -ExecutionPolicy unrestricted {Path.Combine(Path.GetDirectoryName(sRootDLL), "Helpers", "updateRegressionSetting.ps1")} {testClient.IPAddress} {testClient.User} {testClient.Password} {testClient.RunnerFolder} {ReleaseName}";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            using (Process process = Process.Start(startInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string line = reader.ReadToEnd();
                    sbBuilder.AppendLine(line);
                    Logger.Debug(line);
                }
                await process.WaitForExitAsync();
                intExitCode = process.ExitCode;
            }

            Logger.Info("-- End updating Release for client");
            Logger.Info($"-- ExitCode: {intExitCode}");
            result.Add("result", intExitCode != 0 ? "error" : "success");
            result.Add("message", sbBuilder.ToString());

            return result;
        }
        private static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
    }

    public class TestCaseExtend: TestCase
    {
        public string RegressionTestId;
    }
}