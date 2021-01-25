using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace ATTM_API.Helpers
{
    public class CSharpTestProjectHelper
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        public static string[] supportedBrowers = new[]
        {
            "Chrome1",
            "Chrome2",
            "Chrome3",
            "FireFox1",
            "FireFox2",
            "FireFox3"
        };
        private static string sRootDLL = System.Reflection.Assembly.GetExecutingAssembly().Location;
        public static string sRootPath = Path.GetDirectoryName(sRootDLL);
        private static DirectoryInfo drInfoRoot = new DirectoryInfo(sRootPath);
        public static string sProjectPath = drInfoRoot.Parent.Parent.Parent.Parent.FullName;
        public static string sTestCasesFolder = Path.Combine(sProjectPath, "TestFW", "TestProjectCsharp", "TestCases");
        public static string sTestProjectCsharpcsproj = Path.Combine(sProjectPath, "TestFW", "TestProjectCsharp", "TestProjectCsharp.csproj");
        public static string sKeyWordsFolder = Path.Combine(sProjectPath, "TestFW", "TestProjectCsharp", "Keywords");
        public static string sTestProjectFolder = drInfoRoot.Parent.FullName;
        public static string sTestProjectDLL = Path.Combine(sTestProjectFolder, "TestProject", "TestProjectCSharp.dll");
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
            Logger.Info($"sTestProjectFolder: {sTestProjectFolder}");
            Logger.Info($"sTestProjectDLL: {sTestProjectDLL}");
            Logger.Info($"sKeyWordsFolder: {sKeyWordsFolder}");
            Logger.Info($"sKeywordListFile: {sKeywordListFile}");
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
    }
}