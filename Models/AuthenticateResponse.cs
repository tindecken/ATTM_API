using ATTM_API.Models;
using CommonModels;

namespace ATTM_API.Models
{
    public class AuthenticateResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }


        public AuthenticateResponse(User user, string resToken)
        {
            Id = user.Id;
            Email = user.Email;
            Role = user.Role;
            Username = user.Username;
            Token = resToken;
        }
    }
}