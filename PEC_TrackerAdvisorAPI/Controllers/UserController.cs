using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PEC_TrackerAdvisorAPI.Context;
using PEC_TrackerAdvisorAPI.DTOs;
using PEC_TrackerAdvisorAPI.Helpers;
using PEC_TrackerAdvisorAPI.Models;
using PEC_TrackerAdvisorAPI.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PEC_TrackerAdvisorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class UserController : ControllerBase
    {
        private readonly AppDbContext _authContext;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserController(AppDbContext authContext, IConfiguration configuration, IEmailService emailService)
        {
            _authContext = authContext;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync([FromBody] UserLoginDTO userObj)
        {
            if (userObj == null)
                return BadRequest(new { message = "Username or password is incorrect!" });

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Username == userObj.Username);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect!" });

            if (!PasswordHasher.Verify(userObj.Password, user.Password))
                return BadRequest(new { message = "Username or password is incorrect!" });

            user.Token = CreateJwtToken(user);
            var newAceessToken = user.Token;
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(5);
            await _authContext.SaveChangesAsync();

            return Ok(new TokenApiDTO
            {
                AccessToken = newAceessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] UserRegisterDTO userObj)
        {
            if (userObj == null)
                return BadRequest(new { message = "User object is null" });
            if(await UserNameExistsAsync(userObj.Username))
                return BadRequest(new { message = "Username already exists!" });
            if(await EmailExistsAsync(userObj.Email))
                return BadRequest(new { message = "Email already exists!" });
            if (!ValidatePasswordStrength(userObj.Password).IsNullOrEmpty()) // Pasword strength validation
                return BadRequest(new { message = ValidatePasswordStrength(userObj.Password) });

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Username == userObj.Username);

            if (user != null)
                return BadRequest(new { message = "Username already exists!" });

            await _authContext.Users.AddAsync(new User
            {
                Email = userObj.Email,
                FirstName = userObj.FirstName,
                LastName = userObj.LastName,
                Password = PasswordHasher.Hash(userObj.Password),
                Username = userObj.Username,
                Role = "User",
                Token = ""
            });
            await _authContext.SaveChangesAsync();

            return Ok(new
            {
                message = "User registered successfully!"
            });
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<User>> GetAllUsersAsync()
        {
            return Ok(await _authContext.Users.ToListAsync());
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync(TokenApiDTO tokenObj)
        {
            if(tokenObj is null)
                return BadRequest(new { message = "Invalid Client Request" });

            var accessToken = tokenObj.AccessToken;
            var refreshToken = tokenObj.RefreshToken;
            var principal = GetPrincipalFromExpiredToken(accessToken);
            var username = principal.Identity?.Name;
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Username == username);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return BadRequest(new { message = "Invalid Client Request" });

            var newAccessToken = CreateJwtToken(user);
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _authContext.SaveChangesAsync();

            return Ok(new TokenApiDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("send-reset-email/{email}")]
        public async Task<IActionResult> SendResetEmailAsync(string email)
        {
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return BadRequest(new { message = "Email does not exist!" });

            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var emailToken = Convert.ToBase64String(tokenBytes);
            user.ResetPasswordToken = emailToken;
            user.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(15);

            var from = _configuration["EmailSettings:From"];
            var emailObj = new Email(email, "Password Reset", EmailBody.ResetPasswordEmailBody(email, emailToken));
            
            _emailService.SendEmail(emailObj);
            _authContext.Entry(user).State = EntityState.Modified;
            await _authContext.SaveChangesAsync();

            return Ok(new { message = "Reset password email sent successfully!" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordDTO resetPasswordObj)
        {
            var newToken = resetPasswordObj.EmailToken.Replace(" ", "+");
            var user = await _authContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == resetPasswordObj.Email && x.ResetPasswordToken == newToken);

            if (user == null)
                return BadRequest(new { message = "Email does not exist!" });

            var tokenCode = user.ResetPasswordToken;
            var tokenExpiry = user.ResetPasswordExpiry;

            if (tokenCode != resetPasswordObj.EmailToken || tokenExpiry < DateTime.UtcNow)
                return BadRequest(new { message = "Invalid reset link!" });

            user.Password = PasswordHasher.Hash(resetPasswordObj.NewPassword);
            _authContext.Entry(user).State = EntityState.Modified;
            await _authContext.SaveChangesAsync();

            return Ok(new { message = "Password reset successfully!" });
        }

        private string CreateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("supersecretkey.....................");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new(ClaimTypes.Role, user.Role),
                    new(ClaimTypes.Name, $"{user.Username}")
                }),
                Expires = DateTime.UtcNow.AddSeconds(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _authContext.Users.Any(x => x.RefreshToken == refreshToken);

            if (tokenInUser)
            {
                return CreateRefreshToken();
            }

            return refreshToken;
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("supersecretkey.....................")),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        private async Task<bool> UserNameExistsAsync(string username) => await _authContext.Users.AnyAsync(x => x.Username == username);

        private async Task<bool> EmailExistsAsync(string email) => await _authContext.Users.AnyAsync(x => x.Email == email);

        private string ValidatePasswordStrength(string password)
        {
            //Implement password strength validation
            StringBuilder sb = new();
            if(password.Length < 6)
            {
                sb.Append("Password must be at least 6 characters long. " + Environment.NewLine);
            }
            if(!password.Any(char.IsUpper))
            {
                sb.Append("Password must contain at least one uppercase letter. " + Environment.NewLine);
            }
            if(!password.Any(char.IsDigit))
            {
                sb.Append("Password must contain at least one digit. " + Environment.NewLine);
            }
            if(!lowerCaseAlphaRegex().IsMatch(password) && !upperCaseAlphaRegex().IsMatch(password))
            {
                sb.Append("Password must contain at least one letter or digit. " + Environment.NewLine);
            }

            return sb.ToString();
        }

        [GeneratedRegex(@"[a-z]")]
        private static partial Regex lowerCaseAlphaRegex();

        [GeneratedRegex(@"[A-Z]")]
        private static partial Regex upperCaseAlphaRegex();
    }
}
