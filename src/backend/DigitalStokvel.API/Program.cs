using DigitalStokvel.API.Configuration;
using DigitalStokvel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

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
    
    // Swagger configuration - TODO: Configure OpenAPI documentation
    // builder.Services.AddSwaggerGen();

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
            policy.WithOrigins(
                    builder.Configuration["AllowedOrigins:Mobile"] ?? "*",
                    builder.Configuration["AllowedOrigins:Web"] ?? "*"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // Configure Authentication (JWT Bearer)
    // TODO: Implement JWT authentication configuration

    // Application Insights
    builder.Services.AddApplicationInsightsTelemetry();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        // app.UseSwagger();
        // app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowMobileAndWeb");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow,
        version = "1.0.0"
    })).WithName("HealthCheck");

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
