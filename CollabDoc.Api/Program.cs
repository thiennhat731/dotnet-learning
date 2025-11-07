using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CollabDoc.Realtime.Hubs;
using CollabDoc.Infrastructure.Mappings;
using CollabDoc.Api.Middlewares;
using System.Text.Json;
using CollabDoc.Application.Common;
using CollabDoc.Application;
using CollabDoc.Infrastructure;
using CollabDoc.Realtime;
using CollabDoc.Infrastructure.Settings;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// =====================
// 1️⃣ DEPENDENCY INJECTION
// =====================
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRealtime();

// =====================
// 2️⃣ JWT AUTHENTICATION CONFIG với Options Pattern
// =====================
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var serviceProvider = builder.Services.BuildServiceProvider();
        var jwtSettings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/collab"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var response = new ApiResponse<string>(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized. Token missing or invalid.",
                    null,
                    "Unauthorized"
                );

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return context.Response.WriteAsync(json);
            },

            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var response = new ApiResponse<string>(
                    StatusCodes.Status403Forbidden,
                    "Forbidden. You do not have permission to access this resource.",
                    null,
                    "Forbidden"
                );

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return context.Response.WriteAsync(json);
            }
        };
    });

// =====================
// 3️⃣ BASIC SERVICES
// =====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// =====================
// =====================
// 4️⃣ CORS CONFIG với Options Pattern
// =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        var corsSettings = builder.Configuration.GetSection("CorsSettings").Get<CorsSettings>();

        policy.WithOrigins(corsSettings.AllowedOrigins)
              .WithMethods(corsSettings.AllowedMethods)
              .WithHeaders(corsSettings.AllowedHeaders.Any() ? corsSettings.AllowedHeaders : new[] { "*" });

        if (corsSettings.AllowCredentials)
        {
            policy.AllowCredentials();
        }
    });
});
var app = builder.Build();

// =====================
// 5️⃣ INITIALIZE MONGODB MAPS
// =====================
CollabDoc.Infrastructure.DependencyInjection.InitializeMongoMaps();

// =====================
// 6️⃣ MIDDLEWARE PIPELINE
// =====================
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<FormatResponseMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<CollabHub>("/hubs/collab");
app.MapControllers();

app.Run();
