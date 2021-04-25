namespace ATTM_API.Models
{
    public interface IATTMDatabaseSettings
    {
        string CategoriesCollectionName { get; set; }
        string TestSuitesCollectionName { get; set; }
        string TestGroupsCollectionName { get; set; }
        string TestCasesCollectionName { get; set; }
        string TestEnvironmentsCollectionName { get; set; }
        string UsersCollectionName { get; set; }
        string TestClientsCollectionName { get; set; }
        string KeywordsCollectionName { get; set; }
        string TestAUTsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
    public class ATTMDatabaseSettings : IATTMDatabaseSettings
    {
        public string CategoriesCollectionName { get; set; }

        public string TestSuitesCollectionName { get; set; }
        public string TestGroupsCollectionName { get; set; }
        public string TestCasesCollectionName { get; set; }
        public string TestEnvironmentsCollectionName { get; set; }
        public string UsersCollectionName { get; set; }
        public string TestClientsCollectionName { get; set; }
        public string KeywordsCollectionName { get; set; }
        public string TestAUTsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}