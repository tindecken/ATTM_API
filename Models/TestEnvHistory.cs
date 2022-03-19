using System;
using System.Collections.Generic;
using ATTM_API.Models.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    [BsonIgnoreExtraElements]
    public class TestEnvHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public UpdateTestEnvData UpdateTestEnvData { get; set; }
        public TestEnv TestEnv { get; set; }
    }
}