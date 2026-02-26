using CRM.Backend.Infra.Auth;

namespace CRM.Backend.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/token", (TokenRequest request, JwtService jwtService, IWebHostEnvironment env) =>
        {
            if (!env.IsDevelopment())
                return Results.Problem("Demo auth endpoint is only available in Development.", statusCode: 403);

            if (request.Username == "admin" && request.Password == "admin123")
            {
                var token = jwtService.GenerateToken("user-001", "admin@crm.com", "Admin User");
                return Results.Ok(new { token });
            }
            return Results.Unauthorized();
        })
        .WithName("GetToken")
        .WithTags("Auth")
        .AllowAnonymous();
    }
}

public record TokenRequest(string Username, string Password);