using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficerService.Data;
using OfficerService.IServices;
using OfficerService.Services;
using OfficerService.Settings;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//Settings
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<FarmerServiceOptions>(builder.Configuration.GetSection("ExternalServices:FarmerService"));

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be set and be at least 32 characters long.");
if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience) || jwt.ExpiryMinutes <= 0)
    throw new InvalidOperationException("Jwt:Issuer, Jwt:Audience and Jwt:ExpiryMinutes (greater than 0) must be set.");

//Controllers  
builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

//Database 
builder.Services.AddDbContext<OfficerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OfficerDb")));

//Authentication 
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();

// ---------- Application services ----------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOfficerProfileService, OfficerProfileService>();
builder.Services.AddScoped<IVerificationService, VerificationService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IComplianceRecordService, ComplianceRecordService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddHttpClient<IFarmerCallbackClient, FarmerCallbackClient>(client =>
    client.Timeout = TimeSpan.FromSeconds(10));

//OpenAPI + CORS 
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

//Turn service errors into clean JSON responses 
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (KeyNotFoundException ex) { await WriteError(context, StatusCodes.Status404NotFound, ex.Message); }
    catch (UnauthorizedAccessException ex) { await WriteError(context, StatusCodes.Status401Unauthorized, ex.Message); }
    catch (ForbiddenException ex) { await WriteError(context, StatusCodes.Status403Forbidden, ex.Message); }
    catch (ConflictException ex) { await WriteError(context, StatusCodes.Status409Conflict, ex.Message); }
    catch (InvalidOperationException ex) { await WriteError(context, StatusCodes.Status400BadRequest, ex.Message); }
});

static Task WriteError(HttpContext context, int statusCode, string message)
{
    if (context.Response.HasStarted)
        return Task.CompletedTask;

    context.Response.Clear();
    context.Response.StatusCode = statusCode;
    return context.Response.WriteAsJsonAsync(new { error = message });
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("AgriConnect - Officer Service"));
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
