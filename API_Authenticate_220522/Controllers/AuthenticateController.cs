using API_Authenticate_220522.Authentication;
using API_Authenticate_220522.Models;

using Google.Apis.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API_Authenticate_220522.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        IAuthenticator authenticator;
        UserManager<ApplicationUser> userManager;
        SignInManager<ApplicationUser> signInManager;
        IConfiguration configuration;

        public AuthenticateController(IAuthenticator authenticator, UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            this.authenticator = authenticator;
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
        }

        [HttpPost("GetToken",Name = "GetToken")]
        [Authorize]
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

        [HttpGet("Login", Name = "Login")]
        [Authorize]
        public IActionResult SignInWithGoogle()
        {
            var authenticationProperties = signInManager.ConfigureExternalAuthenticationProperties("Google", Url.Action(nameof(HandleExternalLogin)));
            return Challenge(authenticationProperties, GoogleDefaults.AuthenticationScheme);
        }


        //Returns the User Details upon Successful Logon

        public async Task<IActionResult> HandleExternalLogin()
        {
            var info = await signInManager.GetExternalLoginInfoAsync();
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (!result.Succeeded) //user does not exist yet
            {
                
                var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(newUser);
                if (!createResult.Succeeded)
                    throw new Exception(createResult.Errors.Select(e => e.Description).Aggregate((errors, error) => $"{errors}, {error}"));

                await userManager.AddLoginAsync(newUser, info);
                var newUserClaims = info.Principal.Claims.Append(new Claim("userId", newUser.Id));
                await userManager.AddClaimsAsync(newUser, newUserClaims);
                await signInManager.SignInAsync(newUser, isPersistent: false);
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            }

            var result2 = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            var claims = result2.Principal.Identities
                .FirstOrDefault().Claims.Select(claim => new
                {
                    claim.Issuer,
                    claim.OriginalIssuer,
                    claim.Type,
                    claim.Value
                });

            return Ok(claims);

        }

        [HttpGet("welcome",Name = "welcome")]
        public IActionResult Welcome()
        {
            return Content("Welcome to authentication by Google");
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
