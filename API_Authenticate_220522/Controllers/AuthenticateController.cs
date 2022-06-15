using API_Authenticate_220522.Authentication;
using API_Authenticate_220522.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Authenticate_220522.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        IAuthenticator authenticator;

        public AuthenticateController(IAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        [HttpPost("GetToken",Name = "GetToken")]
        public IActionResult GetToken([FromBody] User user)
        {
            var token = authenticator.Generate_JWT(user.name);

            if (token != null)
            {
                var refresh_token = authenticator.Generate_Refresh_Token(user.name);

                if (token != null && refresh_token != null)
                    return Ok(new RefreshToken { jwt_token = token.token, refresh_token = refresh_token.token,user_name = user.name});
            }

            return BadRequest();
        }

        [HttpPost("CheckLogin",Name ="CheckLogin")]
        public IActionResult CheckLogin([FromBody] User user)
        {
            if (user.name == "Arjun" && user.password == "123456")
                return Ok();

            return BadRequest();
        }

        [HttpPost("GetRefreshToken",Name = "GetRefreshToken")]
        public IActionResult GetRefreshToken([FromBody] RefreshToken tokenData)
        {

            var username = tokenData.user_name;
            var token = tokenData.jwt_token;
            var refresh_token = tokenData.refresh_token;
            var error = "";

            bool res1 = authenticator.Validate_JWT(token, username);
            bool res2 = authenticator.Validate_Refresh_Token(refresh_token, username);
            authenticator.Revoke_Refresh_Token(refresh_token, username);

            if (res1 && res2)
            {
                var new_token = authenticator.Generate_JWT(username);
                var new_refresh_token = authenticator.Generate_Refresh_Token(username);
                return Ok(new RefreshToken { jwt_token = new_token.token, refresh_token = new_refresh_token.token, user_name = username });
            }

            if (!res1) error = "JWT token is invalid. ";
            if (!res2) error = error + "Refresh token is invalid. ";

            return BadRequest(error);
        }
    }
}
