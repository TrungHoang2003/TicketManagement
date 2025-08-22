using System.Text;
using Application;
using BuildingBlocks.Commons;
using BuildingBlocks.EmailHelper;
using DotNetEnv;
using Infrastructure;
using Scalar.AspNetCore;
using Serilog;
using TicketManagement.Api.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var envFile = environment switch
{
    "Development" => ".env",
    "Docker" => ".env.docker",
    "Production" => ".env.production",
    _ => ".env"
};

var envPath = Path.Combine(Directory.GetCurrentDirectory(), envFile);
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
// Get connection string from configuration
var postgresConnectionStr= builder.Configuration["ConnectionStrings:DefaultConnection"];
var redisConnectionStr = builder.Configuration["ConnectionStrings:Redis"];

// Add Infrastructure layer with PostgreSQL configuration
builder.Services.AddInfrastructure(postgresConnectionStr, redisConnectionStr);

// Add Application layer
 builder.Services.AddApplication(builder.Configuration);

// Add Controllers
builder.Services.AddControllers();

// Add HttpContextAccessor for accessing current user
builder.Services.AddHttpContextAccessor();

// Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration) // Đọc cấu hình từ appsettings.json
    .ReadFrom.Services(services) // Cho phép DI cho các enricher/sinks nếu cần
);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]
                                   ?? throw new BusinessException("JWT secret is not configured"))),
        ValidateIssuer = false, // Set to true in production
        ValidateAudience = false, // Set to true in production
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HeadOfItOnly", policy => policy.RequireRole("Head Of IT"));
    options.AddPolicy("HeadOfQaOnly", policy=>policy.RequireRole("Head Of QA"));
    options.AddPolicy("AdminOnly", policy=>policy.RequireRole("Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlerMiddleWare>();
//app.UseMiddleware<TokenValidateMiddleware>();
// Add Serilog request logging
app.UseSerilogRequestLogging();
app.MapControllers();

try
{
    Log.Information("Starting Application...");
    Log.Information("Using connection string: {ConnectionString}", postgresConnectionStr);
    Log.Information("Using Redis connection string: {RedisConnectionString}", redisConnectionStr);
    Log.Information("Environment: {Environment}", environment);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush(); //Đảm bảo tất cả các log đang chờ được đẩy đi trước khi ứng dụng tắt
}