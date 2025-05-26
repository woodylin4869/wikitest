using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using H1_ThirdPartyWalletAPI.Model.OneWalletGame;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Code;

namespace H1_ThirdPartyWalletAPI.Service.Common
{
    public class JWTService
    {
        public JWTService()
        {
        }
        public string GenerateToken(string systemCode, string webId, string memberAccount, DateTime expiredDateTime, OW_RCG.TokenType tokenType = OW_RCG.TokenType.AuthToken)
        {
            var client = Config.CompanyToken.RCG_Token;
            var secret = Config.CompanyToken.RCG_Secret;

            var claims = new List<Claim>();
            claims.Add(new Claim("systemcode", systemCode, "string"));
            claims.Add(new Claim("webid", webId, "string"));
            claims.Add(new Claim("memberaccount", memberAccount, "string"));
            claims.Add(new Claim("tokentype", tokenType.ToString(), "string"));


            var userClaimsIdentity = new ClaimsIdentity(claims);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes($"{client}{secret}"));

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //Issuer = "01Test00010",
                Subject = userClaimsIdentity,
                Expires = expiredDateTime,
                SigningCredentials = signingCredentials,
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var serializeToken = tokenHandler.WriteToken(securityToken);

            return serializeToken;
        }
        public string GenerateRcgJwtToken(string systemCode, string webId, string memberAccount, DateTime expiredDateTime, OW_RCG.TokenType tokenType = OW_RCG.TokenType.AuthToken)
        {
            var claims = new List<Claim>
                {
                    //new Claim(JwtRegisteredClaimNames.Email, ""),
                    //new Claim("FullName", ""),
                    new Claim(JwtRegisteredClaimNames.NameId, memberAccount),
                    new Claim("systemcode", systemCode, "string"),
                    new Claim("webid", webId, "string"),
                    new Claim("memberaccount", memberAccount, "string"),
                    new Claim("tokentype", tokenType.ToString(), "string")
                };

            claims.Add(new Claim(ClaimTypes.Role, nameof(Platform.RCG)));
            var userClaimsIdentity = new ClaimsIdentity(claims);
            var key = Encoding.UTF8.GetBytes(Config.JWT.KEY);
            var securityKey = new SymmetricSecurityKey(key);

            var jwt = new JwtSecurityToken
            (
                issuer: Config.JWT.Issuer,
                audience: Config.JWT.Audience,
                claims: claims,
                expires: expiredDateTime,
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return token;
        }
        public string GenerateJwtToken(string role,DateTime expiredDateTime)
        {
            var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Email, ""),
                    new Claim("FullName", ""),
                    new Claim(JwtRegisteredClaimNames.NameId, ""),
                };

            claims.Add(new Claim(ClaimTypes.Role, role));
            //var userClaimsIdentity = new ClaimsIdentity(claims);
            var key = Encoding.UTF8.GetBytes(Config.JWT.KEY);
            var securityKey = new SymmetricSecurityKey(key);

            var jwt = new JwtSecurityToken
            (
                issuer: Config.JWT.Issuer,
                audience: Config.JWT.Audience,
                claims: claims,
                expires: expiredDateTime,
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return token;
        }

        public JwtSecurityToken DecodeToken(HttpContext context)
        {
            var headers = context.Request.Headers;
            if (!headers.Keys.Any(x => x == "Authorization"))
            {

            }
            var Authorization = headers["Authorization"].ToString();
            var Token = Authorization.Substring(Authorization.IndexOf("Bearer ") + 7);

            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(Token);

            return jwtToken;
        }
    }
}

