using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ATTM_API.Models.Entities
{
    public class TestEnvCloneData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string NewName { get; set; }
        public string NewDescription { get; set; }
        public string CloneBy { get; set; }
    }
}
