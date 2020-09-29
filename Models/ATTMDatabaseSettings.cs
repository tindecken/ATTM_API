namespace ATTM_API.Models
{
    public interface IATTMDatabaseSettings
    {
        string CategoriesCollectionName { get; set; }
        string UsersCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
    public class ATTMDatabaseSettings : IATTMDatabaseSettings
    {
        public string CategoriesCollectionName { get; set; }
        public string UsersCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}