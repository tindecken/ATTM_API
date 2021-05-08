using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    [BsonIgnoreExtraElements]
    public class RegressionQueue : DevQueue
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public new string Id { get; set; }
        public string ReleaseId { get; set; }
    }
}