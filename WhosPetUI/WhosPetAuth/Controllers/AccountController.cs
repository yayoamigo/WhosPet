using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WhosPetCore.DTO.Incoming.Auth;
using WhosPetCore.Domain.Indentity;
using WhosPetCore.Domain.Entities.Auth;
using WhosPetCore.Domain.Entities;
using AlertaPatitasAPIUI.Helpers;
using WhosPetCore.Helpers;
using WhosPetCore.Domain.ServiceContracts;

namespace WhosPetAuth.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly string _connectionString;
        private readonly IUserRoleService _roleService;
        private readonly IRefreshTokenService _refreshTokenService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config,
            TokenValidationParameters tokenValidationParameters,
            IRefreshTokenService refreshTokenService,
            IUserRoleService userRoleService


            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _tokenValidationParameters = tokenValidationParameters;
            _connectionString = config.GetConnectionString("WhosPet");
            _roleService = userRoleService;
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string> { "Invalid payload" }
                });
            }

            var userExist = await _userManager.FindByEmailAsync(model.Email);

            if (userExist != null)
            {
                return BadRequest(new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string> { "User already exists" }
                });
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Profile = new UserProfile
                {
                    Email = model.Email,
                    Name = model.Name,
                    Surname = model.Surname,
                    City = model.City,
                    Address = model.Address
                }
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

                await _userManager.AddClaimsAsync(user, claims);
                await _userManager.AddToRoleAsync(user, "User");
                var jwt = new JwtTokenGenerator(_roleService, _refreshTokenService);
                var responseResult = await jwt.GenerateJwtToken(user, _config, _connectionString);

                return Ok(responseResult);
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            var ErrorResponse = new AuthResponse()
            {
                Result = false,
                Errors = errors
            };

            return BadRequest(ErrorResponse);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string> { "Invalid payload" }
                });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string> { "User doesn't exist" }
                });
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var claims = await _userManager.GetClaimsAsync(user);

                if (!claims.Any())
                {
                    claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email)
                    };
                    await _userManager.AddClaimsAsync(user, claims);
                }
                var jwt = new JwtTokenGenerator(_roleService, _refreshTokenService);

                var responseResult = await jwt.GenerateJwtToken(user, _config, _connectionString);
                return Ok(responseResult);
            }

            if (result.IsLockedOut)
            {
                return BadRequest(new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string> { "User account locked" }
                });
            }

            var ErrorResponse = new AuthResponse()
            {
                Result = false,
                Errors = new List<string> { "Invalid credentials" }
            };
            return Unauthorized(ErrorResponse);
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string> { "Invalid payload" }
                });
            }
            var JwtHelper = new JwtHelper(_tokenValidationParameters, _connectionString, _roleService, _refreshTokenService, _userManager, _signInManager, _config);

            var result = await JwtHelper.VeryfyAndGenerateToken(model);

            if (result.Errors != null)
            {
                return BadRequest(new AuthResponse()
                {
                    Result = false,
                    Errors = result.Errors
                });
            }

            return Ok(result);
        }

       

    }
}
