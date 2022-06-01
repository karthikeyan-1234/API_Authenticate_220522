using API_Authenticate_220522.Contexts;
using API_Authenticate_220522.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API_Authenticate_220522.Authentication
{
    public class Authenticator : IAuthenticator
    {
        public Token Jwt_Token { get; set; }
        public Token Refresh_Token { get; set; }
        private string secret_key { get; set; }
        private TokenDBContext db;

        public Authenticator(TokenDBContext db)
        {
            this.db = db;
            this.secret_key = "ThisIsASuperSecureKeyThatCannotBeGuessedByAnybodyEasilyForEver";
        }

        public Token Generate_JWT(string username)
        {
            try
            {
                var token_handler = new JwtSecurityTokenHandler();
                Jwt_Token = new Token();
                SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret_key));
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,username)
                };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims);
                var expiry = DateTime.Now.AddSeconds(10);

                var token_descriptor = new SecurityTokenDescriptor
                {
                    Expires = expiry,
                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
                    Subject = claimsIdentity
                };

                var token = token_handler.CreateToken(token_descriptor);
                Jwt_Token.token = token_handler.WriteToken(token);
                Jwt_Token.expires_at = expiry;
                Jwt_Token.user_name = username;
                return Jwt_Token;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public Token Generate_Refresh_Token(string username)
        {
            try
            {
                var rand_no = new byte[32];
                var rand_gen = RandomNumberGenerator.Create();
                rand_gen.GetBytes(rand_no);
                var ref_token = Convert.ToBase64String(rand_no);

                var result = db.Tokens.Where(t => t.is_expired == false);
                foreach (var _token in result)
                    _token.is_expired = true;

                db.SaveChanges();

                Token token = new Token
                {
                    token = ref_token,
                    expires_at = DateTime.Now.AddSeconds(60),
                    user_name = username
                };
                db.Tokens.Add(token);
                db.SaveChanges();
                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public bool Validate_JWT(string token, string username)
        {
            try
            {
                var token_params = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret_key)),
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false,
                    ValidateLifetime = false
                };

                var token_handler = new JwtSecurityTokenHandler();
                SecurityToken out_token;
                var principal = token_handler.ValidateToken(token, token_params, out out_token);
                JwtSecurityToken jwtSecurityToken = out_token as JwtSecurityToken;

                if (principal.FindFirst(ClaimTypes.Name)?.Value == username && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool Validate_Refresh_Token(string token, string username)
        {
            try
            {
                var current_time = DateTime.Now;
                var refToken = db.Tokens.Where(t => t.token == token && t.user_name == username &&
                t.is_expired == false && t.expires_at >= current_time).FirstOrDefault();

                if(refToken != null)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool Revoke_Refresh_Token(string token,string username)
        {
            try
            {
                var token_ = db.Tokens.Where(t => t.token == token && t.user_name == username).FirstOrDefault();

                if (token_ != null)
                {
                    token_.is_expired = true;
                    db.SaveChanges();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        } 
    }
}
