using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    [BsonIgnoreExtraElements]
    public class KeywordFeature
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Keyword> Keywords;
    }
}