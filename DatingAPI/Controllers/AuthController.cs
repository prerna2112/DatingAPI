using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingAPI.Data;
using DatingAPI.Dtos;
using DatingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IAuthRespository _repo;
        private IConfiguration _config;
        private IMapper _mapper;
        public AuthController(IAuthRespository repo ,IConfiguration config, IMapper mapper)
        {
            _repo = repo;
            _config = config;
            _mapper = mapper;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegister)
        {
            //Vaidate request
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);  
            userForRegister.Username = userForRegister.Username.ToLower();
            if (await _repo.UserExists(userForRegister.Username))
                return BadRequest("username already exists!!");
            var userToCreate = _mapper.Map<User>(userForRegister);
           
            var createdUser = await _repo.Register(userToCreate, userForRegister.Password);
            var userToReturn = _mapper.Map<UerForDetailedDto>(createdUser);
            //return CreatedAtRoute()
            return CreatedAtRoute("GetUser", new {Controller = "Users", id= createdUser.Id}, userToReturn);
            
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLogin)
        {
            //throw new Exception("Application!");
            var userFormRepo = await _repo.Login(userForLogin.UserName.ToLower(), userForLogin.Password);
            //first we are making sure that we have a user in our database with these username and password.

            if (userFormRepo == null)
            {
                return Unauthorized();
            }
            //Token has 2 claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,userFormRepo.Id.ToString()),
                new Claim(ClaimTypes.Name,userFormRepo.Username)
            };
            //creating a security key 
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var user = _mapper.Map<UserForListDto>(userFormRepo);
            return Ok(new
                    {
                        token = tokenHandler.WriteToken(token),
                        user 
                    }
             );
            
        }
        
    }
}
