using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TestWebAPI.Authentication;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace TestWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly UserManager<ApplicationUser> roleManager;
        private readonly IConfiguration _configuration;


        private static readonly string[] Summaries = new[]
      {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public AuthenticationController(UserManager<ApplicationUser> userManager, UserManager<ApplicationUser> roleManager, IConfiguration _configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._configuration = _configuration;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var username = await userManager.FindByNameAsync(model.name);
                if (username == null)
                {
                    var result = await userManager.CreateAsync(new ApplicationUser() { UserName = model.name, Email = model.email }, model.password);
                    if (result.Succeeded)
                    {
                        return Ok(await userManager.FindByNameAsync(model.name));
                    }
                    else
                    {
                        return BadRequest(result.Errors);
                    }
                }
                else
                {
                    BadRequest("user already exist");
                }
               
            }
            return BadRequest("Invalid model data");
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(loginModel.email);
                
                if (user != null && await userManager.CheckPasswordAsync(user, loginModel.password))
                {
                    var userRoles = await roleManager.GetRolesAsync(user);
                    var userClaims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Email , user.Email)
                    };

                    foreach (var role in userRoles)
                    {
                        userClaims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                    }

                    var tokendescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(userClaims),
                        Expires = DateTime.UtcNow.AddHours(2),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MyKeyFromAppConfig")),
                         SecurityAlgorithms.HmacSha256)
                    };
                    var tokenHandeler = new JwtSecurityTokenHandler();
                    var token = tokenHandeler.WriteToken(tokenHandeler.CreateToken(tokendescriptor));
                    return Ok(token);

                    //var token = new JwtSecurityToken(null, null, userClaims, expires: DateTime.Now.AddHours(2),
                    //    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MyKeyFromAppConfig")),
                    //    SecurityAlgorithms.HmacSha256));
                    //return Ok (new JwtSecurityTokenHandler().WriteToken(token));


                }
                else
                {
                    return BadRequest("user is not registered");
                }
            }
            else
            {
                ModelState.AddModelError("invalid model", "Invalid model data");
            }

            return BadRequest();

        }



        [HttpGet]
        [Authorize]
        public IEnumerable<WeatherForecast> testGet()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

    }
}
