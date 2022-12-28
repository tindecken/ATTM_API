using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ATTM_API.Models;
using ATTM_API.Helpers;
using CommonModels;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using ATTM_API.Models.Entities;
using System.Text.Unicode;
using Microsoft.AspNetCore.Http;

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

            var user = _users.Find<User>(user => user.Username == model.Username).FirstOrDefault();
            // return null if user not found
            if (user == null) return null;

            byte[] data = Convert.FromBase64String(user.Password);
            MD5 md5 = MD5.Create();
            var tripDes = TripleDES.Create();
            tripDes.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(_secret));
            tripDes.Mode = CipherMode.ECB;
            
            ICryptoTransform transform = tripDes.CreateDecryptor();
            byte[] results = transform.TransformFinalBlock(data, 0, data.Length);
            var decryptedPassword = Encoding.UTF8.GetString(results);

            if (decryptedPassword.Equals(model.Password))
            {
                // authentication successful so generate jwt token
                var token = generateJwtToken(user);

                return new AuthenticateResponse(user, token);
            }
            else
            {
                return null;
            }
        }

        public async Task<JObject> ChangePassword(ChangePasswordData changePasswordData)
        {
            // Get regression test
            JObject result = new JObject();

            // Check New Password and Confirm Password is the same
            if (!changePasswordData.NewPassword.Equals(changePasswordData.ConfirmNewPassword))
            {
                result.Add("result", "error");
                result.Add("message", "New password and confirm password are not the same!");
                return result;
            }

            // Check if CurrentPassword is the same with new Password
            if (changePasswordData.CurrentPassword.Equals(changePasswordData.NewPassword))
            {
                result.Add("result", "error");
                result.Add("message", "Current password and new password are the same!");
                return result;
            }

            var user = _users.Find<User>(user => user.Id == changePasswordData.UserId).FirstOrDefault();
            if (user == null)
            {
                result.Add("result", "error");
                result.Add("message", "User not found!");
                return result;
            }

            //Check current password is correct or not
            byte[] data = Convert.FromBase64String(user.Password);
            MD5 md5 = MD5.Create();
            var tripDes = TripleDES.Create();
            tripDes.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(_secret));
            tripDes.Mode = CipherMode.ECB;

            ICryptoTransform transform = tripDes.CreateDecryptor();
            byte[] results = transform.TransformFinalBlock(data, 0, data.Length);
            var decryptedPassword = Encoding.UTF8.GetString(results);

            if (!decryptedPassword.Equals(changePasswordData.CurrentPassword))
            {
                result.Add("result", "error");
                result.Add("message", "Current password is incorrect!");
                return result;
            }

            // Change password
            // Encrypt new password
            var tripDes2 = TripleDES.Create();
            tripDes2.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(_secret));
            tripDes2.Mode = CipherMode.ECB;
            tripDes2.Padding = PaddingMode.PKCS7;
            byte[] DataToEncrypt = UTF8Encoding.UTF8.GetBytes(changePasswordData.NewPassword);
            ICryptoTransform cTransform = tripDes2.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);

            var filter = Builders<User>.Filter.Eq("Id", changePasswordData.UserId);
            var update = Builders<User>.Update.Set("Password", Convert.ToBase64String(resultArray));
            await _users.UpdateOneAsync(filter, update);
            

            result.Add("result", "success");
            result.Add("data", null);
            result.Add("message", $"Change password successful.");
            return result;
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