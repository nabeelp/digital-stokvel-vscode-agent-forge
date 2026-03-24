using DigitalStokvel.API.Configuration;
using DigitalStokvel.Core.Interfaces;
using DigitalStokvel.Infrastructure;
using DigitalStokvel.Infrastructure.Data;
using DigitalStokvel.Infrastructure.Integrations;
using DigitalStokvel.Infrastructure.Repositories;
using DigitalStokvel.Services.Authentication;
using DigitalStokvel.Services.Authorization;
using DigitalStokvel.Services.Compliance;
using DigitalStokvel.Services.Contributions;
using DigitalStokvel.Services.Governance;
using DigitalStokvel.Services.Groups;
using DigitalStokvel.Services.Interest;
using DigitalStokvel.Services.Members;
using DigitalStokvel.Services.Notifications;
using DigitalStokvel.Services.Payouts;
using DigitalStokvel.Services.Security;
using DigitalStokvel.Services.Wallet;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models; // Temporarily commented out for migration generation
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Digital Stokvel Banking API");

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    // TODO: Configure Swagger properly for .NET 10 (temporarily disabled for migration generation)

    // Configure Database
    builder.Services.AddDbContext<DigitalStokvelDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null
            )
        )
    );

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowMobileAndWeb", policy =>
        {
            var allowedOrigins = new List<string>();
            
            if (builder.Environment.IsDevelopment())
            {
                allowedOrigins.AddRange(new[]
                {
                    "http://localhost:3000",
                    "http://localhost:8081",
                    "http://192.168.1.1:8081" // React Native
                });
            }
            else
            {
                var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
                if (origins != null)
                {
                    allowedOrigins.AddRange(origins);
                }
            }

            policy.WithOrigins(allowedOrigins.ToArray())
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // Configure Authentication (JWT Bearer)
    var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured. Set Jwt:Key in appsettings.json");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DigitalStokvel";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DigitalStokvelApp";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // HTTPS required in production (SP-04)
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // No tolerance for expired tokens (SP-14)
        };

        // Add event handlers for logging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Debug("JWT token validated for user: {User}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // Register FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // Application Insights
    builder.Services.AddApplicationInsightsTelemetry();

    // Register Repositories and Unit of Work
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IGroupRepository, GroupRepository>();
    builder.Services.AddScoped<IMemberRepository, MemberRepository>();
    builder.Services.AddScoped<IContributionRepository, ContributionRepository>();
    builder.Services.AddScoped<IPayoutRepository, PayoutRepository>();
    builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
    builder.Services.AddScoped<IVoteRepository, VoteRepository>();
    builder.Services.AddScoped<IDisputeRepository, DisputeRepository>();
    
    // Authentication repositories
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

    // Register Services
    builder.Services.AddScoped<IGroupService, GroupService>();
    builder.Services.AddScoped<IMemberService, MemberService>();
    builder.Services.AddScoped<IContributionService, ContributionService>();
    builder.Services.AddScoped<IPayoutService, PayoutService>();
    builder.Services.AddScoped<IWalletService, WalletService>();
    builder.Services.AddScoped<IInterestService, InterestService>();
    builder.Services.AddScoped<IGovernanceService, GovernanceService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    
    // Authentication and Security services
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
    builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
    builder.Services.AddScoped<IFraudDetectionService, FraudDetectionService>();
    builder.Services.AddScoped<IAuditService, AuditService>();
    builder.Services.AddScoped<IComplianceService, ComplianceService>();

    // Register External Integrations
    builder.Services.AddScoped<ICoreBankingClient, CoreBankingClient>();

    // Configure Problem Details
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital Stokvel Banking API v1");
            options.RoutePrefix = "swagger";
        });
    }

    // Global exception handler
    app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;

            Log.Error(exception, "Unhandled exception occurred");

            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred processing your request",
                Detail = app.Environment.IsDevelopment() ? exception?.Message : "An unexpected error occurred",
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        });
    });

    app.UseHttpsRedirection();
    app.UseCors("AllowMobileAndWeb");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Health check endpoints
    app.MapGet("/health", () => Results.Ok(new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow,
        version = "1.0.0"
    })).WithName("HealthCheck").WithTags("Health");

    app.MapGet("/health/ready", async (DigitalStokvelDbContext dbContext) =>
    {
        try
        {
            await dbContext.Database.CanConnectAsync();
            return Results.Ok(new
            {
                status = "ready",
                database = "connected",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Readiness check failed");
            return Results.Json(new
            {
                status = "not ready",
                database = "disconnected",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            }, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }).WithName("ReadinessCheck").WithTags("Health");

    // Database migration on startup (dev only)
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DigitalStokvelDbContext>();
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
{
    Log.CloseAndFlush();
}
