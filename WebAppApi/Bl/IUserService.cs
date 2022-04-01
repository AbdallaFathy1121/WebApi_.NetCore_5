using Domains;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebAppApi.Bl
{
    public interface IUserService
    {
        Task<UserManagerResponse> RegisterUserAsync(RegisterViewModel model);
        Task<UserManagerResponse> LoginUserAsync(LoginViewModel model);
    }

    public class UserService : IUserService 
    {
        // Use UserManager && IConfigration
        private UserManager<IdentityUser> _userManager;
        private IConfiguration _configration;
        public UserService(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configration = configuration;
        }
    
        // Method Register User
        public async Task<UserManagerResponse> RegisterUserAsync(RegisterViewModel model)
        {
            if (model == null)
                throw new NullReferenceException("Register Model is null");

            // Check Password Confirmed
            if (model.Password != model.ConfirmPassword)
                return new UserManagerResponse
                {
                    Message = "Confirm Password dosn't Match password",
                    IsSuccess = false
                };

            var identityUser = new IdentityUser
            {
                Email = model.Email,
                UserName = model.Email
            };

            var result = await _userManager.CreateAsync(identityUser, model.Password);

            if (result.Succeeded)
            {
                return new UserManagerResponse
                {
                    Message = "User Created Successfully!",
                    IsSuccess = true
                };
            }
            else
            {
                return new UserManagerResponse
                {
                    Message = "User did not Create",
                    IsSuccess = false,
                    Errors = result.Errors.Select(x => x.Description)
                };
            }

        }

        // Method Login User
        public async Task<UserManagerResponse> LoginUserAsync(LoginViewModel model)
        {
            // Find User By Email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new UserManagerResponse
                {
                    Message = "There is no user with that Email Address",
                    IsSuccess = false
                };
            }

            // Check Valid Password 
            var result = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!result)
            {
                return new UserManagerResponse
                {
                    Message = "InValid Password",
                    IsSuccess = false
                };
            }

            // Callback functhon CreateJwtToken
            var jwtSecurity = await CreateJwtToken(user);

            return new UserManagerResponse
            {
                Message = new JwtSecurityTokenHandler().WriteToken(jwtSecurity),
                IsSuccess = true,
                ExpirDate = jwtSecurity.ValidTo
            };
        }


        // Function Create JwtToken
        private async Task<JwtSecurityToken> CreateJwtToken(IdentityUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configration["AuthSettings:Key"]));

            var token = new JwtSecurityToken(
                    issuer: _configration["AuthSettings:Issuer"],
                    audience: _configration["AuthSettings:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddDays(30),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

    }
}
