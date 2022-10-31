using AuthAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthAPI.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AuthDBContext _dbContext;
        private readonly JWTSetting _jWTSetting;
        private readonly IRefreshTokenGenerator tokenGenerator;
        public UserController(AuthDBContext dbContext, IOptions<JWTSetting> jWTSetting, IRefreshTokenGenerator _refreshToken)
        {
            _dbContext = dbContext;
            _jWTSetting = jWTSetting.Value;
            tokenGenerator = _refreshToken;
        }

        [NonAction]
        public TokenResponse Authenticate(string username, Claim[] claims)
        {
            TokenResponse tokenResponse = new TokenResponse();
            var tokenkey = Encoding.UTF8.GetBytes(_jWTSetting.securityKey);
            var tokenhandler = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                 signingCredentials: new SigningCredentials(new SymmetricSecurityKey(tokenkey), SecurityAlgorithms.HmacSha256)

                );
            tokenResponse.JWTToken = new JwtSecurityTokenHandler().WriteToken(tokenhandler);
            tokenResponse.RefreshToken = tokenGenerator.GenerateToken(username);

            return tokenResponse;
        }


        [Route("Authenticate")]
        [HttpPost]
        public IActionResult Authenticate([FromBody] userced user)
        {
            TokenResponse tokenresponse = new TokenResponse();
            var _user = _dbContext.TblUser.FirstOrDefault(o => o.UserId == user.username &&
                                                                    o.Password == user.password && o.IsActive == true);
            if (_user == null)
                return Unauthorized();

            var tokenhandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_jWTSetting.securityKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                        new Claim[]
                        {
                            new Claim(ClaimTypes.Name, _user.UserId),
                            new Claim(ClaimTypes.Role, _user.Role),


                        }
                ),
                Expires = DateTime.Now.AddMinutes(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey),
                                                           SecurityAlgorithms.HmacSha256)
            };
            var token = tokenhandler.CreateToken(tokenDescriptor);
            string finaltoken = tokenhandler.WriteToken(token);

            tokenresponse.JWTToken = finaltoken;
            tokenresponse.RefreshToken = tokenGenerator.GenerateToken(user.username);
            return Ok(tokenresponse);
        }

        [Route("Refresh")]
        [HttpPost]
        public IActionResult Refresh([FromBody] TokenResponse token)
        {
            var tokenhandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;


            var principle = tokenhandler.ValidateToken(token.JWTToken, new TokenValidationParameters
            {

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jWTSetting.securityKey)),
                ValidateIssuer = false,
                ValidateAudience = false

            }, out securityToken);
            var _token = securityToken as JwtSecurityToken;

            if (_token != null && !_token.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
            {
                return Unauthorized();
            }
            var username = principle.Identity.Name;
            var _reftable = _dbContext.TblRefreshtoken.FirstOrDefault(o => o.UserId == username && o.RefreshToken == token.RefreshToken);
            if (_reftable == null)
            {
                return Unauthorized();
            }
            TokenResponse _resutl = Authenticate(username, principle.Claims.ToArray());
            return Ok(_resutl);
        }
        [Route("GetMenubyRole/{role}")]
        [HttpGet]
        public IActionResult GetMenubyRole(string role)
        {
            var _result = (from q1 in _dbContext.TblPermission.Where(item => item.RoleId == role)
                           join q2 in _dbContext.TblMenu
                           on q1.MenuId equals q2.Id
                           select new { q1.MenuId, q2.Name, q2.LinkName }).ToList();
            // var _result = _dbContext.TblPermission.Where(o => o.RoleId == role).ToList();

            return Ok(_result);
        }

        [Route("HaveAccess")]
        [HttpGet]
        public IActionResult HaveAccess(string role, string menu)
        {
            APIResponse result = new APIResponse();
            //var username = principal.Identity.Name;
            var _result = _dbContext.TblPermission.Where(o => o.RoleId == role && o.MenuId == menu).FirstOrDefault();
            if (_result != null)
            {
                result.result = "pass";
            }
            return Ok(result);
        }

        [Route("GetAllRole")]
        [HttpGet]
        public IActionResult GetAllRole()
        {
            var _result = _dbContext.TblRole.ToList();
            // var _result = _dbContext.TblPermission.Where(o => o.RoleId == role).ToList();

            return Ok(_result);
        }

        [HttpPost("Register")]
        public APIResponse Register([FromBody] TblUser value)
        {
            string result = string.Empty;
            try
            {
                var _emp = _dbContext.TblUser.FirstOrDefault(o => o.UserId == value.UserId);
                if (_emp != null)
                {
                    result = string.Empty;
                }
                else
                {
                    TblUser tblUser = new TblUser()
                    {
                        Name = value.Name,
                        Email = value.Email,
                        UserId = value.UserId,
                        Role = string.Empty,
                        Password = value.Password,
                        IsActive = false
                    };
                    _dbContext.TblUser.Add(tblUser);
                    _dbContext.SaveChanges();
                    result = "pass";
                }
            }
            catch (Exception ex)
            {
                result = string.Empty;
            }
            return new APIResponse { keycode = string.Empty, result = result };
        }

    }
}
