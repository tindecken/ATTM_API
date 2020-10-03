using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    public class Category
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("name")]
        [JsonRequired]
        public string CategoryName { get; set; } 
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;
        [BsonElement("testsuites")]
        public List<string> TestSuites { get; set; } = new List<string>();
    }
}