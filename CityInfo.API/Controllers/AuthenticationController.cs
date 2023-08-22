using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CityInfo.API.Controllers;

[Route("api/authentication")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthenticationController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public class AuthenticationRequestBody
    {
        public string? UserName { get; set; }
        
        public string? Password { get; set; }
    }
    
    [HttpPost("authenticate")]
    public ActionResult<string> Authenticate(AuthenticationRequestBody authenticationRequestBody)
    {
        var user = ValidateUserCredentials(
            authenticationRequestBody.UserName, 
            authenticationRequestBody.Password);

        if (user == null)
        {
            return Unauthorized();
        }
        
        var secretKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"]));
        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var claimsForToken = new List<Claim>();
        claimsForToken.Add(new Claim("sub", user.UserId.ToString()));
        claimsForToken.Add(new Claim("given_name", user.FirstName));
        claimsForToken.Add(new Claim("family_name", user.LastName));
        claimsForToken.Add(new Claim("city", user.City));
        
        var jwtSecurityToken = new JwtSecurityToken(
            _configuration["Authentication:Issuer"],
            _configuration["Authentication:Audience"],
            claimsForToken,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            signingCredentials);
        
        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        return Ok(token);
    }

    private CityInfoUser ValidateUserCredentials(string? userName, string? password)
    {
        // we dont have a database, so we will hardcode the user credentials
        return new CityInfoUser(
            1,
            userName ?? "",
            "Kevin",
            "Dockx",
            "Antwerp");
    }
    
    private class CityInfoUser
    {
        public int UserId { get; set; }
        
        public string UserName { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string City { get; set; }

        public CityInfoUser(int userId, string userName, string firstName, string lastName, string city)
        {
            UserId = userId;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            City = city;
        }
    }
}