using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
namespace ATTM_API.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("id")]
        public string Id { get; set; }
        [BsonElement("username")]
        public string Username { get; set; }
        [BsonElement("role")]
        public string Role { get; set; }
        [BsonElement("email")]
        public string Email { get; set; }
        [JsonIgnore]
        [BsonElement("password")]
        public string Password { get; set; }
    }
}