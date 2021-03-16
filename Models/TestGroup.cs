using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    [BsonIgnoreExtraElements]
    public class TestGroup
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("Name")]
        [JsonRequired]
        [JsonProperty("Name")]
        public string Name { get; set; } 
        public string CodeName { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> TestCaseIds { get; set; } = new List<string>();
    }
}