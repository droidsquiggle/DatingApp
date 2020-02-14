using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    // the api controller extends base controller which defines the MVC portions
    // we're not using the V part but the api controller is stil needed for web calls
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            // config needed to get access for AppSettings.json for login function
            _config = config;
            _mapper = mapper;
            _repo = repo;

        }

        // connect register part of controller
        // input comes as json so we use a DTO (data transfer object) 
        // DTO's are basically plain objects to define what a json input variables are
        // this automatically infers from controller that this input is [FromBody] because of [ApiController]
        // if [ApiController] doesnt exist we will need [FromBody] and the dto wont register the model state
        // will need to add in an infstatement to makesure the modelstate is valid 
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            // validate request

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
            if (await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("Username already exists");

            var userToCreate = new User
            {
                Username = userForRegisterDto.Username
            };

            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            // to be used later
            // return CreatedAtRoute();

            // hardcoded result that CreatedAtRoute normally does
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {

            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.password);

            if(userFromRepo == null)
                return Unauthorized();

            // create JWT token which includes username, when they cleared register, and how long they are allowed
            // 2 claims in the token, userid and username
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            // once claim is created, we need to sign the key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
        
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // create token using the claims from above
            // we give it an expire date of 24 hours (just for training purposes)
            var tokenDescriptior = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            // introduce jwt and create new handler
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptior);

            // we will mape the user from repo to the UserForListDto and include that in the response
            // this way our local storage at the highest level of the application will have user info
            // used to store user main photo in the nav bar
            // could create a new dto specificallyfor this but being lazy
            var user = _mapper.Map<UserForListDto>(userFromRepo);
            
            // write token into response we send back to clients with the token handler
            // the returned token json to the browser you can decode it in on:
            // https://jwt.io 
            return Ok(new {
                token = tokenHandler.WriteToken(token),
                user
            });
            
        }
    }
}