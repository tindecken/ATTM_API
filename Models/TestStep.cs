using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ATTM_API.Models
{
    public class TestStep
    {
        public string UUID { get; set; }
        public string TestAUT { get; set; }
        public string Keyword { get; set; }
        public string Name { get; set; }
        public string Feature { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<TestParam> Params { get; set; } = new List<TestParam>();
        public bool isDisabled { get; set; } = false;
        public bool isComment { get; set; } = false;

    }
}