using API_Authenticate_220522.Models;

namespace API_Authenticate_220522.Authentication
{
    public interface IAuthenticator
    {
        Token Jwt_Token { get; set; }
        Token Refresh_Token { get; set; }

        Token Generate_JWT(string username);
        Token Generate_Refresh_Token(string username);
        bool Validate_JWT(string token, string username);
        bool Validate_Refresh_Token(string token, string username);
        bool Revoke_Refresh_Token(string token, string username);
    }
}