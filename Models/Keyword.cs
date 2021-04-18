using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    [BsonIgnoreExtraElements]
    public class Keyword
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public List<TestParam> Params;
        public DateTime CreatedDate;
        public List<string> ImageDescriptionIds;
    }
}