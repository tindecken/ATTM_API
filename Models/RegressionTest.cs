using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    [BsonIgnoreExtraElements]
    public class RegressionTest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string RegressionId { get; set; }
        public string Release { get; set; }
        public string Build { get; set; }
        public string TestCaseName { get; set; } 
        public string TestCaseFullName { get; set; }
        public string CategoryName { get; set; }
        public string TestSuiteName { get; set; }
        public string TestGroupName { get; set; }
        public bool IsHighPriority { get; set; }
        public int QueueId { get; set; }
        public string Owner { get; set; }
        public string Status { get; set; }
        public string RegressionRunRecordId { get; set; }
        public string AnalyzeBy { get; set; }
        public string Issue { get; set; }
        public string Comment { get; set; }
    }
}