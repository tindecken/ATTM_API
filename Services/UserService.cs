using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using ATTM_API.Models;
using ATTM_API.Helpers;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace ATTM_API.Services
{
    public class UserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private readonly IMongoCollection<User> _users;
        private readonly ATTMAppSettings _appSettings;
        private readonly string _secret;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        public UserService(IATTMAppSettings appSettings, IATTMDatabaseSettings dbSettings)
        {
            _secret = appSettings.Secret;
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);

            _users = database.GetCollection<User>(dbSettings.UsersCollectionName);
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            Logger.Info($"User: {JsonConvert.SerializeObject(model)}");
            var user = _users.Find<User>(user => user.Username == model.Username && user.Password == model.Password).FirstOrDefault();

            // return null if user not found
            if (user == null) return null;

            // authentication successful so generate jwt token
            var token = generateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }

        public List<User> GetAll() =>
            _users.Find(user => true).ToList();

        public User GetById(string id)
        {
            return _users.Find<User>(u => u.Id == id).FirstOrDefault();
        }

        // helper methods

        private string generateJwtToken(User user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}