using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ATTM_API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;
using NUnit.Framework;
using static System.Net.Mime.MediaTypeNames;
using Formatting = Newtonsoft.Json.Formatting;
using Microsoft.Extensions.Configuration;


namespace ATTM_API.Helpers
{
    public class TestProjectHelper
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));

        private static string sRootDLL = System.Reflection.Assembly.GetExecutingAssembly().Location;
        public static string sRootPath = Path.GetDirectoryName(sRootDLL);
        private static DirectoryInfo drInfoRoot = new DirectoryInfo(sRootPath);
        public static string sProjectPath = drInfoRoot.Parent.Parent.Parent.Parent.FullName;
        public static string sTestCasesFolder = Path.Combine(sProjectPath, "TestProject", "TestCases");
        public static string sTestProjectCsharpcsproj = Path.Combine(sProjectPath, "TestProject", "TestProject.csproj");
        public static string sKeyWordsFolder = Path.Combine(sProjectPath, "TestProject", "Keywords");
        public static string sTestProjectFolder = drInfoRoot.Parent.FullName;
        public static string sTestProjectDLL = Path.Combine(sTestProjectFolder, "TestProject", "TestProject.dll");
        public static string sKeywordListFile = Path.Combine(Path.GetTempPath(), "Keyword.json");

        
        public static string sPatternStartSummary = "/// <summary>";
        public static string sPatternEndSummary = "/// </summary>";
        public static string sPatternParam = @"/// <param.*</param>$";
        public static string sPatternGroupKeyword = @"public void (?<KeywordName>.+)\((?<Params>.+)\).*";
        public static string sPatternGroupParam = "<param name=\"(?<ParamName>.+)\">(?<ParamDescription>.+)Exp:(?<ParamExampleValue>.+)</param>";

        /// <summary>
        /// Get the keyword list from Test\Keywords
        /// Store it in keywords.json
        /// </summary>
        public static void GetKeywords()
        {
            Regex rgStartSummary = new Regex(sPatternStartSummary, RegexOptions.IgnoreCase);
            Regex rgEndSummary = new Regex(sPatternEndSummary);
            Regex rgParam = new Regex(sPatternParam);
            Regex rgGroupKeyword = new Regex(sPatternGroupKeyword);
            Regex rgGroupParam = new Regex(sPatternGroupParam);
            try
            {
                using (StreamWriter file = File.CreateText(sKeywordListFile))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    writer.Formatting = Formatting.None;
                    writer.WriteStartObject();
                    writer.WritePropertyName("_id");
                    writer.WriteValue($"{ObjectId.GenerateNewId()}");
                    writer.WritePropertyName("refreshDate");
                    writer.WriteValue(DateTime.UtcNow);
                    writer.WritePropertyName("categories");     //Start Categories
                    writer.WriteStartArray();

                    DirectoryInfo diKeywordFolder = new DirectoryInfo(sKeyWordsFolder);
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
                                    catch (Exception ex)
                                    {
                                        Logger.Error($"{ex}");
                                    }
                                }
                                //Write to XML file for sKWDescriptions and sKWUpdateMessages
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

                    writer.WriteEnd();                          //End Categories
                    writer.WriteEndObject();
                }

                Logger.Info($"Get Keyword list and store to file successfully");
            }

            catch (Exception ex)
            {
                throw new ApplicationException($"{ex}");
            }
        }

        public static void GenerateCode(List<TestCase> lstTestCases, string runType, bool isDebug = false)
        {
            var TestProjectCsharp = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["TestProjectCsharp"];
            var DefaultTestCaseTimeOutInMinus = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["DefaultTestCaseTimeOutInMinus"];
            var MaximumTestCaseTimeOutInMinus = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["MaximumTestCaseTimeOutInMinus"];
            var SupportBrowser = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["SupportBrowser"];

            #region Delete item in file TestProjectCsharp.csproj
            XmlDocument doc = new XmlDocument();
            doc.Load(sTestProjectCsharpcsproj);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
            string sXpath = $@"ns:Project/ns:ItemGroup/ns:Compile[@Include]";
            XmlNodeList nodes = doc.SelectNodes(sXpath, nsmgr);
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["Include"].Value.StartsWith("TestCases") && !node.Attributes["Include"].Value.Contains(@"TestCases\EXCLUDE"))
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
            doc.Save(sTestProjectCsharpcsproj);

            #endregion

            #region Delete all TestCases Folder (but subfolder EXCLUDE) in TestProjectCsharp

            if (Directory.Exists(sTestCasesFolder))
            {
                DirectoryInfo diTestCases = new DirectoryInfo(sTestCasesFolder);
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
                Logger.Error($"There's no TestCases Folder in TestProjectCsharp, please check");
            }

            #endregion

            #region Generate Code

            var lstSupportBrowser = SupportBrowser.Split(",");

            foreach (var testcase in lstTestCases)
            {
                Logger.Debug($"TestCase: {JsonConvert.SerializeObject(testcase)}");
                // Create Category folder if not exist
                if (!Directory.Exists(Path.Combine(sTestCasesFolder, testcase.CategoryName)))
                {
                    Directory.CreateDirectory(Path.Combine(sTestCasesFolder, testcase.CategoryName));
                }

                // Create TestSuite file into Category folder
                // If TestSuite file is not exist --> create it + add some packages, setup, teardown, testcase part....
                // If TestSuite is exist --> only insert testcase part


                //if (!File.Exists(Path.Combine(sTestCasesFolder, tc.Category, tc.TestSuite + ".cs")))
                //{
                //    File.Create(Path.Combine(sTestCasesFolder, tc.Category, tc.TestSuite + ".cs"));

                //}

                string tsCodeFile = Path.Combine(TestProjectCsharp, "TestCases", testcase.CategoryName, testcase.TestSuiteId + ".cs");

                //File.Create(tsCodeFile);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(@"using NUnit.Framework;");
                stringBuilder.AppendLine(@"using NUnit.Framework.Internal;");
                stringBuilder.AppendLine(@"using System;");
                stringBuilder.AppendLine(@"using TestProjectCSharp.Framework;");
                stringBuilder.AppendLine(@"using TestProjectCSharp.Framework.CustomAttributes;");
                stringBuilder.AppendLine(@"using SQLServices;");
                stringBuilder.AppendLine(@"using TestProjectCSharp.Keywords;");
                stringBuilder.AppendLine(@"using TestProjectCSharp.Framework.WrapperFactory;");
                stringBuilder.AppendLine(@"using TestProjectCSharp.Keywords.Saucedemo;");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine($@"namespace TestProjectCSharp.TestCases");
                stringBuilder.AppendLine(@"{");
                stringBuilder.AppendLine("\t[TestFixture]");
                stringBuilder.AppendLine($"\tclass {testcase.TestSuiteId} : SetupAndTearDown");
                stringBuilder.AppendLine("\t{");
                stringBuilder.AppendLine("\t\tstatic int RunId;");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("\t\t[OneTimeSetUp]");
                stringBuilder.AppendLine("\t\tpublic void ClassSetup()");
                stringBuilder.AppendLine("\t\t{");
                switch (runType.ToUpper())
                {
                    case string debug when debug.Contains("DEVELOP"):
                        stringBuilder.AppendLine("\t\t\tRunId = SQLUtils.LastRunId_Plus_1(\"develop\");");
                        break;
                    case string regression when regression.Contains("REGRESSION"):
                        stringBuilder.AppendLine("\t\t\tRunId = SQLUtils.LastRunId_Plus_1(\"regression\");");
                        break;
                }
                stringBuilder.AppendLine("\t\t\tTestExecutionContext.CurrentContext.CurrentTest.Properties.Add(\"RunId\", RunId);");
                stringBuilder.AppendLine("\t\t}");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("\t\t[OneTimeTearDown]");
                stringBuilder.AppendLine("\t\tpublic void ClassTearDown()");
                stringBuilder.AppendLine("\t\t{");
                stringBuilder.AppendLine("\t\t}");
                stringBuilder.AppendLine("");
                //TestCase Block
                int iOrder = 1;
                foreach (TestCase tc in lstTestCases)
                {
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
                    stringBuilder.AppendLine($"\t\t[TestCaseId(\"{tc.tcId}\")]");
                    stringBuilder.AppendLine($"\t\t[TestCaseName(\"{tc.Name}\")]");
                    stringBuilder.AppendLine($"\t\t[Description(\"{tc.Description}\")]");
                    stringBuilder.AppendLine($"\t\t[Category(\"{tc.CategoryName}\")]");
                    stringBuilder.AppendLine($"\t\t[TestSuite(\"{tc.TestSuiteId}\")]");
                    stringBuilder.AppendLine($"\t\t[TestGroup(\"{tc.TestGroupId}\")]");
                    if (isDebug) stringBuilder.AppendLine($"\t\t[IsDebug(\"true\")]");
                    else stringBuilder.AppendLine($"\t\t[IsDebug(\"false\")]");
                    stringBuilder.AppendLine($"\t\t[RunType(\"{runType}\")]");
                    stringBuilder.AppendLine($"\t\t[Author(\"{tc.Designer}\")]");
                    stringBuilder.AppendLine($"\t\t[Team(\"{tc.Team}\")]");
                    stringBuilder.AppendLine($"\t\t[RunOwner(\"{Environment.MachineName}\")]");
                    stringBuilder.AppendLine($"\t\t[Type(\"{tc.Type}\")]");
                    List<string> lstDistinctAUTs = new List<string>();
                    foreach (TestStep ts in tc.TestSteps)
                    {
                        if (string.IsNullOrEmpty(ts.Keyword)) continue;
                        if (ts.isDisabled || ts.isComment || ts.Keyword.ToUpper().Equals("CLEANUP")) continue;
                        bool containsItem = lstDistinctAUTs.Any(item => item.ToUpper().Equals(ts.TestAUT.ToUpper()));
                        if (!containsItem) lstDistinctAUTs.Add(ts.TestAUT);
                    }
                    //Base on testAUTName ==> get testAUTType == BROWSER, expected to know how many WebDriver used in testcase.
                    foreach (string aut in lstDistinctAUTs)
                    {
                        if (lstSupportBrowser.Contains(aut))
                        {
                            stringBuilder.AppendLine($"\t\t[WebDriver(\"{aut}\")]");
                        }
                    }
                    stringBuilder.AppendLine($"\t\t[WorkItem(\"{tc.WorkItem}\")]");
                    stringBuilder.AppendLine($"\t\t// {tc.Name}");
                    stringBuilder.AppendLine($"\t\tpublic void {tc.tcId}()");
                    stringBuilder.AppendLine("\t\t{");


                    foreach (string aut in lstDistinctAUTs)
                    {
                        if (lstSupportBrowser.Contains(aut))
                        {
                            stringBuilder.AppendLine($"\t\t\tWebDriverFactory.InitBrowser(\"{aut}\");");
                        }
                    }

                    //TestSteps block
                    List<TestStep> lstDistinctTestSteps = new List<TestStep>();
                    foreach (TestStep ts in tc.TestSteps)
                    {
                        if (ts.isDisabled || ts.isComment || ts.Keyword.ToUpper().Equals("CLEANUP")) continue;
                        bool containsItem = lstDistinctTestSteps.Any(item => item.Keyword == ts.Keyword && item.TestAUT == ts.TestAUT);
                        if (!containsItem) lstDistinctTestSteps.Add(ts);
                    }

                    foreach (TestStep ts in lstDistinctTestSteps)
                    {
                        if (lstSupportBrowser.Contains(ts.TestAUT))
                        {
                            stringBuilder.Append($"\t\t\t{ts.Feature} {ts.TestAUT}_{ts.Feature} = new {ts.Feature}(WebDriverFactory.Driver");
                            stringBuilder.Append($"{ts.TestAUT.Substring(ts.TestAUT.Length - 1)}");
                            stringBuilder.AppendLine(");");
                        }
                        else
                        {
                            stringBuilder.AppendLine($"\t\t\t{ts.Feature} {ts.TestAUT}_{ts.Feature} = new {ts.Feature}();");
                        }
                    }

                    stringBuilder.AppendLine("\t\t\t// ------------------------------------------------------");
                    stringBuilder.AppendLine();

                    StringBuilder sBuilderCleanUpKeywords = new StringBuilder();
                    StringBuilder sBuilderKeywords = new StringBuilder();

                    //index of CleanUp Keyword
                    int indexCleanUp = tc.TestSteps.FindIndex(ts => ts.Keyword.ToUpper().Equals("CLEANUP"));
                    Logger.Info(indexCleanUp == -1
                        ? $"Test case [{tc.Name}] has no CleanUp step"
                        : $"Test case [{tc.Name}] - CleanUp at index: {indexCleanUp}");
                    //I do hard code indexCleanUp in case of user doesn't use CleanUp in the test case
                    if (indexCleanUp == -1) indexCleanUp = int.MaxValue;
                    for (int i = 0; i < tc.TestSteps.Count; i++)
                    {
                        if (tc.TestSteps[i].Keyword.ToUpper().Equals("CLEANUP")) continue;
                        // BEFORE CLEANUP
                        if (i < indexCleanUp)
                        {
                            if (tc.TestSteps[i].isComment)
                            {
                                sBuilderKeywords.AppendLine($"\t\t\t// {tc.TestSteps[i].Params[0].Value}");
                            }
                            else
                            {
                                if (tc.TestSteps[i].isDisabled)
                                {
                                    sBuilderKeywords.Append("\t\t\t// ");
                                }
                                else
                                {
                                    sBuilderKeywords.Append("\t\t\t");
                                }

                                sBuilderKeywords.Append($"{tc.TestSteps[i].TestAUT}_{tc.TestSteps[i].Feature}.{tc.TestSteps[i].Keyword}(");
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
                        }
                        //AFTER CLEANUP
                        else
                        {
                            if (!isTearDownCommentAdded)
                            {
                                sBuilderCleanUpKeywords.AppendLine("\t\t\t// Teardown");
                                isTearDownCommentAdded = true;
                            }

                            if (tc.TestSteps[i].isComment)
                            {
                                sBuilderCleanUpKeywords.AppendLine($"\t\t\t// {tc.TestSteps[i].Params[0].Value}");
                            }
                            else
                            {
                                if (tc.TestSteps[i].isDisabled)
                                {
                                    sBuilderCleanUpKeywords.Append("\t\t\t// ");
                                }
                                else
                                {
                                    sBuilderCleanUpKeywords.Append("\t\t\t");
                                }

                                sBuilderCleanUpKeywords.Append($"AdditionalTearDown(() => {tc.TestSteps[i].TestAUT}_{tc.TestSteps[i].Feature}.{tc.TestSteps[i].Keyword}(");
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
                                sBuilderCleanUpKeywords.AppendLine("));");
                            }
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

                // Add Compile node in TestProjectCsharp.csproj
                doc.Load(Path.Combine(TestProjectCsharp, "TestProjectCSharp.csproj"));
                nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
                XmlElement CompileElement = doc.CreateElement("Compile", "http://schemas.microsoft.com/developer/msbuild/2003");

                // Get string from TestCases file. Exp: from: C:\dev\ATTM\TestProjectCsharp\TestCases\Appium2\Appium_Test2 - Copy.cs to: TestCases\Appium2\Appium_Test2 - Copy.cs
                string sIncludeValue = string.Empty;
                string separator = "TestCases";
                int separatorIndex = tsCodeFile.IndexOf(separator, StringComparison.Ordinal);
                if (separatorIndex >= 0)
                {
                    sIncludeValue = $"TestCases{tsCodeFile.Substring(separatorIndex + separator.Length)}";
                }
                CompileElement.SetAttribute("Include", sIncludeValue);
                XmlNode xmlCompileNode = doc.SelectSingleNode(sXpath, nsmgr);
                xmlCompileNode.ParentNode.AppendChild(CompileElement);
                doc.Save(Path.Combine(TestProjectCsharp, "TestProjectCSharp.csproj"));
            }
            #endregion

        }
    }
}