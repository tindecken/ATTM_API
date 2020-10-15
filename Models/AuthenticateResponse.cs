using ATTM_API.Models;

namespace ATTM_API.Models
{
    public class AuthenticateResponse
    {
        public string id { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public string username { get; set; }
        public string token { get; set; }


        public AuthenticateResponse(User user, string resToken)
        {
            id = user.Id;
            email = user.Email;
            role = user.Role;
            username = user.Username;
            token = resToken;
        }
    }
}