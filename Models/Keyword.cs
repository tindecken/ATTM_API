using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    public class Keyword
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime refreshDate { get; set; } = DateTime.UtcNow;
        [BsonExtraElements]
        public BsonDocument categories { get; set; }
    }
}