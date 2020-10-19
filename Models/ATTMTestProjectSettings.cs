using System.IO;

namespace ATTM_API.Models
{
    public class ATTMTestProjectSetting
    {
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
        public static string sProjectPath = drInfoRoot.Parent.Parent.FullName;
        public static string sTestCasesFolder = Path.Combine(sProjectPath, "TestProjectCsharp", "TestCases");
        public static string sTestProjectCsharpcsproj = Path.Combine(sProjectPath, "TestProjectCsharp", "TestProjectCsharp.csproj");
        public static string sKeyWordsFolder = Path.Combine(sProjectPath, "TestProjectCsharp", "Keywords");
        public static string sTestProjectFolder = drInfoRoot.Parent.FullName;
        public static string sTestProjectDLL = Path.Combine(sTestProjectFolder, "TestProject", "TestProjectCSharp.dll");
        public static string sPatternStartSummary = "/// <summary>";
        public static string sPatternEndSummary = "/// </summary>";
        public static string sPatternParam = @"/// <param.*</param>$";
        public static string sPatternGroupKeyword = @"public void (?<KeywordName>.+)\((?<Params>.+)\).*";
        public static string sPatternGroupParam = "<param name=\"(?<ParamName>.+)\">(?<ParamDescription>.+)Exp:(?<ParamExampleValue>.+)</param>";
    }
}