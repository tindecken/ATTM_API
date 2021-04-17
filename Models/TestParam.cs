using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    public class TestParam
    {
        public string Name { get; set; } 
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ExampleValue { get; set; } = string.Empty;
        public string TestNodePath { get; set; } = string.Empty;
    }
}