using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CollabDoc.Application.Security;
using CollabDoc.Application.Services;
using CollabDoc.Realtime.Hubs;
using CollabDoc.Application.Interfaces;
using CollabDoc.Infrastructure.Settings;
using CollabDoc.Infrastructure.Mappings;
using CollabDoc.Api.Middlewares;
using System.Text.Json;
using CollabDoc.Application.Common;
using CollabDoc.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// =====================
// 1️⃣ CONFIGURATION
// =====================
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

var mongoSettings = builder.Configuration
    .GetSection("MongoDbSettings")
    .Get<MongoDbSettings>()
    ?? throw new InvalidOperationException("❌ MongoDB settings not found in configuration.");

builder.Services.AddSingleton(mongoSettings);

// =====================
// 2️⃣ DEPENDENCY INJECTION
// =====================
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddSignalR();
builder.Logging.AddConsole();

// =====================
// 3️⃣ JWT AUTHENTICATION CONFIG
// =====================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey missing.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // ✅ Gộp tất cả event vào chung
    options.Events = new JwtBearerEvents
    {
        // Cho phép SignalR lấy token từ query string
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

        // Xử lý lỗi Unauthorized
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

        // Xử lý lỗi Forbidden
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
// 4️⃣ BASIC SERVICES + CORS CONFIG
// =====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ✅ CORS Config (đặt trước builder.Build())
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// =====================
// 5️⃣ REGISTER CLASS MAPS
// =====================
DocumentMap.RegisterClassMap();
UserMap.RegisterClassMap();

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

// ⚡ Bật CORS trước Authentication
app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<CollabHub>("/hubs/collab");
// app.UseHttpsRedirection();
app.MapControllers();
app.Run();
