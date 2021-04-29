using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    [BsonIgnoreExtraElements]
    public class DevQueue
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string TestCaseId { get; set; }
        public string TestCaseCodeName { get; set; }
        public string TestCaseName { get; set; }
        public string TestCaseFullName { get; set; }
        public string QueueStatus { get; set; }
        public string QueueType { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime RunAt { get; set; }
        public string ClientName { get; set; }
        public bool IsHighPriority { get; set; }
        public string Note { get; set; }
    }
}