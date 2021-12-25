namespace ATTM_API.Models
{
    public interface IATTMDatabaseSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string CategoriesCollectionName { get; set; }
        string TestSuitesCollectionName { get; set; }
        string TestGroupsCollectionName { get; set; }
        string TestCasesCollectionName { get; set; }
        string TestCaseHistoriesCollectionName { get; set; }
        string TestEnvironmentsCollectionName { get; set; }
        string UsersCollectionName { get; set; }
        string TestClientsCollectionName { get; set; }
        string KeywordsCollectionName { get; set; }
        string TestAUTsCollectionName { get; set; }
        string DevQueuesCollectionName { get; set; }
        string DevRunRecordsCollectionName { get; set; }
        string RegressionsCollectionName { get; set; }
        string RegressionTestsCollectionName { get; set; }
        string RegressionRunRecordsCollectionName { get; set; }
        string SettingsCollectionName { get; set; }
        
    }
    public class ATTMDatabaseSettings : IATTMDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string CategoriesCollectionName { get; set; }

        public string TestSuitesCollectionName { get; set; }
        public string TestGroupsCollectionName { get; set; }
        public string TestCasesCollectionName { get; set; }
        public string TestCaseHistoriesCollectionName { get; set; }
        public string TestEnvironmentsCollectionName { get; set; }
        public string UsersCollectionName { get; set; }
        public string TestClientsCollectionName { get; set; }
        public string KeywordsCollectionName { get; set; }
        public string TestAUTsCollectionName { get; set; }
        public string DevQueuesCollectionName { get; set; }
        public string DevRunRecordsCollectionName { get; set; }
        public string RegressionsCollectionName { get; set; }
        public string RegressionTestsCollectionName { get; set; }
        public string RegressionRunRecordsCollectionName { get; set; }
        public string SettingsCollectionName { get; set; }
        
    }
}