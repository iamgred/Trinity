//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TeamServer.Repositories;
using Trinity.Shared.DTOs.Authentication;

namespace TeamServer.Controllers
{
    [AllowAnonymous]
    [Route(Routes.Auth)]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly OperatorRepository _operators;
        private readonly CampaignBridgeRepository _campaignBridge;
        private readonly IConfiguration _config;

        public AuthController(OperatorRepository operators, CampaignBridgeRepository campaignBridge, IConfiguration config)
        {
            _operators = operators;
            _campaignBridge = campaignBridge;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var op = await _operators.GetByUsernameAsync(dto.Username);

            // TODO: replace plain-text comparison with BCrypt.Verify once passwords are hashed
            if (op is null || op.Password != dto.Password)
                return Unauthorized();

            var campaigns = await _campaignBridge.GetCampaignsByOperatorIdAsync(op.ID);
            var campaignId = campaigns.FirstOrDefault()?.ID;

            var token = BuildToken(op.Username, campaignId);
            return Ok(new LoginResponse { Token = token, Username = op.Username });
        }

        private string BuildToken(string username, int? campaignId)
        {
            var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry  = DateTime.UtcNow.AddHours(_config.GetValue<double>("Jwt:ExpiryHours", 8));

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
            };

            if (campaignId is int id)
                claims.Add(new Claim("campaignId", id.ToString()));

            var jwt = new JwtSecurityToken(
                issuer:             _config["Jwt:Issuer"],
                audience:           _config["Jwt:Audience"],
                claims:             claims,
                expires:            expiry,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//