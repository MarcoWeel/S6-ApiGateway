using Ocelot.Middleware;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using API_Gateway.Models;

namespace API_Gateway.Helpers;

public static class JwtMiddleware
{
    public static Func<HttpContext, string, Func<Task>, Task> CreateAuthorizationFilter
        => async (context, securityKey, next) =>
        {
            var routeClaimsRequirements = context.Items.DownstreamRoute().RouteClaimsRequirement;
            if (routeClaimsRequirements.Count > 0)
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if(token != null && Validate(routeClaimsRequirements, token, securityKey))
                {
                    await next.Invoke();
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Items.SetError(new UnauthenticatedError("Unauthorized"));
                }
            }
            else
            {
                await next.Invoke();
            }
        };

    public static bool Validate(Dictionary<string, string> routeClaimsRequirements, string token, string securityKey)
    {
        // validate token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(securityKey);
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;

        // validate roles
        var userRoles = (Roles)int.Parse(jwtToken.Claims.First(x => x.Type == "role").Value);
        var requirementsRoles = routeClaimsRequirements["Role"].Replace(" ", "").Split(",");

        foreach (var requirementRole in requirementsRoles)
            if(Enum.TryParse(requirementRole, out Roles role))
                return true;

        return false;
    }
}