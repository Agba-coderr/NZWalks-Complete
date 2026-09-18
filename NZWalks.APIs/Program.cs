using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NZWalks.Infrastructure.Data;
using NZWalks.Infrastructure.Repositories;
using NZWalks.Infrastructure.Services;
using NZWalks.Application.Mappings;
using NZWalks.Application.DTOs;
using NZWalks.Application.Interfaces.Repositories;
using NZWalks.Application.Interfaces.Services;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using NZWalks.Application.Common;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Information()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddControllers()
     .AddJsonOptions(options =>
     {
         options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
     });
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NZ Walks API",
        Version = "v1"
    });

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference(
                JwtBearerDefaults.AuthenticationScheme,
                document
            ),
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<NZWalksDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NZWalksConnectionString")));

builder.Services.AddDbContext<NZWalksAuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NZWalksAuthConnectionString")));

builder.Services.AddScoped<IRegionRepository, SQLRegionRepository>();

builder.Services.AddScoped<IWalkRepository, SQLWalkRepository>();

builder.Services.AddScoped<ITokenRepository, TokenRepository>();

builder.Services.AddScoped<IImageRepository, LocalImageRepository>();

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>("NZWalks")
    .AddEntityFrameworkStores<NZWalksAuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key configuration is missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var result = Result.Failure("Authentication required. Please log in to continue. If you do not have an account, please create one.", StatusCodes.Status401Unauthorized);

                await context.Response.WriteAsJsonAsync(result);
            },

            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                var result = Result.Failure("Access denied. You do not have permission to perform this action.", StatusCodes.Status403Forbidden);
                await context.Response.WriteAsJsonAsync(result);
            }
        };
    });

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AutoMapperProfiles).Assembly));

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto |
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedHost
});

if (app.Environment.IsDevelopment()) //middleware for development environment
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Images")),
    RequestPath = "/Images"
});

app.MapControllers();

app.Run();