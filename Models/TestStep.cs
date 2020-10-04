using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    public class TestStep
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("name")]
        [JsonRequired]
        [JsonProperty("Name")]
        public string Name { get; set; } 
        public string Description { get; set; } = string.Empty;
        public int MyProperty { get; set; }

    }
}