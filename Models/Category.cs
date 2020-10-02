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
        [BsonRequired]
        [JsonRequired]
        public string CategoryName { get; set; } 
        [BsonElement("description")]
        public string Description { get; set; }
        [BsonElement("_id_TestSuites")]
        public List<string> _id_TestSuites { get; set; }
    }
}