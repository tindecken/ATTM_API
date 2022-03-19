namespace ATTM_API.Models
{
    public interface IATTMAppSettings
    {
        string Secret { get; set; }
        string TestProject { get; set; }
        string BuiltSource { get; set; }
        string PSToolsFolder { get; set; }
        string DefaultTestCaseTimeOutInMinus { get; set; }
        string MaximumTestCaseTimeOutInMinus { get; set; }
        string SupportedBrowsers { get; set; }
    }
    public class ATTMAppSettings : IATTMAppSettings
    {
        public string Secret { get; set; }
        public string TestProject { get; set; }
        public string BuiltSource { get; set; }
        public string PSToolsFolder { get; set; }
        public string DefaultTestCaseTimeOutInMinus { get; set; }
        public string MaximumTestCaseTimeOutInMinus { get; set; }
        public string SupportedBrowsers { get; set; }
        
        
    }
}