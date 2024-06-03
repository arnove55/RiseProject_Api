﻿using AngularApi.Context;
using AngularApi.Helpers;
using AngularApi.Models;
using AngularApi.Models.DTO;
using AngularApi.Repository;
using AngularApi.UtilityServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AngularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepo _userRepo;
        private readonly AppDbContext _authContext;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        public UserController(AppDbContext appDbContext,IUserRepo userRepo, IConfiguration configuration, IEmailService emailService) 
        {
            _authContext=appDbContext;
            _userRepo =userRepo;
            _configuration = configuration;
            _emailService = emailService;   
        }
        [HttpPost("authenticate")]
        public async Task<IActionResult>Authenticate([FromBody]User userObj)
        {
            if(userObj == null)
            {
                return BadRequest();
            }
            
            
            var users = await _userRepo.GetUserByUsernameAsync(userObj.Username);
            if (users == null)
            {
                return NotFound(new {Message="user not found"});
            }
            if (!PasswordHash.VerifyPassword(userObj.Password, users.Password))
            {
                return BadRequest(new { Message = "Incorrect Password" });
            }
            users.Token=CreateJwtToken(users);
            await _userRepo.UpdateUserAsync(users);
            return Ok(new 
            {
                Token=users.Token,
                Message="login Success!!"
            });

        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]User userObj)
        {
            if (userObj == null)
            {
                return BadRequest();
            }
            //check username
            if (await _userRepo.CheckUsernameExistsAsync(userObj.Username))
            {
                return BadRequest(new { Message = "Username already exists" });
            }

            //check email
            if (await _userRepo.CheckEmailExistsAsync(userObj.Email))
            {
                return BadRequest(new { Message = "Email already exits" });
            }
            //check password strength
            var pass = CheckPassword(userObj.Password);
            if(!string.IsNullOrEmpty(pass))
            {
                return BadRequest(new {Messgae=pass.ToString()});
            }


            userObj.Password =PasswordHash.HashPassword(userObj.Password);
            //userObj.Password=PasswordHash.VerifyPassword(userObj.Password);
            userObj.Token = "";
            await _userRepo.AddUserAsync(userObj);
            return Ok(new {Message="You have registerd successfully!!"});
        }
        private string CheckPassword(string password)
        {
            StringBuilder sb = new StringBuilder();
            if (password.Length < 8)
            {
                sb.Append("Minimum password length should be 8" + Environment.NewLine);
            }
            if(!(Regex.IsMatch(password,"[a-z]") && Regex.IsMatch(password,"[A-Z]")
                && Regex.IsMatch(password, "[0-9]")))
            {
                sb.Append("Password should contain one big letter and a number" + Environment.NewLine);
            }
            if(!Regex.IsMatch(password,"[<,>@!#$%^&*()_+{}?/:;|]" ))
            {
                sb.Append("Password should contain special character" + Environment.NewLine);
            }
            return sb.ToString();
        }
        private string CreateJwtToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryveryveryScerete.....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name,$"{user.Username}"),
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials,
            };
            var token=jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);

        }
        [HttpGet]
        public async Task<ActionResult<User>> GetAllUsers()
        {
            return Ok(await _authContext.Users.ToListAsync());
        }
        [HttpPost("send-reset-email/{email}")]
        public async Task<IActionResult> SendEmail(string email)
        {
            var user= await _authContext.Users.FirstOrDefaultAsync(a=>a.Email==email);
            if (user is null) 
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Email Dosen't exists"

                });
            }
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var emailToken=Convert.ToBase64String(tokenBytes);
            user.ResetPasswordToken = emailToken;
            user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
            string from = _configuration["EmailSettings:From"];
            var emailModel=new Emailmodel(email,"Reset Password",EmailBodyClass.EmailBody(email,emailToken));
            _emailService.EndEmail(emailModel);
            _authContext.Entry(user).State= EntityState.Modified;
            await _authContext.SaveChangesAsync();
            await _authContext.AddRangeAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Email sent"
            });

        }
        [HttpPost("forgot")]
        public async Task<IActionResult> Resetpassword(ResetPasswordDTO resetPasswordDTO)
        {
            var newToken = resetPasswordDTO.EmailToken.Replace(" ", "+");
            var user = await _authContext.Users.AsNoTracking().FirstOrDefaultAsync(a => a.Email == resetPasswordDTO.Email);
            if (user is null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "User Dosen't exists"

                });
            }
            var tokeCode = user.ResetPasswordToken;
            DateTime emailTokenExpiry= user.ResetPasswordExpiry;
            if(tokeCode != resetPasswordDTO.EmailToken || emailTokenExpiry<DateTime.Now)
            {
                return BadRequest(
                    new
                    {
                        StatusCode = 400,
                        Message = "Invalid Reset Link"
                    });
            }
            user.Password = PasswordHash.HashPassword(resetPasswordDTO.NewPassword);
            _authContext.Entry(user).State=EntityState.Modified;
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Password Reset Successfull"
            });
        }

    }
}
