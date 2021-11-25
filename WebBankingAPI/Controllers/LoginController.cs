using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using WebBankingAPI.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace WebBankingAPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [HttpPost("")]
        public ActionResult Login([FromBody] User credentials)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                User candidate = model.Users.FirstOrDefault(q => q.Username == credentials.Username && q.Password == credentials.Password);
                if (candidate == null) return Ok("Username o Password errati");

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor//descrive il token, ma non lo crea
                {
                    SigningCredentials = new SigningCredentials(SecurityKeyGenerator.GetSecurityKey(), SecurityAlgorithms.HmacSha256),
                    Expires = DateTime.UtcNow.AddDays(1),
                    Subject = new ClaimsIdentity(
                     new Claim[]
                     {
                         new Claim("Id", candidate.Id.ToString()),
                         new Claim("Username", candidate.Username.ToString())
                     }
                      )
                };
                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);//oggetto di classe security token
                return Ok(tokenHandler.WriteToken(token));//restituisce una stringa, il nostro token
            }
        }
        [Authorize]//protegge i metodi riservati 
        [HttpPost("/Logout")]
        public ActionResult Logout()
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var ID = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Id").Value;
                var name = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Username").Value;
                User soggetto = model.Users.FirstOrDefault(q => q.Id.ToString() == ID && q.Username == name);

                soggetto.LastLogout = DateTime.Now;
                model.SaveChanges();
                return Ok();
            }
        }
    }
}
