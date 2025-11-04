using System.Text;
using day11.Application;
using day11.Infrastructure;
using day11.Infrastructure.Jwt;
using day11.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// 1️⃣ Register MongoDB Settings
// ===============================
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<MongoDbSettings>(sp =>
    builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>()
    ?? new MongoDbSettings());

// ===============================
// 2️⃣ Register Layers (DI)
// ===============================
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// ===============================
// 3️⃣ JWT Authentication Config
// ===============================
var jwtSettings = new JwtSettings();
builder.Services.AddSingleton(jwtSettings);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(jwtSettings.Key);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// ===============================
// 4️⃣ Core ASP.NET services
// ===============================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===============================
// 5️⃣ Build app pipeline
// ===============================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
