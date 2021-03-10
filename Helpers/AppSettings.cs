using System.IO;

namespace ATTM_API.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string TestProjectCsharp { get; set; }
        public string DefaultTestCaseTimeOutInMinus { get; set; }
        public string MaximumTestCaseTimeOutInMinus { get; set; }
        public string SupportBrowser { get; set; }

    }
}